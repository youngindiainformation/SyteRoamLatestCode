using TestApp2.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TestApp2.Models;
using SignaturePad.Forms;
using ImageFromXamarinUI;
using NativeMedia;
using DeviceOrientation.Forms.Plugin.Abstractions;
using System.Net.Http;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkOrderPage : ContentPage
    {
        private SQLiteConnection con = DependencyService.Get<ISQLite>().GetConnection();
        ObservableCollection<LineDetails> lineDetails = new ObservableCollection<LineDetails>();
        List<LineItems> GetItems = new List<LineItems>();
        string postResponse = "false";
        Stream screenShotImage;
        public WorkOrderPage()
        {
            InitializeComponent();
        }

        public WorkOrderPage(List<LineItems> Items)
        {
            GetItems = Items;
            InitializeComponent();
            ID.Text = AppointmentDetailPage.objContractId;
            CustomerName.Text = LoginStorage.CustomerName;
            TmPicert.Time = DateTime.Now.TimeOfDay;
            lineDetails = new ObservableCollection<LineDetails>();
            foreach (var item in Items)
            {
                lineDetails.Add(new LineDetails { LineNum = item?.LineNum, ItemName = item?.ItemName, Quantity = item?.Quantity?.Substring(0, 1), Unit = item?.Unit });
            }
            MenuItemsListView.ItemsSource = lineDetails;
            var svc = DependencyService.Get<IDeviceOrientation>();
            if (svc != null)
            {

            }

        }

        public static byte[] streamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        private async void SubmitClicked(object sender, EventArgs e)
        {
            btnSubmit.IsVisible = false;
            screenShotImage = await root.CaptureImageAsync();
            var image = await MainSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
            var mStream = (MemoryStream)image;
            byte[] Signaturedata = mStream.ToArray();
            string base64Val = Convert.ToBase64String(Signaturedata);
            WorkOrderData workOrderData = new WorkOrderData();
            PodDocumentDetails podDocument = new PodDocumentDetails();
            string POCID = null;
            //workOrderData.LineNumbers = "";
            workOrderData.PartnerID = LoginStorage.PartnerId;
            workOrderData.ContractID = AppointmentDetailPage.objContractId;
            workOrderData.CustomerName = AppointmentDetailPage.objCustName;
            workOrderData.Signature = base64Val;
            workOrderData.Name = Name.Text;
            podDocument.PodName = Name.Text;
            podDocument.podTime = new TimeSpan(TmPicert.Time.Hours, TmPicert.Time.Minutes, TmPicert.Time.Seconds).ToString();
            podDocument.podContractID = AppointmentDetailPage.objContractId;
            workOrderData.Time = new TimeSpan(TmPicert.Time.Hours, TmPicert.Time.Minutes, TmPicert.Time.Seconds).ToString();
            podDocument.PodDoc = streamToByteArray(screenShotImage);
            workOrderData.Date = DtPicker.Date.ToString("MM/dd/yyyy");
            string submitDate = DtPicker.Date.ToString("yyyy-MM-dd") + " " + workOrderData.Time + ".000";
            workOrderData.SubmitDate = submitDate;
            podDocument.podDate = submitDate;
            MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
            string ItemId = null;

            List<string> linesData = new List<string>();
            foreach (var item in GetItems)
            {
                linesData.Add(item.LineNum);
            }
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                foreach (var item in GetItems)
                {
                    string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_RentalContLines?properties=contract_1&filter=contract_1='" + workOrderData.ContractID + "' and cont_line=" + item.LineNum;
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, null, 0);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Items");
                    string jtokenStr = res.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
                        JArray array = JArray.Parse(jtokenStr);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                            ItemId = data._ItemId;
                        }
                    }

                    var postDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/ERS_RentalContLines?refresh=true";
                    var accessToken = LoginStorage.AccessToken;
                    string jsonData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"CustDelSignature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DelDate\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"LineStatus\",\"Value\":\"Delivered\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustDelName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"contract_1\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"cont_line\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    var approveData = JsonConvert.DeserializeObject<AprooveData>(jsonData);
                    approveData.Changes[0].ItemId = ItemId;
                    approveData.Changes[0].Properties[0].Value = base64Val;
                    approveData.Changes[0].Properties[1].Value = submitDate;
                    approveData.Changes[0].Properties[3].Value = workOrderData.Name;

                    jsonData = JsonConvert.SerializeObject(approveData);
                    var properties1 = mongooseAPIRequest.ProcessRestAPIRequest(postDataURl, "POST", true, jsonData, null, 0);
                    var dynJson1 = JObject.Parse(properties);
                    var url1 = dynJson1.SelectToken("Success");
                    postResponse = url1.ToString(Newtonsoft.Json.Formatting.None);
                }
                //foreach (var item in GetItems)
                //{
                //    //prepared the required deliver data for submit
                //    var deliverData = LoginStorage.Site + "," + workOrderData.ContractID + "," + item.LineNum + "," + item.Quantity;
                //    workOrderData.Quantity = item.Quantity;
                //    //get deliver data URL 
                //    var getDeliverDataDataURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?clm=ERS_Rental_POC_POD_SP&clmparam=" + deliverData;
                //    //intiate API request
                //    var httpClient = new HttpClient();
                //    MongooseAPIRequest objMongooseAPIRequest = new MongooseAPIRequest();
                //    //get uinque POCID deliver from table 
                //    var getDeliverResponse = mongooseAPIRequest.ProcessRestAPIRequest(getDeliverDataDataURL, "GET", true, null, LoginStorage.AccessToken);
                //    //parse deliver json
                //    var collectionJson = JObject.Parse(getDeliverResponse);
                //    //check items list
                //    var checkStaus = collectionJson.SelectToken("Items");
                //    //to set final format of response
                //    string finalResult = checkStaus.ToString(Newtonsoft.Json.Formatting.None);
                //    if (finalResult != "null")
                //    {
                //        //get POCID from response
                //        ObservableCollection<GetPOCId> getPOCId = new ObservableCollection<GetPOCId>();
                //        JArray array = JArray.Parse(finalResult);
                //        for (int i = 0; i < array.Count; i++)
                //        {
                //            JToken elem = array[i];
                //            string formatString = elem.ToString(Newtonsoft.Json.Formatting.None);
                //            var resultData = JsonConvert.DeserializeObject<GetPOCId>(formatString);
                //            POCID = resultData.IDPODPOC;
                //        }
                //    }
                //    //get url for table itemID
                //    string getPOCItemIDURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?Properties=IDPODPOC&filter=IDPODPOC=" + POCID;
                //    //response for POCID items
                //    var responsePOCItem = mongooseAPIRequest.ProcessRestAPIRequest(getPOCItemIDURL, "GET", true, null, LoginStorage.AccessToken);
                //    //parse collection json
                //    var parseResponse = JObject.Parse(responsePOCItem);
                //    //check items list
                //    var selectItemResult = parseResponse.SelectToken("Items");
                //    //to set final format of response
                //    string itemResultFormat = selectItemResult.ToString(Newtonsoft.Json.Formatting.None);
                //    if (itemResultFormat != "null")
                //    {
                //        //get ItemID for eariler POCID from response
                //        ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
                //        JArray array = JArray.Parse(itemResultFormat);
                //        for (int i = 0; i < array.Count; i++)
                //        {
                //            JToken elem = array[i];
                //            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                //            var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                //            ItemId = data._ItemId;
                //        }
                //    }
                //    //get url for signature update
                //    var deliverResponsePostDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/ERS_Rental_POC_POD?refresh=true";
                //    var accessToken = LoginStorage.AccessToken;
                //    //prepare the update object with respective data
                //    //USE -  "POD" as type of collection line submit 
                //    string deliverRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                //    //deserialize with signature form object
                //    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(deliverRequiredData);
                //    updateSignatureData.Changes[0].ItemId = ItemId;
                //    updateSignatureData.Changes[0].Properties[0].Value = base64Val;
                //    updateSignatureData.Changes[0].Properties[1].Value = submitDate;
                //    updateSignatureData.Changes[0].Properties[2].Value = workOrderData.Name;
                //    updateSignatureData.Changes[0].Properties[3].Value = "POD"; //set type of Deliver line submit
                //    updateSignatureData.Changes[0].Properties[4].Value = "Delivered"; //set status of collection line 
                //    //deserialize with signature form object
                //    deliverRequiredData = JsonConvert.SerializeObject(updateSignatureData);
                //    //response for deliver line submit 
                //    var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(deliverResponsePostDataURl, "POST", true, deliverRequiredData, LoginStorage.AccessToken);
                //    //parse deliver json
                //    var parseSingaureResponse = JObject.Parse(signatureResponse);
                //    //check API status
                //    var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                //    postResponse = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                //    var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                //    ObservableCollection<string> objCollectionPostObjects = new ObservableCollection<string>()
                //    {
                //        LoginStorage.Site,
                //        workOrderData.ContractID,
                //        item.LineNum,
                //        "Delivered"
                //    };

                //    var client3 = new HttpClient();
                //    //MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                //    string pocjsonData = JsonConvert.SerializeObject(objCollectionPostObjects);
                //    var pocproperties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, pocjsonData, LoginStorage.AccessToken);
                //    var pocdynJson = JObject.Parse(pocproperties);
                //    var pocres = pocdynJson.SelectToken("Success");
                //}
            }
            workOrderData.LineNumbers = string.Join(",", linesData);
            podDocument.podLineNumbers = string.Join(",", linesData);
            con.Insert(podDocument);
            con.Insert(workOrderData);
            var isApptExists = con.Table<ContractLineDetails>().Where(a => a.Contract == AppointmentDetailPage.objContractId).Count();
            var appts = con.Table<ContractLineDetails>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();
            if (isApptExists > 0)
            {
                foreach (var item in appts)
                {
                    foreach (var lines in GetItems)
                    {
                        if (item.Cont_line == lines.LineNum)
                        {
                            con.Table<ContractLineDetails>().Delete(x => x.Cont_line == lines.LineNum);
                        }
                    }
                }
                var Linesdata = con.Table<ContractLineDetails>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();
                await DisplayAlert("Notification", "Delivered Lines has been submitted successfully", "ok");
                MessagingCenter.Send<WorkOrderPage>(this, "Hi");
                await Navigation.PopAsync();
            }

        }

        private async void OrderMenuClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet(" ", "Cancel", null, "Material", "Labor", "Miscellanous", "Pay with Credit Card");

            if (action != null)
            {
                switch (action)
                {
                    case "Material":
                        break;

                    case "Labor":
                        break;

                    case "Miscellanous":
                        break;

                    case "Pay with Credit Card":
                        await Navigation.PushAsync(new AuthorizationPage());
                        break;

                    default:
                        break;
                }
            }
        }

    }

    public class LineDetails
    {
        public string LineNum { get; set; }
        public string ItemName { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
    }
    public class GetItemId
    {
        public string contract_1 { get; set; }
        public string _ItemId { get; set; }
        public string Bookmark { get; set; }
    }

    public class PropertyModel
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public object OriginalValue { get; set; }
        public bool Modified { get; set; }
        public bool IsNull { get; set; }
        public bool IsNestedCollection { get; set; }
    }

    public class ChangeModel
    {
        public int Action { get; set; }
        public string ItemId { get; set; }
        public List<PropertyModel> Properties { get; set; }
        public int UpdateLocking { get; set; }
    }

    public class AprooveData
    {
        public List<ChangeModel> Changes { get; set; }
    }
}

