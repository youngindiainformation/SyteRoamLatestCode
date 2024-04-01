using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Acr.UserDialogs;
using Newtonsoft.Json;
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
    public partial class YardPickingLineDetail : ContentPage
    {
        public List<string> AttachmentList { get; set; } = new List<string>();

        List<LineItems> list = new List<LineItems>();
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string objSiteRef { get; set; }
        public string objDescription { get; set; }
        public string objItem { get; set; }
        public string objUnitflag { get; set; }
        public string objQuantity { get; set; }

        private SQLiteConnection con;

        public YardPickingLineDetail()
        {
            InitializeComponent();
        }
        public YardPickingLineDetail(string objContId, string objLineNum, string objSitRef)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            List<string> lineStatusList = new List<string>()
            {
               "Open",
               "Picked",
               "Delivery Inspected",
               "Loaded"
            };
            //bind status list 
            LineStatusList.ItemsSource = lineStatusList;
            objContractId = objContId;
            objLineNumber = objLineNum;
            objSiteRef = objSitRef;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetYardPickupLineDetails();
            }
            else
            {
                GetYardPickupDetailsFromLocalDB();
            }

        }
        protected override void OnAppearing()
        {
           
            MessagingCenter.Subscribe<YardReturnUnitSelection, string>(this, "Hi", async (sender, arg) =>
            {
                SerialNum.Text = arg;
                objUnitflag = "0";
                //await DisplayAlert("Message received", "arg=" + arg, "OK");
            });

        }
        public void GetYardPickupDetailsFromLocalDB()
        {
            ObservableCollection<YardPickingLineDetails> collectionLinesListDetails = new ObservableCollection<YardPickingLineDetails>();
            collectionLinesListDetails.Clear();
            var isApptExists = con.Table<YardPickingLineDetails>().Where(a => a.cont_line == objLineNumber).Count();
            var appts = con.Table<YardPickingLineDetails>().Where(a=>a.cont_line == objLineNumber).FirstOrDefault();

            if (isApptExists > 0)
            {
                LineItems items = new LineItems();

                var data = con.Table<YardPickingLineDetails>().Where(a=>a.cont_line == objLineNumber).FirstOrDefault();

                LineNum.Text = data.cont_line;
                SerialNum.Text = data.ser_num;
                Item.Text = data.item;
                objDescription = data.description;
                Description.Text = objDescription;
                objQuantity = data.qty?.Split('.').FirstOrDefault();
                Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                //get lineStatus with value
                LineStatusList.SelectedIndex = GetLineStatus(data.lineStatus);
                //get BillingFrequency with value
                BillingFrequency.Text = GetBillingFrequency(data.Billingfreq);
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
                    txtStartDate.Text = data.start_date?.ToString("MM/dd/yyyy hh:mm:ss tt");
                }
                else
                    txtStartDate.Text = "";

                if (data.end_date != null)
                {
                    txtEndDate.Text = data.end_date?.ToString("MM/dd/yyyy hh:mm:ss tt");
                }
                else
                    txtEndDate.Text = "";

                if (data.checkInDate != null)
                {
                    txtCheckInDate.Text = data.checkInDate?.ToString("MM/dd/yyyy hh:mm:ss tt");
                }
                else
                    txtCheckInDate.Text = "";

                if (data.checkOutDate != null)
                {
                    txtCheckOutDate.Text = data.checkOutDate?.ToString("MM/dd/yyyy hh:mm:ss tt");
                }
                else
                    txtCheckOutDate.Text = "";

                items.ItemName = data.item;
                items.LineNum = data.cont_line;
                items.Quantity = data.qty?.Split('.').FirstOrDefault();
                items.Unit = data.ser_num;
                list.Add(items);
                UM.Text = data.u_m;
                objItem = data.item;
                LeadPartner.Text = LoginStorage.Username;
                
            }
        }
        #region Get collection details 
        private void GetYardPickupLineDetails()
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
                    string yardPickupDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardPickinglineDetailByContract?clm=ERS_GetYardPickinglineDetailByContractSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    //get collectionDetails response
                    var yardPickupDetailsResponse = mongooseAPIRequest.ProcessRestAPIRequest(yardPickupDetailsURL, "GET", true, null, LoginStorage.AccessToken);
                    //convert to json object
                    var yardPickupDetailsJson = JObject.Parse(yardPickupDetailsResponse);
                    var getItems = yardPickupDetailsJson.SelectToken("Items");
                    //remove formatter
                    string finalResult = getItems.ToString(Newtonsoft.Json.Formatting.None);
                    //check total items 
                    if (finalResult != "null")
                    {
                        //convert to array for collection details 
                        JArray yardPickupDetailsArray = JArray.Parse(finalResult);
                        for (int i = 0; i < yardPickupDetailsArray.Count; i++)
                        {
                            JToken elem = yardPickupDetailsArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<ContractLinesData>(jtokenString);
                            //bind collection data to UI form 
                            objQuantity = data.qty?.Split('.').FirstOrDefault();
                            LineNum.Text = data.cont_line;
                            SerialNum.Text = data.ser_num;
                            Item.Text = data.item;
                            UM.Text = data.u_m;
                            objDescription = data.description;
                            Description.Text = objDescription;
                            Quantity.Text = data.qty?.Split('.').FirstOrDefault();
                            objUnitflag = data.unitflag;
                            if(objUnitflag == "0")
                            {
                                unitsIcon.IsEnabled = false;
                            }
                            //get lineStatus with value
                            LineStatusList.SelectedIndex = GetLineStatus(data.lineStatus);
                            //get BillingFrequency with value
                            BillingFrequency.Text = GetBillingFrequency(data.Billingfreq);
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
                                txtStartDate.Text = DateTimeDataConverter.StringtoDateTime(data.start_date);
                            }
                            else
                                txtStartDate.Text = "";

                            if (data.end_date != null)
                            {
                                txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data.end_date);
                            }
                            else
                                txtEndDate.Text = "";

                            if (data.checkInDate != null)
                            {
                                txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkInDate);
                            }
                            else
                                txtCheckInDate.Text = "";

                            if (data.checkOutDate != null)
                            {
                                txtCheckOutDate.Text = DateTimeDataConverter.StringtoDateTime(data.checkOutDate);
                            }
                            else
                                txtCheckOutDate.Text = "";
                            objItem = data.item;
                            items.ItemName = data.item;
                            items.LineNum = data.cont_line;
                            items.Quantity = data.qty?.Split('.').FirstOrDefault();
                            items.Unit = data.ser_num;
                            list.Add(items);
                            
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
                case "Picked":
                    status = 1;
                    break;
                case "Delivery Inspected":
                    status = 2;
                    break;
                case "Loaded":
                    status = 3;
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
        private async void UnitClicked(object sender, EventArgs e)
        {
            SaveUnitDetails data = new SaveUnitDetails();
            data.ContractID = objContractId;
            data.LineNum = objLineNumber;
            data.item = objItem;
            await Navigation.PushAsync(new YardReturnUnitSelection(data));
        }

        private void PickingSubmitClicked(object sender, EventArgs e)
        {

        }
        public async void SaveSignature()
        {
            var image = await MainSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
            if (image == null)
            {
                await DisplayAlert("", "Please sign to save", "ok");
            }
            else
            {
                var mStream = (MemoryStream)image;
                byte[] Signaturedata = mStream.ToArray();
                string base64Val = Convert.ToBase64String(Signaturedata);

                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string ItemId = null;
                string POCID = null;
                string postResponse = "";
                List<string> linesData = new List<string>();
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    //prepared the required collection data for submit
                    var yardPickupData = LoginStorage.Site + "," + objContractId + "," + objLineNumber + ","  + objQuantity;
                    //get collection data URL 
                    var getYardPickUpURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?clm=ERS_Rental_POC_POD_SP&clmparam=" + yardPickupData;
                    //intiate API request
                    var httpClient = new HttpClient();
                    MongooseAPIRequest objMongooseAPIRequest = new MongooseAPIRequest();
                    //get uinque POCID collection from table 
                    var getYardPickupResponse = mongooseAPIRequest.ProcessRestAPIRequest(getYardPickUpURL, "GET", true, null, LoginStorage.AccessToken);
                    //parse collection json
                    var YardPickupJson = JObject.Parse(getYardPickupResponse);
                    //check items list
                    var checkStaus = YardPickupJson.SelectToken("Items");
                    //to set final format of response
                    string finalResult = checkStaus.ToString(Newtonsoft.Json.Formatting.None);
                    if (finalResult != "null")
                    {
                        //get POCID from response
                        ObservableCollection<GetPOCId> getPOCId = new ObservableCollection<GetPOCId>();
                        JArray array = JArray.Parse(finalResult);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            string formatString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var resultData = JsonConvert.DeserializeObject<GetPOCId>(formatString);
                            POCID = resultData.IDPODPOC;
                            ItemId = elem["_ItemID"].ToString();
                        }
                    }
                    ////get url for table itemID
                    //string getPOCItemIDURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?Properties=IDPODPOC&filter=IDPODPOC=" + POCID;
                    ////response for POCID items
                    //var responsePOCItem = mongooseAPIRequest.ProcessRestAPIRequest(getPOCItemIDURL, "GET", true, null, LoginStorage.AccessToken);
                    ////parse collection json
                    //var parseResponse = JObject.Parse(responsePOCItem);
                    ////check items list
                    //var selectItemResult = parseResponse.SelectToken("Items");
                    ////to set final format of response
                    //string itemResultFormat = selectItemResult.ToString(Newtonsoft.Json.Formatting.None);
                    //if (itemResultFormat != "null")
                    //{
                    //    //get ItemID for eariler POCID from response
                    //    ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
                    //    JArray array = JArray.Parse(itemResultFormat);
                    //    for (int i = 0; i < array.Count; i++)
                    //    {
                    //        JToken elem = array[i];
                    //        string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                    //        var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                    //        ItemId = data._ItemId;
                    //    }
                    //}
                    //get url for signature update
                    var collectionPostDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/ERS_Rental_POC_POD?refresh=true";
                    var accessToken = LoginStorage.AccessToken;
                    //prepare the update object with respective data
                    //USE -  "POC" as type of collection line submit 
                    // string collectionRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    string yardPickUpData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    //deserialize with signature form object
                    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(yardPickUpData);
                    updateSignatureData.Changes[0].ItemId = ItemId;
                    updateSignatureData.Changes[0].Properties[0].Value = base64Val;
                    updateSignatureData.Changes[0].Properties[1].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ".000"; 
                    updateSignatureData.Changes[0].Properties[2].Value = LoginStorage.Username;
                    updateSignatureData.Changes[0].Properties[3].Value = "POP";
                    updateSignatureData.Changes[0].Properties[4].Value = LineStatusList.SelectedItem.ToString();
                    yardPickUpData = JsonConvert.SerializeObject(updateSignatureData);
                    //response for collection line submit 
                    var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionPostDataURl, "POST", true, yardPickUpData, LoginStorage.AccessToken);
                    //parse collection json
                    var parseSingaureResponse = JObject.Parse(signatureResponse);
                    //check API status
                    var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                    postResponse = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                }
                if (postResponse == "true")
                {
                    await DisplayAlert("", "Saved Successfully!!", "Ok");
                }
                else
                {
                    await DisplayAlert("", "Please enter valid data", "Ok");
                }               
                await Navigation.PopAsync();
            }
        }
        private async void SaveYardPickupLinesDetails(object sender, EventArgs e)
        {
            if(objUnitflag == "0")
            {

                SaveYardPickupLinesData();
            }
            else
            {
                if(SerialNum == null)
                {
                    await DisplayAlert("Alert", "Please Select Unit Number", "OK");
                }
                else
                {
                    SaveYardPickupLinesData();
                }
            }
        }
        private async void SaveYardPickupLinesData()
        {
            try
            {
                               
                    var selectedStatus = LineStatusList.SelectedItem.ToString();
                try
                {
                    var current = Connectivity.NetworkAccess;
                    if (current == NetworkAccess.Internet)
                    {

                        var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                        ObservableCollection<string> objYardReturnPostObjects = new ObservableCollection<string>()
                            {
                                LoginStorage.Site,
                                objContractId,
                                objLineNumber,
                                selectedStatus
                            };
                        var client3 = new HttpClient();
                        MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                        string jsonData = JsonConvert.SerializeObject(objYardReturnPostObjects);
                        var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                        var dynJson = JObject.Parse(properties);
                        var res = dynJson.SelectToken("Success");
                        var url = dynJson.SelectToken("Success");
                        string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                        if (jtokenStr == "true")
                        {
                            SaveSignature();
                        }
                        else
                        {
                            await DisplayAlert("", "Please enter valid data", "Ok");
                        }
                    }
                    else
                    {
                        var image = await MainSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
                        var mStream = (MemoryStream)image;
                        byte[] Signaturedata = mStream.ToArray();
                        string base64Val = Convert.ToBase64String(Signaturedata);
                        YardPickupPostTemplate postYardPickupTemp = new YardPickupPostTemplate();
                        postYardPickupTemp.SelectedStatus = selectedStatus;
                        postYardPickupTemp.contractID = objContractId;
                        postYardPickupTemp.LineNumber = objLineNumber;
                        postYardPickupTemp.SiteId = LoginStorage.Site;
                        postYardPickupTemp.Signature = base64Val;
                        postYardPickupTemp.objQty = objQuantity;
                        postYardPickupTemp.Flag = "0";
                        postYardPickupTemp.SignedTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ".000";
                        con.Insert(postYardPickupTemp);
                        var isApptExists = con.Table<YardPickingLinesData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).Count();
                        var appts = con.Table<YardPickingLinesData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();
                        if (isApptExists > 0)
                        {
                            foreach (var item in appts)
                            {
                                if (selectedStatus == "Loaded")
                                {
                                    con.Table<YardPickingLinesData>().Delete(x => x.Cont_line == item.Cont_line);
                                }
                                else
                                {
                                    MessagingCenter.Send<YardPickingLineDetail>(this, selectedStatus);
                                    //con.Table<YardPickingLinesData>().Delete(x => x.Cont_line == item.Cont_line);
                                    //YardPickupPostTemplate data = new YardPickupPostTemplate();
                                    //data.contractID = objContractId;
                                    //data.LineNumber = objLineNumber;
                                    //data.SelectedStatus = selectedStatus;
                                    //con.Insert(data);
                                }
                            }
                            await DisplayAlert("", "Saved Successfully!!", "Ok");
                            await Navigation.PopAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("", ex.Message, "ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Please Select the status and sign to save", "ok");
            }
        }
    }
}
public class SaveUnitDetails
{
    public string item { get; set; }
    public string ContractID { get; set; }
    public string LineNum { get; set; }
}
