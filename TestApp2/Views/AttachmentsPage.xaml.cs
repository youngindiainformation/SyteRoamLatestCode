using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SQLite;
using TestApp2.Models.SQLite;
using System.IO;
using TestApp2.ViewModels;
using System.Net.Http;
using TestApp2.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AttachmentsPage : ContentPage
    {
        private SQLiteConnection con;
        private ObservableCollection<AttachmentsViewModel> myList;
        public ObservableCollection<AttachmentsViewModel> LocalDBData { get; set; } = new ObservableCollection<AttachmentsViewModel>();
        public static string objContractId { get; set; }
        public string jtokenStr1 = "";

        public ObservableCollection<AttachmentsViewModel> MyList
        {
            get { return myList; }
            set { myList = value; }
        }


        public AttachmentsPage(ObservableCollection<AttachmentsViewModel> list,string contractID)
        {
            InitializeComponent();

            con = DependencyService.Get<ISQLite>().GetConnection();           
            this.BindingContext = this;
            objContractId = contractID;
            List<Attachments> loacalDbData = con.Table<Attachments>().Where(a => a.ContractId == objContractId).ToList();
            foreach (var item in loacalDbData)
            {
                LocalDBData.Add(new AttachmentsViewModel { Name = item.AttachmentTitle });
            }

            MyList = new ObservableCollection<AttachmentsViewModel>();
            //MyList = list;
            MyList = LocalDBData;
            LocalDBData = new ObservableCollection<AttachmentsViewModel>();
            
            ATTLIST.ItemsSource = MyList;
            if (MyList.Count > 0)
            {
                SyncAttachments.IsEnabled=true;
            }
            else
            {
                SyncAttachments.IsEnabled = false;
            }
        }
        private async void SyncAttachmentsClicked(object sender, EventArgs e)
        {

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                //http://117.203.102.179:8085/IDORequestService/ido/load/ERS_RentalContractObjectDocument?clm=ERS_RentalContractObjectDocumentSP&clmparam=SC00000099,sumanjpgname,jpg,null 
                List<Attachments> loacalDbData = con.Table<Attachments>().Where(a => a.ContractId == objContractId).ToList();
                string finalResponse = "";
                string type = "jpg";
                foreach (var item in loacalDbData)
                {

                    if (item.DataType == "application/pdf")
                    {
                        type = "pdf";
                    }
                    var APIData = objContractId + "," + item.AttachmentTitle + "," + type + "," + "null";
                    //var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                    string imageValue = Convert.ToBase64String(item.DataInBytes);

                    var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_RentalContractObjectDocument?clm=ERS_RentalContractObjectDocumentSP&clmparam=" + APIData;
                    var client3 = new HttpClient();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    //check items list
                    var checkStaus = dynJson.SelectToken("Items");
                    //to set final format of response
                    string finalResult = checkStaus.ToString(Newtonsoft.Json.Formatting.None);
                    var url = dynJson.SelectToken("Success");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr == "true")
                    {
                        //var upload = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/DocumentObjects?Properties=DocumentName&filter=DocumentName=" + "'" + item.AttachmentTitle + "'";
                        //var properties1 = mongooseAPIRequest.ProcessRestAPIRequest(upload, "GET", true, null, LoginStorage.AccessToken);
                        //var dynJson1 = JObject.Parse(properties1);
                        //var res = dynJson1.SelectToken("Items");
                        //string jtokenStr1 = res.ToString(Newtonsoft.Json.Formatting.None);
                        //if (jtokenStr1 != "null")
                        //{
                        var ItemId = GetItemId(finalResult); //UpdateDocumentObject_ExtSp   //DocumentObjects //
                        var postDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/DocumentObjects?refresh=true";
                        //string jsonData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"CustDelSignature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DelDate\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"LineStatus\",\"Value\":\"Delivered\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustDelName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"contract_1\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"cont_line\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                        string jsonData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[DocumentObject]doc.ID=[4f5061ae-72aa-49ae-a701-0193583ea763]doc.DT=[2022-05-2017:45:54.043]\",\"Properties\":[{\"Name\":\"DocumentObject\",\"Value\":\"iVBORw0KGgoAAAANSUhEUgAAAPMAAAA2CAYAAAAS\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                        var attachmentData = JsonConvert.DeserializeObject<AprooveData>(jsonData);

                        attachmentData.Changes[0].ItemId = ItemId;
                        attachmentData.Changes[0].Properties[0].Value = imageValue;

                        string finalJson = JsonConvert.SerializeObject(attachmentData);
                        var postResponse = mongooseAPIRequest.ProcessRestAPIRequest(postDataURl, "POST", true, finalJson, LoginStorage.AccessToken);
                        var postresJson = JObject.Parse(postResponse);
                        var url1 = postresJson.SelectToken("Success");
                        finalResponse = url1.ToString(Newtonsoft.Json.Formatting.None);
                        // }
                    }
                    //else
                    //{
                    //    await DisplayAlert("", "Please enter valid data", "Ok");
                    //}
                }
                if (finalResponse == "true")
                {
                    await DisplayAlert("Success Message", "Successfully Submited", "ok");
                    await Navigation.PopAsync();
                    con.Table<Attachments>().Delete(a => a.ContractId == objContractId);
                }
                else
                {
                    await DisplayAlert("Error Message", "Invalid Data", "ok");
                }
            }
            else
            {
                await DisplayAlert("Error Message", "Please check your internet connection", "OK");
            }
        }
        string ItemId = null;

        public string GetItemId(string JToken)
        {
            ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
            JArray array = JArray.Parse(JToken);
            for (int i = 0; i < array.Count; i++)
            {
                JToken elem = array[i];
                // token string
                string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                ItemId = data._ItemId;
            }
            return ItemId;
        }
        private async void AddDocumentClicked(object sender, EventArgs e)
        {
            var file = await FilePicker.PickAsync();
            var fileName = file.FileName;
            var stream = await file.OpenReadAsync();
            var buffer = new byte[16 * 1024];
            var arr = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                arr = ms.ToArray();
            }
            var type = file.ContentType;
            var isAttachmentExits = (from ud in con.Table<Attachments>().Where(a => a.AttachmentTitle == fileName) select ud).Count();

            if (isAttachmentExits == 0)
            {
                Attachments insertAttDetails = new Attachments();
                var split = fileName.Split('/');
                var DocName = split.Last();
                string DocumentTitle = objContractId + "/documents/" + DocName;
                insertAttDetails.AttachmentTitle = DocumentTitle;
                insertAttDetails.ContractId = objContractId;
                insertAttDetails.DataInBytes = arr;
                insertAttDetails.DataType = type.ToString();
                con.Insert(insertAttDetails);
                List<Attachments> loacalDbData = con.Table<Attachments>().Where(a => a.ContractId == objContractId).ToList();
                foreach (var item in loacalDbData)
                {
                    LocalDBData.Add(new AttachmentsViewModel { Name = item.AttachmentTitle });
                }
                MyList = LocalDBData;
                ATTLIST.ItemsSource = MyList;
                //MyList.Add(new AttachmentsViewModel { Name = file.FullPath });

            }

        }

        protected override void OnAppearing()
        {
            this.BindingContext = this;

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
}