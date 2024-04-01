using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Common;
using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewYardReturnlines : ContentPage
    {
        public List<string> AttachmentList { get; set; } = new List<string>();

        List<LineItems> list = new List<LineItems>();
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string objSiteRef { get; set; }
        public string objDescription { get; set; }
        public string objQuantity { get; set; }
        public bool isHold { get; set; } = false;

        private SQLiteConnection con;

        public NewYardReturnlines()
        {
            InitializeComponent();
        }
        public NewYardReturnlines(string objContId, string objLineNum, string objSitRef)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            List<string> lineStatusList = new List<string>()
            {

               "Collected",
               "Returned",
               "Return Inspected"
            };
            //bind status list 
            LineStatusList.ItemsSource = lineStatusList;
            objContractId = objContId;
            objLineNumber = objLineNum;
            objSiteRef = objSitRef;
            LeadPartner.Text = LoginStorage.Username;

            //call collection details

        }
        protected override void OnAppearing()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetYardReturnLinesDetails();
            }
            else
            {
                GetYardReturnDetailsFromLocalDB();
            }
        }
        public void GetYardReturnDetailsFromLocalDB()
        {
            ObservableCollection<YardReturnLineDetails> YardReturnLinesDetailsList = new ObservableCollection<YardReturnLineDetails>();
            YardReturnLinesDetailsList.Clear();
            var isApptExists = con.Table<YardReturnLineDetails>().Where(a => a.Cont_line == objLineNumber).Count();
            var appts = con.Table<YardReturnLineDetails>().Where(a => a.Cont_line == objLineNumber).FirstOrDefault();

            if (isApptExists > 0)
            {
                LineItems items = new LineItems();

                var data = con.Table<YardReturnLineDetails>().Where(a => a.Cont_line == objLineNumber).FirstOrDefault();
                UM.Text = data.U_m;
                objQuantity = data.Qty?.Split('.').FirstOrDefault();
                LineNum.Text = data.Cont_line;
                SerialNum.Text = data.Ser_num;
                Item.Text = data.Item;
                objDescription = data.Description;
                Description.Text = data.Description;
                Quantity.Text = data.Qty?.Split('.').FirstOrDefault();
                //get lineStatus with value
                LineStatusList.SelectedIndex = GetLineStatus(data.LineStatus);
                //get BillingFrequency with value
                BillingFrequency.Text = GetBillingFrequency(data.Billingfreq);
                if (data.Rate_conv != null)
                {
                    Rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.Rate_conv));
                }
                Unit_of_rate.Text = GetBillingFrequency(data.unit_of_rate);
                if (data.Prorate_rate_conv != null)
                {
                    Prorate_rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.Prorate_rate_conv));
                }
                Prorate_unit_of_rate.Text = GetBillingFrequency(data.prorate_unit_of_rate);
                if (data.Start_date != null)
                {
                    txtStartDate.Text = DateTimeDataConverter.StringtoDateTime(data.Start_date);
                }

                if (data.End_date != null)
                {
                    txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data?.End_date);
                }

                if (data.CheckInDate != null)
                {
                    txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data.CheckInDate);
                }

                if (data.CheckOutDate != null)
                {
                    txtCheckOutDate.Text = DateTimeDataConverter.StringtoDateTime(data?.CheckOutDate);
                }

                items.ItemName = data.Item;
                items.LineNum = data.Cont_line;
                items.Quantity = data.Qty?.Split('.').FirstOrDefault();
                items.Unit = data.Ser_num;
                list.Add(items);
            }

        }
        private void GetYardReturnLinesDetails()
        {
            try
            {
                //check internet status
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //collect collection details API url
                    string collectionDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardReturnlinesByContract?clm=ERS_GetYardReturnlineDetailByContractSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    //get collectionDetails response
                    var collectionDetailsResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionDetailsURL, "GET", true, null, LoginStorage.AccessToken);
                    //convert to json object
                    var collectionDetailsJson = JObject.Parse(collectionDetailsResponse);
                    var getItems = collectionDetailsJson.SelectToken("Items");
                    //remove formatter
                    string finalResult = getItems.ToString(Newtonsoft.Json.Formatting.None);
                    //check total items 
                    if (finalResult != "null")
                    {
                        //convert to array for collection details 
                        JArray collectionDetailsArray = JArray.Parse(finalResult);
                        for (int i = 0; i < collectionDetailsArray.Count; i++)
                        {
                            JToken elem = collectionDetailsArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<YardReturnLineDetails>(jtokenString);
                            //bind collection data to UI form
                            objQuantity = data.Qty?.Split('.').FirstOrDefault();
                            UM.Text = data.U_m;
                            LineNum.Text = data.Cont_line;
                            SerialNum.Text = data.Ser_num;
                            Item.Text = data.Item;
                            objDescription = data.Description;
                            Description.Text = data.Description;
                            Quantity.Text = data.Qty?.Split('.').FirstOrDefault();
                            //get lineStatus with value
                            LineStatusList.SelectedIndex = GetLineStatus(data.LineStatus);
                            //get BillingFrequency with value
                            BillingFrequency.Text = GetBillingFrequency(data.Billingfreq);
                            if (data.Rate_conv != null)
                            {
                                Rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.Rate_conv));
                            }
                            Unit_of_rate.Text = GetBillingFrequency(data.unit_of_rate);
                            if (data.Prorate_rate_conv != null)
                            {
                                Prorate_rate_conv.Text = string.Format("{0:0.00}", decimal.Parse(data.Prorate_rate_conv));
                            }
                            Prorate_unit_of_rate.Text = GetBillingFrequency(data.prorate_unit_of_rate);
                            if (data.Start_date != null)
                            {
                                txtStartDate.Text = DateTimeDataConverter.StringtoDateTime(data?.Start_date);
                            }

                            if (data.End_date != null)
                            {
                                txtEndDate.Text = DateTimeDataConverter.StringtoDateTime(data?.End_date);
                            }

                            if (data.CheckInDate != null)
                            {
                                txtCheckInDate.Text = DateTimeDataConverter.StringtoDateTime(data?.CheckInDate);
                            }

                            if (data.CheckOutDate != null)
                            {
                                txtCheckOutDate.Text =  DateTimeDataConverter.StringtoDateTime(data?.CheckOutDate);
                            }

                            items.ItemName = data.Item;
                            items.LineNum = data.Cont_line;
                            items.Quantity = data.Qty?.Split('.').FirstOrDefault();
                            items.Unit = data.Ser_num;
                            list.Add(items);
                            //Insert locat database 
                            YardReturnLineData yardReturnLineDetails = new YardReturnLineData();
                            var isContractLineExists = con.Table<YardReturnLineData>().Where(a => a.Contract == objContractId && a.Cont_line == data.Cont_line).Count();
                            if (isContractLineExists == 0)
                            {
                                yardReturnLineDetails.Contract = objContractId;
                                yardReturnLineDetails.Cont_line = data.Cont_line;
                                yardReturnLineDetails.Description = data.Description;
                                yardReturnLineDetails.Start_date = DateTimeDataConverter.StringtoDateTimedata(data?.Start_date);
                                yardReturnLineDetails.End_date = DateTimeDataConverter.StringtoDateTimedata(data?.End_date);
                                yardReturnLineDetails.CheckInDate = DateTimeDataConverter.StringtoDateTimedata(data?.CheckInDate);
                                yardReturnLineDetails.CheckOutDate = DateTimeDataConverter.StringtoDateTimedata(data?.CheckOutDate);
                                yardReturnLineDetails.Billing_freq = data.Billingfreq;
                                yardReturnLineDetails.Ser_num = data.Ser_num;
                                yardReturnLineDetails.Item = data.Item;
                                yardReturnLineDetails.Qty = data.Qty;
                                yardReturnLineDetails.Rate_conv = data.Rate_conv;
                                yardReturnLineDetails.Unit_of_rate = data.UnitOfRate;
                                yardReturnLineDetails.Prorate_rate_conv = data.Prorate_rate_conv;
                                yardReturnLineDetails.Prorate_unit_of_rate = data.prorate_unit_of_rate;

                                con.Insert(yardReturnLineDetails);
                            }
                        }
                    }
                    else
                        DisplayAlert("YardReturn Lines Details", "No YardReturn data found for this Contract -" + objContractId, "OK");
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
        public int GetLineStatus(string objLineStatus)
        {
            int status = 0;
            switch (objLineStatus)
            {
                case "Collected":
                    status = 0;
                    break;
                case "Returned":
                    status = 1;
                    break;
                case "Return Inspected":
                    status = 2;
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
        
        private async void RentalContractDetails(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            await Navigation.PushAsync(new RentalContractPage());
            UserDialogs.Instance.HideLoading();
        }
        private async void NotepadClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkOrderPage());
        }
        private async void UnitClicked(object sender, EventArgs e)
        {

            //await Navigation.PushAsync(new YardReturnUnitSelection("suchi"));
        }
        public async void SaveSignature()
        {
            var image = await ReturnSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
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
                try
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        //prepared the required collection data for submit
                        var yardPickupData = LoginStorage.Site + "," + objContractId + "," + objLineNumber + "," +objQuantity;
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
                        updateSignatureData.Changes[0].Properties[3].Value = "POR";
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
                }
                catch (Exception ex)
                {
                    await DisplayAlert("", "Please Select the status and sign to save", "ok");
                }

                await Navigation.PopAsync();
            }
        }

        private async void Save_YardDetails(object sender, EventArgs e)
        {
            var selectedStatus = LineStatusList.SelectedItem.ToString();
            //if (selectedStatus == "Return Inspected" && holdToggle.IsToggled == false)
            //{
            //    await DisplayAlert("Alert", "Are you sure to save without putting in on hold?", "ok");
            //}
            if (selectedStatus == null )
            {
                await DisplayAlert("Alert", "Please Select the status to save", "ok");
            }
            else
            {
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
                        var image = await ReturnSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
                        var mStream = (MemoryStream)image;
                        byte[] Signaturedata = mStream.ToArray();
                        string base64Val = Convert.ToBase64String(Signaturedata);
                        YardReturnPostTemplate postYardReturnTemp = new YardReturnPostTemplate();
                        postYardReturnTemp.SelectedStatus = selectedStatus;
                        postYardReturnTemp.LineNumber = objLineNumber;
                        postYardReturnTemp.contractID = objContractId;
                        postYardReturnTemp.siteId = LoginStorage.Site;
                        postYardReturnTemp.Signature = base64Val;
                        postYardReturnTemp.objQty = objQuantity?.Split('.').FirstOrDefault();
                        postYardReturnTemp.SignedTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ".000";
                        con.Insert(postYardReturnTemp);
                        var isApptExists = con.Table<YardReturnLineData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).Count();
                        var appts = con.Table<YardReturnLineData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();
                        if (isApptExists > 0)
                        {
                            foreach (var item in appts)
                            {
                                if (selectedStatus == "Return Inspected")
                                {
                                    con.Table<YardReturnLineData>().Delete(x => x.Cont_line == item.Cont_line);
                                }
                            }
                        }
                        await DisplayAlert("", "Saved Successfully!!", "Ok");
                        await Navigation.PopAsync();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("", "", "ok");
                }
            }
        }
        
        private void ReturnSubmitClicked(object sender, EventArgs e)
        {

        }

        async void Hold_Buttonclicked(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            Switch switchData = sender as Switch;
            

        }


    }  
}