using TestApp2.Common.Helper;
using TestApp2.Models;

using TestApp2.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using TestApp2.Models.SQLite;
using TestApp2.Common.APIclasses;
using Acr.UserDialogs;
using TestApp2.ViewModels;
using Xamarin.Essentials;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestApp2SyteLineServicePageDetail : ContentPage
    {
        private SQLiteConnection con;

        public string ContractId { get; set; }

        public string Reference_Number { get; set; }

        public delegate void Notify();

        public event Notify ProcessCompleted;

        string ItemId = null;
        string postResponse = "false";
        string jtokenStr = "false";

        public TestApp2SyteLineServicePageDetail()
        {
            InitializeComponent();
            UserDialogs.Instance.HideLoading();
            con = DependencyService.Get<ISQLite>().GetConnection();            
            BindingContext = this;
            syncDataOnline.Text = "Sync is Completed";
            syncDataOnline.TextColor = Color.Green;
            
            //if(LoginStorage.IsCloudConfig == true)
            //{
            //    con.DeleteAll<AppointmentList>();
            //    con.DeleteAll<ContractDetails>();
            //    //con.DeleteAll<Attachments>();
            //    con.DeleteAll<ContractLineDetails>();
            //    con.DeleteAll<ContractLineSiteData>();
            //    con.DeleteAll<CustomerListData>();
            //    con.DeleteAll<WorkOrderData>();
            //    con.DeleteAll<PostAppointmentTemp>();
            //    con.DeleteAll<CollectionLineData>();
            //    con.DeleteAll<CollectionWorkOrderData>();
            //    con.DeleteAll<CollectionLineDetailsTable>();
            //    con.DeleteAll<YardReturnLineData>();
            //    con.DeleteAll<YardReturnPostTemplate>();
            //    con.DeleteAll<YardPickingLinesData>();
            //    con.DeleteAll<YardPickupPostTemplate>();
            //    con.DeleteAll<YardPickingLineDetails>();
            //    con.DeleteAll<YardReturnLineDetails>();
            //    con.DeleteAll<PocDocumentDetails>();
            //    //con.DeleteAll<PodDocumentDetails>();
            //}

        }

        protected override void OnAppearing()
        {
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            base.OnAppearing();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            Networkstatus();
            GetUserDetailsFromLoaclDB();

            //This API call is maid to get all the data from the application in one go.
            //GetAppointmentsList(LoginStorage.PartnerId);
            UserDialogs.Instance.HideLoading();
        }
        //Get the data from local DB
        void GetUserDetailsFromLoaclDB()
        {
            var userDetails = con.Table<UserDetails>().ToList();
            foreach (var user_Detail in userDetails)
            {
                LoginUname.Text = user_Detail.Username;
                AppCount.Text = user_Detail.ApptCount;
                LoginSite.Text = user_Detail.Site;
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        public void Networkstatus()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                LoginStatus.Text = "Online";
                LoginStatus.TextColor = Color.Green;
                SyncDataOnline();
            }
            else
            {
                LoginStatus.Text = "Offline";
                LoginStatus.TextColor = Color.Red;
                syncDataOnline.Text = "Sync is Completed";
                syncDataOnline.TextColor = Color.Green;
            }
        }
        public void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var current = e.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                LoginStatus.Text = "Online";
                LoginStatus.TextColor = Color.Green;
                
            }
            else
            {
                LoginStatus.Text = "Offline";
                LoginStatus.TextColor = Color.Red;
            }
        }
        private async void AppointmentsClicked(object sender, EventArgs e)
        {

            UserDialogs.Instance.ShowLoading("Loading please wait...");
            await Task.Delay(2000);
            await Navigation.PushAsync(new AppointmentsHomePage("Appointment "));
            UserDialogs.Instance.HideLoading();
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        private void TestApp2MenuClicked(object sender, EventArgs e)
        {
            DisplayAlert("", "Synchronization", "Cancel");
        }
        public async void GetAppointmentsList(string objPartnerId)
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    ObservableCollection<ScheduleDetails> scheduleDetails = new ObservableCollection<ScheduleDetails>();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string getRentalContractAppointmentsByPartnerIdURL =RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_RentalSchedule?clm=RentalgetRentalContractAppointmentsByPartnerIdSp&clmparam=" + LoginStorage.Site + "," + objPartnerId + "," + "R";
                    var rentalContractAppointments = mongooseAPIRequest.ProcessRestAPIRequest(getRentalContractAppointmentsByPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(rentalContractAppointments);
                    var url = dynJson.SelectToken("Items");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        JArray array = JArray.Parse(jtokenStr);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<AppointmentsList>(jtokenString);
                            AppointmentList appointmentDetails = new AppointmentList();
                            var isApptExists = con.Table<AppointmentList>().Where(a => a.Ref_num == data.ref_num).Count();
                            if (isApptExists == 0)
                            {
                                appointmentDetails.Status = data.appt_stat;
                                appointmentDetails.Appt_type = data.appt_type;
                                appointmentDetails.Complete = data.Complete;
                                appointmentDetails.Description = data.description;
                                appointmentDetails.Hrs = data.hrs;
                                appointmentDetails.Ref_num = data.ref_num;
                                appointmentDetails.Sched_date = data.sched_date;
                                appointmentDetails.Partner_id = LoginStorage.PartnerId;
                                con.Insert(appointmentDetails);
                                GetAppointmentListDetails(data.ref_num);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }
        public async void GetAppointmentListDetails(string objContractId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string GetContractsDetailsByContractIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractsDetailsByContractId?clm=ERS_GetContractsDetailsByContractIdSP&clmparam=" + objContractId;
                    var RentalContractAppointmentsByContractId = mongooseAPIRequest.ProcessRestAPIRequest(GetContractsDetailsByContractIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(RentalContractAppointmentsByContractId);
                    var url = dynJson.SelectToken("Items");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        JArray array = JArray.Parse(jtokenStr);
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<ContractDetailsAPI>(jtokenString);
                            ContractDetails contractDetails = new ContractDetails();
                            var isContractExists = con.Table<ContractDetails>().Where(a => a.Contract == data.Contract).Count();
                            if (isContractExists == 0)
                            {
                                contractDetails.Contract = data.Contract;
                                contractDetails.Description = AppointmentsHomePage.ApptDescription;
                                contractDetails.Start_date = data.Start_date;
                                contractDetails.End_date = data.End_date;
                                contractDetails.Name = data.Name;
                                contractDetails.Cust_num = data.Cust_num;
                                contractDetails.Addr1 = data.Addr1;
                                contractDetails.City = data.City;
                                contractDetails.State = data.State;
                                contractDetails.Country = data.Country;
                                contractDetails.Zip = data.Zip;
                                contractDetails.Phone = data.Phone;
                                contractDetails.Email = data.Email;
                                contractDetails.Status = data.Cont_stat;
                                contractDetails.Billing_freq = data.BillingFreq;
                                contractDetails.Billing_type = data.BillingType;
                                contractDetails.appointment_type = AppointmentsHomePage.ApptType;
                                con.Insert(contractDetails);
                                GetDeliveryLines(data.Contract);
                                GetCollectionLines(data.Contract);
                            }
                        }
                    }
                }

                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }

        public void GetDeliveryLines(string objContractId)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContractLinesBasicInfoByContractIdSP&clmparam=" + objContractId;
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
                            var data = JsonConvert.DeserializeObject<ContractLinesData>(jtokenString);
                            List<ContractLineDetails> contractLinesList = (from ld in con.Table<ContractLineDetails>().Where(a => a.Contract == data.contract && a.Cont_line == data.cont_line) select ld).ToList();
                            if (contractLinesList.Count == 0)
                            {
                                ContractLineDetails contractLineDetails = new ContractLineDetails();
                                contractLineDetails.Cont_line = data.cont_line;
                                contractLineDetails.Ser_num = data.ser_num;
                                contractLineDetails.Item = data.item;
                                contractLineDetails.Description = data.description;
                                contractLineDetails.Qty = data.qty;
                                contractLineDetails.Contract = data.contract;
                                contractLineDetails.Site_ref = data.site_ref;
                                con.Insert(contractLineDetails);
                                //GetDeliveryLineDetails(data.contract, data.cont_line);
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }
        }

        public void GetCollectionLines(string objContractId)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //get Collection API URK
                    string collectionBasicInfoURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContLinesCollectionBasicInfoByContractSP&clmparam=" + objContractId;
                    //get result data
                    var getCollectionData = mongooseAPIRequest.ProcessRestAPIRequest(collectionBasicInfoURL, "GET", true, null, LoginStorage.AccessToken);
                    //pars json
                    var collectionJson = JObject.Parse(getCollectionData);
                    //select items 
                    var collectionItems = collectionJson.SelectToken("Items");
                    //remove formater 
                    string collectionResult = collectionItems.ToString(Newtonsoft.Json.Formatting.None);
                    if (collectionResult != "null")
                    {
                        ObservableCollection<CollectionLinesListDetails> collectionLinesListDetails = new ObservableCollection<CollectionLinesListDetails>();
                        JArray collectionArray = JArray.Parse(collectionResult);
                        for (int i = 0; i < collectionArray.Count; i++)
                        {
                            JToken elem = collectionArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var collectionDeserialize = JsonConvert.DeserializeObject<ContractLineDetails>(jtokenString);
                            List<CollectionLineData> collectionLinesList = (from ld in con.Table<CollectionLineData>().Where(a => a.Contract == collectionDeserialize.Contract && a.Cont_line == collectionDeserialize.Cont_line) select ld).ToList();
                            //bind local table 
                            if (collectionLinesList.Count == 0)
                            {
                                CollectionLineData collectionLines = new CollectionLineData();
                                collectionLines.Cont_line = collectionDeserialize.Cont_line;
                                collectionLines.Ser_num = collectionDeserialize.Ser_num;
                                collectionLines.Item = collectionDeserialize.Item;
                                collectionLines.Description = collectionDeserialize.Description;
                                collectionLines.Qty = collectionDeserialize.Qty;
                                collectionLines.Contract = collectionDeserialize.Contract;
                                collectionLines.Site_ref = collectionDeserialize.Site_ref;
                                con.Insert(collectionLines);
                            }
                        }
                    }
                    var appts = con.Table<CollectionLineData>().Where(a => a.Contract == objContractId).ToList();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }

        }

        public async void SyncDataOnline()
        {
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            List<WorkOrderData> workOrderData = con.Table<WorkOrderData>().ToList();
            if (workOrderData.Count > 0)
            {
                syncDeliveryLinesData(workOrderData);
            }
            List<PostAppointmentTemp> appointmentData = con.Table<PostAppointmentTemp>().ToList();
            if (appointmentData.Count > 0)
            {
                SyncAppointmentsData(appointmentData);
            }
            List<CollectionWorkOrderData> colletionLines = con.Table<CollectionWorkOrderData>().ToList();
            if(colletionLines.Count > 0)
            {
                syncCollectionLinesData(colletionLines);
            }
            List<YardReturnPostTemplate> postYardReturnData = con.Table<YardReturnPostTemplate>().ToList();
            if(postYardReturnData.Count >0)
            {
                SyncYardReturnLines(postYardReturnData);
            }
            List<YardPickupPostTemplate> postYardPickupData = con.Table<YardPickupPostTemplate>().ToList();
            if(postYardPickupData.Count > 0)
            {
                SaveYardPickupLinesData(postYardPickupData);
            }
            if(jtokenStr1 == "true" && postResponse =="true")
            {
                await DisplayAlert("", "Saved Successfully!!", "Ok");
            }
            UserDialogs.Instance.HideLoading();
        }

        string jtokenStr1 = "";

        public string SyncAppointmentsData(List<PostAppointmentTemp> postappdata)
        {
           
            var appointdata = postappdata.ToArray();
            foreach (var item in appointdata)
            {
                var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>()
                    {
                    LoginStorage.Site,
                    LoginStorage.PartnerId,
                    "R",
                    item.Description,
                    item.Duration,
                    item.RefNum,
                    item.AppDate,
                    item.AppStatus,
                    item.Cswitch,
                    item.ApptType,
                    item.TaskSeqNumber,
                    };
               
                var client3 = new HttpClient();
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();

                string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                var dynJson = JObject.Parse(properties);
                var res = dynJson.SelectToken("Success");
                var url = dynJson.SelectToken("Success");
                jtokenStr1 = url.ToString(Newtonsoft.Json.Formatting.None);

            }
            if (jtokenStr1 == "true")
            {
                //con.DeleteAll<PostAppointmentTemp>();
            }
            else
            {
                //await DisplayAlert("", "Please enter valid data", "Ok");
            }
            return jtokenStr1;
        }

        MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();

        public async void syncDeliveryLinesData(List<WorkOrderData> workOrderdata)
        {
            var workOrderInfo = workOrderdata.ToArray();
            string POCID = "";
            if (workOrderInfo.Length > 0)
            {
                List<string> linesData = new List<string>();
                foreach (var item in workOrderInfo)
                {
                    linesData.Add(item.LineNumbers);
                }
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    foreach (var item in workOrderInfo)
                    {
                        //prepared the required deliver data for submit
                        var deliverData = LoginStorage.Site + "," + item.ContractID + "," + item.LineNumbers + "," + item.Quantity;
                        //get deliver data URL 
                        var getDeliverDataDataURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_Rental_POC_POD?clm=ERS_Rental_POC_POD_SP&clmparam=" + deliverData;
                        //intiate API request
                        var httpClient = new HttpClient();
                        MongooseAPIRequest objMongooseAPIRequest = new MongooseAPIRequest();
                        //get uinque POCID deliver from table 
                        var getDeliverResponse = mongooseAPIRequest.ProcessRestAPIRequest(getDeliverDataDataURL, "GET", true, null, LoginStorage.AccessToken);
                        //parse deliver json
                        var collectionJson = JObject.Parse(getDeliverResponse);
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
                        var deliverResponsePostDataURl = RestApiConstants.BaseUrl + "/IDORequestService/ido/update/ERS_Rental_POC_POD?refresh=true";
                        var accessToken = LoginStorage.AccessToken;
                        //prepare the update object with respective data
                        //USE -  "POD" as type of collection line submit 
                        string deliverRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                        //deserialize with signature form object
                        var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(deliverRequiredData);
                        updateSignatureData.Changes[0].ItemId = ItemId;
                        updateSignatureData.Changes[0].Properties[0].Value = item.Signature;
                        updateSignatureData.Changes[0].Properties[1].Value = item.SubmitDate;
                        updateSignatureData.Changes[0].Properties[2].Value = item.Name;
                        updateSignatureData.Changes[0].Properties[3].Value = "POD"; //set type of Deliver line submit
                        updateSignatureData.Changes[0].Properties[4].Value = "Delivered"; //set status of collection line 
                                                                                          //deserialize with signature form object
                        deliverRequiredData = JsonConvert.SerializeObject(updateSignatureData);
                        //response for deliver line submit 
                        var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(deliverResponsePostDataURl, "POST", true, deliverRequiredData, LoginStorage.AccessToken);
                        //parse deliver json
                        var parseSingaureResponse = JObject.Parse(signatureResponse);
                        //check API status
                        var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                        postResponse = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                        var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                        ObservableCollection<string> objCollectionPostObjects = new ObservableCollection<string>()
                        {
                            LoginStorage.Site,
                            item.ContractID,
                            item.LineNumbers,
                            "Delivered"
                        };

                        var client3 = new HttpClient();
                        //MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                        string pocjsonData = JsonConvert.SerializeObject(objCollectionPostObjects);
                        var pocproperties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, pocjsonData, LoginStorage.AccessToken);
                        var pocdynJson = JObject.Parse(pocproperties);
                        var pocres = pocdynJson.SelectToken("Success");
                        var pocres1 = pocres.ToString(Newtonsoft.Json.Formatting.None);
                        if (pocres1 == "true")
                        {
                            con.Table<WorkOrderData>().Delete(x => x.LineNumbers == item.LineNumbers && x.ContractID == item.ContractID);
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("", "Your data is already updated", "Ok");
            }
        }

        public void syncCollectionLinesData(List<CollectionWorkOrderData> collectionWorkOrders)
        {
            var WorkOrderData = collectionWorkOrders.ToArray();
            MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
            string ItemId = null;
            string POCID = null;
            List<string> linesData = new List<string>();
            foreach (var item in WorkOrderData)
            {
                linesData.Add(item.CollectionLineNumbers);
            }
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                string Quantity = "0";
                foreach (var item in WorkOrderData)
                {
                    if (item.CollectionQuantity == null || item.CollectionQuantity == "")
                    {
                        Quantity = "0";
                    }
                    else
                    {
                        Quantity = item.CollectionQuantity;
                    }
                    //prepared the required collection data for submit
                    var collectionData = LoginStorage.Site + "," + item.CollectionContractID + "," + item.CollectionLineNumbers + "," + Quantity;
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
                    string collectionRequiredData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    //deserialize with signature form object
                    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(collectionRequiredData);
                    updateSignatureData.Changes[0].ItemId = ItemId;
                    updateSignatureData.Changes[0].Properties[0].Value = item.CollectionSignature;
                    updateSignatureData.Changes[0].Properties[1].Value = item.CollectionSubmitDate;
                    updateSignatureData.Changes[0].Properties[2].Value = item.CollectionName;
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
                        item.CollectionContractID,
                        item.CollectionLineNumbers,
                        "Collected"
                    };

                    var client3 = new HttpClient();
                    string pocjsonData = JsonConvert.SerializeObject(objCollectionPostObjects);
                    var pocproperties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, pocjsonData, LoginStorage.AccessToken);
                    var pocdynJson = JObject.Parse(pocproperties);
                    var pocres = pocdynJson.SelectToken("Success");
                    var pocres1 = pocres.ToString(Newtonsoft.Json.Formatting.None);

                    if (pocres1 == "true")
                    {
                        con.Table<CollectionWorkOrderData>().Delete(x => x.CollectionLineNumbers == item.CollectionLineNumbers && x.CollectionContractID == item.CollectionContractID);
                    }
                }
            }
        }

        public async void SaveSignatureYardReturn(string signature,string lineNum,string contractId,string selectedItem,string signedTime,string objQty)
        {
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
                    var yardPickupData = LoginStorage.Site + "," + contractId + "," + lineNum + "," + objQty;
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
                    string yardPickUpData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    //deserialize with signature form object
                    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(yardPickUpData);
                    updateSignatureData.Changes[0].ItemId = ItemId;
                    updateSignatureData.Changes[0].Properties[0].Value = signature;
                    updateSignatureData.Changes[0].Properties[1].Value = signedTime;
                    updateSignatureData.Changes[0].Properties[2].Value = LoginStorage.Username;
                    updateSignatureData.Changes[0].Properties[3].Value = "POR";
                    updateSignatureData.Changes[0].Properties[4].Value = selectedItem;
                    yardPickUpData = JsonConvert.SerializeObject(updateSignatureData);
                    //response for collection line submit 
                    var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionPostDataURl, "POST", true, yardPickUpData, LoginStorage.AccessToken);
                    //parse collection json
                    var parseSingaureResponse = JObject.Parse(signatureResponse);
                    //check API status
                    var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                    var postResponse1 = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                    if (postResponse1 == "true")
                    {
                        con.Table<YardReturnPostTemplate>().Delete(x => x.LineNumber == lineNum && x.contractID == contractId);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "Please Select the status and sign to save", "ok");
            }                
        }

        private async void SyncYardReturnLines(List<YardReturnPostTemplate> yardReturnPostTemplate)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    foreach (var item in yardReturnPostTemplate)
                    {
                        var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                        ObservableCollection<string> objYardReturnPostObjects = new ObservableCollection<string>()
                        {
                            LoginStorage.Site,
                            item.contractID,
                            item.LineNumber,
                            item.SelectedStatus
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
                            SaveSignatureYardReturn(item.Signature, item.LineNumber, item.contractID, item.SelectedStatus,item.SignedTime,item.objQty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "", "ok");
            }
        }
        
        public void SaveYardPickupLinesSignature(string signature,string Linenum,string contractId,string selectedStatus,string signedTime, string objQty)
        {
            
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string ItemId = null;
                string POCID = null;
                string postResponse = "";
                List<string> linesData = new List<string>();
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    //prepared the required collection data for submit
                    var yardPickupData = LoginStorage.Site + "," + contractId + "," + Linenum +","+ objQty;
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
                    string yardPickUpData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"Signature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DatePODPOC\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustomerName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"TypePODPOC\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"Status\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"IDPODPOC\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                    //deserialize with signature form object
                    var updateSignatureData = JsonConvert.DeserializeObject<AprooveData>(yardPickUpData);
                    updateSignatureData.Changes[0].ItemId = ItemId;
                    updateSignatureData.Changes[0].Properties[0].Value = signature;
                    updateSignatureData.Changes[0].Properties[1].Value = signedTime;
                    updateSignatureData.Changes[0].Properties[2].Value = LoginStorage.Username;
                    updateSignatureData.Changes[0].Properties[3].Value = "POP";
                    updateSignatureData.Changes[0].Properties[4].Value = selectedStatus;
                    yardPickUpData = JsonConvert.SerializeObject(updateSignatureData);
                    //response for collection line submit 
                    var signatureResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionPostDataURl, "POST", true, yardPickUpData, LoginStorage.AccessToken);
                    //parse collection json
                    var parseSingaureResponse = JObject.Parse(signatureResponse);
                    //check API status
                    var selctSucessItems = parseSingaureResponse.SelectToken("Success");
                    postResponse = selctSucessItems.ToString(Newtonsoft.Json.Formatting.None);
                    if(postResponse == "true")
                    {
                        con.Table<YardPickupPostTemplate>().Delete(x => x.LineNumber == Linenum && x.contractID == contractId);
                    }
            }
        }
        private async void SaveYardPickupLinesData(List<YardPickupPostTemplate> pickupPostTemplates)
        {

                try
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        foreach (var item in pickupPostTemplates)
                        {
                        if (item.Flag == "0")
                        {


                            var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateLineStatusByContractID?method=ERS_GetUpdateLineStatusByContractIDSP";
                            ObservableCollection<string> objYardReturnPostObjects = new ObservableCollection<string>()
                            {
                                LoginStorage.Site,
                                item.contractID,
                                item.LineNumber,
                                item.SelectedStatus
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
                                SaveYardPickupLinesSignature(item.Signature, item.LineNumber, item.contractID, item.SelectedStatus, item.SignedTime, item.objQty);
                            }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("", ex.Message, "ok");
                }
            
        }

        async void refreshView_Refreshing(System.Object sender, System.EventArgs e)
        {
            GetUserDetailsFromLoaclDB();
            refreshView.IsRefreshing = false;
        }
        public async void GetUserDetails()
        {
            con.DeleteAll<UserDetails>();
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_UserDataAndPartnerIdBySiteRef?clm=ERS_UserDataAndPartnerIdBySiteRefSP&clmparam=" + LoginStorage.Username + "," + LoginStorage.Site;
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var userdata = JsonConvert.DeserializeObject<UserDataProperties>(properties);
                    var dynJson = JObject.Parse(properties);
                    var url = dynJson.SelectToken("Items");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        JArray array = JArray.Parse(jtokenStr);
                    if (array.Count > 0)
                    {
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<UserDetails>(jtokenString);
                            LoginStorage.Username = data.Username;
                            LoginStorage.PartnerId = data.PartnerId;
                            UserDetails userDetails = new UserDetails();
                            var isUserExists = con.Table<UserDetails>().Where(a => a.PartnerId == data.PartnerId).Count();
                            userDetails.PartnerId = data.PartnerId;
                            userDetails.UserId = data.UserId;
                            userDetails.Username = data.Username;
                            userDetails.UserPassword = LoginStorage.Password;
                            userDetails.ApptCount = data.AppointmentCount;
                            userDetails.Site = data.Site_ref;
                            userDetails.Site_ref = data.Site_ref;
                            con.Insert(userDetails);
                        }
                    }
                    else
                    {
                        UserDetails userDetails = new UserDetails();
                        userDetails.Username = LoginStorage.Username;
                        userDetails.ApptCount = "0";
                        userDetails.Site = LoginStorage.Site;
                        userDetails.Site_ref = LoginStorage.Site;
                        con.Insert(userDetails);
                    }
                    }

                }

        }

    }
}