using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using TestApp2.Models.SQLite;
using System.Collections.ObjectModel;
using TestApp2.ViewModels;
using Plugin.Media;
using Acr.UserDialogs;
using TestApp2.Common.Helper;
using System.Net.Http;
using Newtonsoft.Json;
using TestApp2.Common.APIclasses;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Globalization;
using TestApp2.Common;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinePage : ContentPage
    {
        public List<string> AttachmentList { get; set; } = new List<string>();

        List<LineItems> list = new List<LineItems>();
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string objSiteRef { get; set; }
        public string objDescription { get; set; }

        private SQLiteConnection con;
        public LinePage()
        {
            InitializeComponent();

        }
        
        public LinePage(string objContId, string objLineNum, string objSitRef)
        {
            InitializeComponent();
            MessagingCenter.Subscribe<WorkOrderPage>(this, "Hi", (sender) =>
            {
                GetContractLineDetails();
            });
            con = DependencyService.Get<ISQLite>().GetConnection();
            
            objContractId = objContId;
            objLineNumber = objLineNum;
            objSiteRef = objSitRef;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetContractLineDetails();
            }
            else
            {
                GetLineDataFromLocalDB();
            }
        }
        //Get the data from local DB 
        public void GetLineDataFromLocalDB()
        {
            var isContractLineExists = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == objLineNumber).Count();
            if (isContractLineExists > 0)
            {
                var data = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == objLineNumber).FirstOrDefault();
                LineNum.Text = data.Cont_line;
                SerialNum.Text = data.ser_num;
                Item.Text = data.item;
                UM.Text = data.u_m;
                objDescription = data.description;
                Description.Text = objDescription;
                Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                LeadPartner.Text = LoginStorage.PartnerId;
                LineStatusList.Text = data.lineStatus;
                BillingFrequency.Text = GetBillingFrequency(data.Billing_freq);
                if (data.rate_conv != null)
                {
                    Rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.rate_conv));
                }
                Unit_of_rate.Text = GetBillingFrequency(data.unit_of_rate);
                if (data.prorate_rate_conv != null)
                {
                    Prorate_rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.prorate_rate_conv));
                }
                Prorate_unit_of_rate.Text = GetBillingFrequency(data.prorate_unit_of_rate);
                if (data.start_date != null)
                {
                    txtStartDate.Text = DateTime.ParseExact(data.start_date.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                }
                else
                    txtStartDate.Text = "";

                if (data.end_date != null)
                {
                    txtEndDate.Text = DateTime.ParseExact(data.end_date.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                }
                else
                    txtEndDate.Text = "";

                if (data.checkInDate != null)
                {
                    txtCheckInDate.Text = DateTime.ParseExact(data.checkInDate.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                }
                else
                    txtCheckInDate.Text = "";

                if (data.checkOutDate != null)
                {
                    txtCheckOutDate.Text = DateTime.ParseExact(data.checkOutDate.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                }
                else
                    txtCheckOutDate.Text = "";

            }
        }
        public void GetContractLineDetails()
        {
            try
            {
                LeadPartner.Text = LoginStorage.PartnerId;
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl +"/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContractLinesDetailsByContractIdSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Items");
                    string jtokenStr = res.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        JArray array = JArray.Parse(jtokenStr);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);

                            var data = JsonConvert.DeserializeObject<LineData>(jtokenString);
                            LineNum.Text = data.cont_line;
                            UM.Text = data.u_m;
                            SerialNum.Text = data.ser_num;
                            Item.Text = data.item;
                            objDescription = data.description;
                            Description.Text = objDescription;
                            Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                            var _lineStatus = GetLineStatus(data.lineStatus);
                            LineStatusList.Text = data.lineStatus;
                            BillingFrequency.Text = GetBillingFrequency(data.Billingfreq);
                            if (data.rate_conv != null)
                            {
                                Rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.rate_conv));
                            }
                            Unit_of_rate.Text = GetBillingFrequency(data.UnitOfRate);
                            if (data.prorate_rate_conv != null)
                            {
                                Prorate_rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.prorate_rate_conv));
                            }
                            Prorate_unit_of_rate.Text = GetBillingFrequency(data.prorate_unit_of_rate);
                            if (data.start_date != null)
                            {
                                //txtStartDate.Text = data.start_date?.ToString("MM/dd/yyyy hh:mm:ss tt");
                                txtStartDate.Text = DateTimeDataConverter.StringtoDateTime(data.start_date);
                            }
                            else
                                txtStartDate.Text = "";

                            if (data.end_date != null)
                            {
                                //txtEndDate.Text = data.end_date?.ToString("MM/dd/yyyy hh:mm:ss tt");

                                txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data.end_date);
                            }
                            else
                                txtEndDate.Text = "";

                            if (data.checkInDate != null)
                            {
                                //txtCheckInDate.Text = data.checkInDate?.ToString("MM/dd/yyyy hh:mm:ss tt");

                                txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkInDate); ;
                            }
                            else
                                txtCheckInDate.Text = "";

                            if (data.checkOutDate != null)
                            {
                                //txtCheckOutDate.Text = data.checkOutDate?.ToString("MM/dd/yyyy hh:mm:ss tt");

                                txtCheckOutDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkOutDate); ;
                            }
                            else
                                txtCheckOutDate.Text = "";

                            items.ItemName = data.item;
                            items.LineNum = data.cont_line;
                            items.Quantity = data.qty?.Split('.').FirstOrDefault();
                            items.Unit = data.ser_num;
                            list.Add(items);

                            //ContractLineSiteData contractLineDetails = new ContractLineSiteData();
                            //var isContractLineExists = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == data.cont_line).Count();
                            //if (isContractLineExists == 0)
                            //{
                            //    contractLineDetails.Contract = objContractId;
                            //    contractLineDetails.Cont_line = data.cont_line;
                            //    contractLineDetails.description = data.description;
                            //    contractLineDetails.start_date = data.start_date;
                            //    contractLineDetails.end_date = data.end_date;
                            //    contractLineDetails.checkInDate = data.checkInDate;
                            //    contractLineDetails.checkOutDate = data.checkOutDate;
                            //    contractLineDetails.Billing_freq = data.Billingfreq;
                            //    contractLineDetails.ser_num = data.ser_num;
                            //    contractLineDetails.item = data.item;
                            //    contractLineDetails.qty = data.qty;
                            //    contractLineDetails.rate_conv = data.rate_conv;
                            //    contractLineDetails.unit_of_rate = data.UnitOfRate;
                            //    contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
                            //    contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
                            //    con.Insert(contractLineDetails);
                            //}
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "Ok");
            }
        }
        public string GetBillingFrequency(string objBillingFreq)
        {
            string status;
            switch (objBillingFreq)
            {
                case "Y":
                    status = "Yearly";
                    break;
                case "M":
                    status = "Monthly";
                    break;
                case "B":
                    status = "BiMonthly";
                    break;
                case "W":
                    status = "Weekly";
                    break;
                case "2":
                    status = "28 Days";
                    break;
                case "D":
                    status = "Days";
                    break;
                case "H":
                    status = "Hours";
                    break;
                default:
                    status = "";
                    break;

            }
            return status;
        }
        public string GetLineStatus(string objLineStatus)
        {
            string status = "";
            switch (objLineStatus)
            {
                case "0":
                    status = "Open";
                    break;
                case "1":
                    status = "Closed";
                    break;
                case "2":
                    status = "Delivered";
                    break;
                case "3":
                    status = "Hold";
                    break;
                case "4":
                    status = "Picked";
                    break;
                case "5":
                    status = "Packed";
                    break;
                case "6":
                    status = "Shipped";
                    break;
                case "7":
                    status = "Pickup";
                    break;
                case "8":
                    status = "Estimate";
                    break;
                default:
                    break;
            }
            return status;
        }
        private void LineMenuClicked(object sender, EventArgs e)
        {

        }
        private void ClockClicked(object sender, EventArgs e)
        {
            if (!(ClockedOnText.IsVisible && ClockedOnTextLabel.IsVisible))
            {
                ClockedOnText.Text = DateTime.Now.ToString();
                ClockedOnTextLabel.IsVisible = true;
                ClockedOnText.IsVisible = true;
                clockId.Source = "clockicongreen.jpg";
            }
            else if (ClockedOnTextLabel.IsVisible && ClockedOnText.IsVisible)
            {
                ClockedOnOptions();
            }
        }
        private async void ClockedOnOptions()
        {
            var action = await DisplayActionSheet("Please select how to proceed with current clock on: ", "Cancel", null, "Clock Off", "Clear Clock On", "Remain Clocked On");

            if (action != null)
            {
                switch (action)
                {
                    case "Clock Off":
                        break;

                    case "Clear Clock On":
                        ClockedOnText.IsVisible = false;
                        ClockedOnTextLabel.IsVisible = false;
                        clockId.Source = "clockiconorange.jpg";
                        ClockFrame.IsVisible = false;
                        break;

                    case "Remain Clocked On":
                        break;

                    default:
                        break;
                }
            }
        }
        private async void CalendarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Notes());
        }
        private async void CameraClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync
                (new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                    Directory = "Pictures",
                    Name = "test.jpg"
                }); ;

            if (file == null)
                return;
            AttachmentList.Add(file.Path);
        }
        private async void GalleryClicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Warning", "Cannot select image", "Cancel");
                return;
            }
            var pickedphoto = new Plugin.Media.Abstractions.PickMediaOptions()
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            };
            var selectedimage = await CrossMedia.Current.PickPhotoAsync(pickedphoto);
            if (selectedimage == null)
            {
                await DisplayAlert("Warning", "Could not load the image, try again!", "Cancel");
                return;
            }
            AttachmentList.Add(selectedimage.Path);
        }
        //private async void AttachmentClicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new AttachmentsPage(AttachmentList));
        //}
        private async void RentalContractDetails(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            //await Navigation.PushAsync(new RentalContractPage(objContractId));
            UserDialogs.Instance.HideLoading();
        }
        private async void NotepadClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkOrderPage(list));
        }
        private void UnitClicked(object sender, EventArgs e)
        {

        }
    }
}
