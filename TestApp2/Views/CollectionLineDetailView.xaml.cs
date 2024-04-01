using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using SQLite;
using TestApp2.Common;
using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TestApp2.Views
{
    public partial class CollectionLineDetailView : ContentPage
    {
        public List<string> AttachmentList { get; set; } = new List<string>();

        List<LineItems> list = new List<LineItems>();
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string objSiteRef { get; set; }
        public string objDescription { get; set; }
        private SQLiteConnection con;

        public CollectionLineDetailView()
        {
            InitializeComponent();
        }
        public CollectionLineDetailView(string objContId, string objLineNum, string objSitRef)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            
            objContractId = objContId;
            objLineNumber = objLineNum;
            objSiteRef = objSitRef;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetCollectionLineDetails();
            }
            else
            {
                GetCollectionLineDetailsFromLocalDB();
            }
        }
        public void GetCollectionLineDetailsFromLocalDB()
        {
            ObservableCollection<CollectionLineDetailsTable> collectionLinesListDetails = new ObservableCollection<CollectionLineDetailsTable>();
            collectionLinesListDetails.Clear();
            var isApptExists = con.Table<CollectionLineDetailsTable>().Where(a => a.Contract == objContractId && a.Cont_line == objLineNumber).Count();
            var appts = con.Table<CollectionLineDetailsTable>().Where(a => a.Contract == objContractId && a.Cont_line == objLineNumber).FirstOrDefault();

            if (isApptExists > 0)
            {
                LineItems items = new LineItems();

                var data = con.Table<CollectionLineDetailsTable>().Where(a => a.Contract == objContractId && a.Cont_line == objLineNumber).FirstOrDefault();
                UM.Text = data.u_m;
                LineNum.Text = data.Cont_line;
                SerialNum.Text = data.ser_num;
                Item.Text = data.item;
                objDescription = data.description;
                Description.Text = objDescription;
                Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                //get lineStatus with value
                LineStatusList.Text = data.lineStatus;
                //get BillingFrequency with value
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
                    txtStartDate.Text = DateTimeDataConverter.StringtoDateTime(data?.start_date);
                }
                else
                    txtStartDate.Text = "";

                if (data.end_date != null)
                {
                    txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data?.end_date);
                }
                else
                    txtEndDate.Text = "";

                if (data.checkInDate != null)
                {
                    txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data?.checkInDate);
                }
                else
                    txtCheckInDate.Text = "";

                if (data.checkOutDate != null)
                {
                    txtCheckOutDate.Text = DateTimeDataConverter.StringtoDateTime(data?.checkOutDate);
                }
                else
                    txtCheckOutDate.Text = "";

                items.ItemName = data.item;
                items.LineNum = data.Cont_line;
                items.Quantity = data.qty?.Split('.').FirstOrDefault();
                items.Unit = data.ser_num;
                LeadPartner.Text = LoginStorage.Username;
            }
        }
        #region Get collection details 
        private void GetCollectionLineDetails()
        {
            try
            {
                LeadPartner.Text = LoginStorage.Username;
                //check internet status
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //collect collection details API url
                    string collectionDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContLinesCollectionDetailsByContractSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    //get collectionDetails response
                    var collectionDetailsResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionDetailsURL, "GET", true, null, LoginStorage.AccessToken);
                    //convert to json object
                    var collectionDetailsJson = JObject.Parse(collectionDetailsResponse);
                    var getItems = collectionDetailsJson.SelectToken("Items");
                    //remove formatter
                    string fainalResult = getItems.ToString(Newtonsoft.Json.Formatting.None);
                    //check total items 
                    if (fainalResult != "null")
                    {
                        //convert to array for collection details 
                        JArray collectionDetailsArray = JArray.Parse(fainalResult);
                        for (int i = 0; i < collectionDetailsArray.Count; i++)
                        {
                            JToken elem = collectionDetailsArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<ContractLinesData>(jtokenString);
                            //bind collection data to UI form
                            UM.Text = data.u_m;
                            LineNum.Text = data.cont_line;
                            SerialNum.Text = data.ser_num;
                            Item.Text = data.item;
                            objDescription = data.description;
                            Description.Text = objDescription;
                            Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                            //get lineStatus with value
                            LineStatusList.Text = data.lineStatus;
                            //get BillingFrequency with value
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
                                // txtStartDate.Text = DateTime.ParseExact(dateStart, "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                                //txtStartDate.Text = DateTime.Parse(data.start_date.Substring(0, 10)).ToString();
                                //txtStartDate.Text = DateTime.ParseExact(data.start_date.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                            }
                            else
                                txtStartDate.Text = "";

                            if (data.end_date != null)
                            {
                                txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data.end_date);
                                //txtEndDate.Text = DateTime.ParseExact(data.data.start_date.ToString("MM/dd/yyyy");.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                            }
                            else
                                txtEndDate.Text = "";

                            if (data.checkInDate != null)
                            {
                                txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkInDate);
                                //txtCheckInDate.Text = DateTime.ParseExact(data.checkInDate.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                            }
                            else
                                txtCheckInDate.Text = "";

                            if (data.checkOutDate != null)
                            {
                                txtCheckOutDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkOutDate);
                                //txtCheckOutDate.Text = DateTime.ParseExact(data.checkOutDate.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                            }
                            else
                                txtCheckOutDate.Text = "";

                            items.ItemName = data.item;
                            items.LineNum = data.cont_line;
                            items.Quantity = data.qty?.Split('.').FirstOrDefault();
                            items.Unit = data.ser_num;
                            list.Add(items);
                            //Insert locat database 
                            ContractLineSiteData contractLineDetails = new ContractLineSiteData();
                            var isContractLineExists = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == data.cont_line).Count();
                            if (isContractLineExists == 0)
                            {
                                contractLineDetails.Contract = objContractId;
                                contractLineDetails.Cont_line = data.cont_line;
                                contractLineDetails.description = data.description;
                                //contractLineDetails.start_date = data.start_date;
                                //contractLineDetails.end_date = data.end_date;
                                //contractLineDetails.checkInDate = data.checkInDate;
                                //contractLineDetails.checkOutDate = data.checkOutDate;
                                contractLineDetails.Billing_freq = data.Billingfreq;
                                contractLineDetails.ser_num = data.ser_num;
                                contractLineDetails.item = data.item;
                                contractLineDetails.qty = data.qty;
                                contractLineDetails.rate_conv = data.rate_conv;
                                contractLineDetails.unit_of_rate = data.UnitOfRate;
                                contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
                                contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;

                                con.Insert(contractLineDetails);
                            }
                        }
                    }
                    else
                        DisplayAlert("Collection Lines Details", "No Collection data found for this Contract -" + objContractId, "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "Ok");
            }

        }
        #endregion
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
        public int GetLineStatus(string objLineStatus)
        {
            int status = 0;
            switch (objLineStatus)
            {
                case "Open":
                    status = 0;
                    break;
                case "Closed":
                    status = 1;
                    break;
                case "Delivered":
                    status = 2;
                    break;
                case "Hold":
                    status = 3;
                    break;
                case "Picked":
                    status = 4;
                    break;
                case "Packed":
                    status = 5;
                    break;
                case "Shipped":
                    status = 6;
                    break;
                case "Pickup":
                    status = 7;
                    break;
                case "Estimate":
                    status = 8;
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
            //UserDialogs.Instance.ShowLoading("Loading please wait...");
            ////await Navigation.PushAsync(new RentalContractPage(objContractId));
            //UserDialogs.Instance.HideLoading();
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
