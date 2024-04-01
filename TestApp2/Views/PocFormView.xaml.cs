using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ImageFromXamarinUI;
using SQLite;
using Xamarin.Forms;
using SignaturePad.Forms;
using TestApp2.Models.SQLite;
using TestApp2.Common.Helper;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;


namespace TestApp2.Views
{
    public partial class PocFormView : ContentPage
    {
        private SQLiteConnection con = DependencyService.Get<ISQLite>().GetConnection();
        ObservableCollection<CollectionLineDetails> CollectionLineDetails = new ObservableCollection<CollectionLineDetails>();
        string postResponse = "false";

        List<LineItems> GetListItems = new List<LineItems>();
        Stream screenShotImage;

        public PocFormView(List<LineItems> Items)
        {
            try
            {
                InitializeComponent();
                GetListItems = Items;
                contractID.Text = AppointmentDetailPage.objContractId;
                CustName.Text = LoginStorage.CustomerName;
                Time_Picert.Time = DateTime.Now.TimeOfDay;
                CollectionLineDetails = new ObservableCollection<CollectionLineDetails>();
                foreach (var item in Items)
                {
                    CollectionLineDetails.Add(new CollectionLineDetails { Coll_LineNum = item.LineNum, Coll_ItemName = item.ItemName, Coll_Quantity = item.Quantity.Substring(0, 1), Coll_Unit = item.Unit });
                }
                CollectionItemsListView.ItemsSource = CollectionLineDetails;

    }
            catch (Exception ex)
            {

             
            };
        }
        public static byte[] streamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
        private async void CollectionSubmitClicked(object sender, EventArgs e)
        {
            SubmitCollection.IsVisible = false;
            screenShotImage = await root1.CaptureImageAsync();
            var image = await MainSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png);
            var mStream = (MemoryStream)image;
            byte[] Signaturedata = mStream.ToArray();
            string base64Val = Convert.ToBase64String(Signaturedata);
            CollectionWorkOrderData collectionworkOrderData = new CollectionWorkOrderData();
            PocDocumentDetails pocDocument = new PocDocumentDetails();
            //workOrderData.LineNumbers = "";
            collectionworkOrderData.CollectionPartnerID = LoginStorage.PartnerId;
            pocDocument.CollectionContractID = AppointmentDetailPage.objContractId;
            collectionworkOrderData.CollectionContractID = AppointmentDetailPage.objContractId;
            collectionworkOrderData.CollectionSignature = base64Val;
            collectionworkOrderData.CollectionName = EntryName.Text;
            pocDocument.CollectionName = EntryName.Text;
            collectionworkOrderData.CollectionTime = new TimeSpan(Time_Picert.Time.Hours, Time_Picert.Time.Minutes, Time_Picert.Time.Seconds).ToString();
            //collectionworkOrderData.CollectionPocDocument = streamToByteArray(screenShotImage);
            collectionworkOrderData.CollectionDate = Date_Picker.Date.ToString("MM/dd/yyyy");
            string submitDate = Date_Picker.Date.ToString("yyyy-MM-dd") + " " + collectionworkOrderData.CollectionTime + ".000";
            collectionworkOrderData.CollectionSubmitDate = submitDate;
            pocDocument.CollectionDate = submitDate;
            pocDocument.PocDoc = streamToByteArray(screenShotImage);
            pocDocument.CollectionTime = new TimeSpan(Time_Picert.Time.Hours, Time_Picert.Time.Minutes, Time_Picert.Time.Seconds).ToString();
            

            MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
            string ItemId = null;
            string POCID = null;
            List<string> linesData = new List<string>();
            foreach (var item in GetListItems)
            {
                linesData.Add(item.LineNum);
            }
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                foreach (var item in GetListItems)
                {
                    //prepared the required collection data for submit
                    var collectionData = LoginStorage.Site + "," + collectionworkOrderData.CollectionContractID + "," + item.LineNum + "," + item.Quantity;
                    //get collection data URL 
                    var getCollectionDataURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?clm=ERS_Rental_POC_POD_SP&clmparam=" + collectionData;
                    //intiate API request
                    var httpClient = new HttpClient();
                    MongooseAPIRequest objMongooseAPIRequest = new MongooseAPIRequest();
                    //get uinque POCID collection from table 
                    var getCollectionResponse = mongooseAPIRequest.ProcessRestAPIRequest(getCollectionDataURL, "GET", true, null, LoginStorage.AccessToken);
                    //parse collection json
                    var collectionJson = JObject.Parse(getCollectionResponse);
                    //check items list
                    var checkStaus = collectionJson.SelectToken("Items");
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
                        }
                    }
                    //get url for table itemID
                    string getPOCItemIDURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?Properties=IDPODPOC&filter=IDPODPOC=" + POCID;
                    //response for POCID items
                    var responsePOCItem = mongooseAPIRequest.ProcessRestAPIRequest(getPOCItemIDURL, "GET", true, null, LoginStorage.AccessToken);
                    //parse collection json
                    var parseResponse = JObject.Parse(responsePOCItem);
                    //check items list
                    var selectItemResult = parseResponse.SelectToken("Items");
                    //to set final format of response
                    string itemResultFormat = selectItemResult.ToString(Newtonsoft.Json.Formatting.None);
                    if (itemResultFormat != "null")
                    {
                        //get ItemID for eariler POCID from response
                        ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
                        JArray array = JArray.Parse(itemResultFormat);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                            ItemId = data._ItemId;
                        }
                    }
                    //get url for signature update
                    var collectionPostDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/ERS_Rental_POC_POD?refresh=true";
                    var accessToken = LoginStorage.AccessToken;
                    //prepare the update object with respective data
                    //USE -  "POC" as type of collection line submit 
                    // string collectionRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    string collectionRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    //deserialize with signature form object
                    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(collectionRequiredData);
                    updateSignatureData.Changes[0].ItemId = ItemId;
                    updateSignatureData.Changes[0].Properties[0].Value = base64Val;
                    updateSignatureData.Changes[0].Properties[1].Value = submitDate;
                    updateSignatureData.Changes[0].Properties[2].Value = collectionworkOrderData.CollectionName;
                    updateSignatureData.Changes[0].Properties[3].Value = "POC"; //set type of collection line submit
                    updateSignatureData.Changes[0].Properties[4].Value = "Collected"; //set status of collection line 
                    //deserialize with signature form object
                    collectionRequiredData = JsonConvert.SerializeObject(updateSignatureData);
                    //response for collection line submit 
                    var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionPostDataURl, "POST", true, collectionRequiredData, LoginStorage.AccessToken);
                    //parse collection json
                    var parseSingaureResponse = JObject.Parse(signatureResponse);
                    //check API status
                    var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                    postResponse = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                    var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                    ObservableCollection<string> objCollectionPostObjects = new ObservableCollection<string>()
                    {
                        LoginStorage.Site,
                        collectionworkOrderData.CollectionContractID,
                        item.LineNum,
                        "Collected"
                        //selectedStatus
                    };

                    var client3 = new HttpClient();
                    //MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string pocjsonData = JsonConvert.SerializeObject(objCollectionPostObjects);
                    var pocproperties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, pocjsonData, LoginStorage.AccessToken);
                    var pocdynJson = JObject.Parse(pocproperties);
                    var pocres = pocdynJson.SelectToken("Success");
                }
            }
            //Insert data into local DB
            collectionworkOrderData.CollectionLineNumbers = string.Join(",", linesData);
            pocDocument.CollectionLineNumbers = string.Join(",", linesData);
            con.Insert(pocDocument);
            con.Insert(collectionworkOrderData);
            var isApptExists = con.Table<CollectionLineData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).Count();
            var appts = con.Table<CollectionLineData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();
            if (isApptExists > 0)
            {
                foreach (var item in appts)
                {
                    foreach (var lines in GetListItems)
                    {
                        if (item.Cont_line == lines.LineNum)
                        {
                            con.Table<CollectionLineData>().Delete(x => x.Cont_line == lines.LineNum);
                        }
                    }

                }
            }
            var Linesdata = con.Table<CollectionLineData>().Where(a => a.Contract == AppointmentDetailPage.objContractId).ToList();

            await DisplayAlert("Notification", "Collection Lines has been submitted successfully", "ok");
            await Navigation.PopAsync();
        }
    }



public class CollectionLineDetails
    {
        public string Coll_LineNum { get; set; }
        public string Coll_ItemName { get; set; }
        public string Coll_Quantity { get; set; }
        public string Coll_Unit { get; set; }
    }
    public class GetPOCId
    {
        public string IDPODPOC { get; set; }
        public string contract_1 { get; set; }
        public string _ItemID { get; set; }
        public string Bookmark { get; set; }
    }
}

