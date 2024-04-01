using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Media;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using Plugin.Geolocator;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using SQLite;
using TestApp2.Views;
using TestApp2.Models.SQLite;
using TestApp2.Common.Helper;
using TestApp2.Common.APIclasses;
using Acr.UserDialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TestApp2.ViewModels;
using NativeMedia;
using TestApp2.Models;
using Plugin.Permissions;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppointmentDetailPage : ContentPage
    {
        public ObservableCollection<AttachmentsViewModel> AttachmentList { get; set; } = new ObservableCollection<AttachmentsViewModel>();
        public List<LineItems> ItemsList { get; set; } = new List<LineItems>();
        public ObservableCollection<AttachmentsViewModel> LocalDBData { get; set; } = new ObservableCollection<AttachmentsViewModel>();

        private SQLiteConnection con = DependencyService.Get<ISQLite>().GetConnection();
        public static string objContractId { get; set; }
        public string objContractStatus { get; set; }
        public string objStreet { get; set; }
        public string objCity { get; set; }
        public string objCountry { get; set; }
        public static string objCustName { get; set; }
        public static string objCust_num { get; set; }
        public static string objCust_seq { get; set; }
        public bool isRefreshingView { get; set; } = false;
        ScheduleDetails Details = new ScheduleDetails();
        // AppointmentsHomePage homePage = new AppointmentsHomePage();

        public AppointmentDetailPage()
        {
            InitializeComponent();
        }
        public AppointmentDetailPage(ScheduleDetails appointmentsListObject)
        {

            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                InitializeComponent();
                objContractId = appointmentsListObject.Contractid;
                objCustName = appointmentsListObject.customerName;
                Details = appointmentsListObject;
                UpdateLocalSavedDataToServer();
                GetAppStatusList();
                GetAppointmentTypeList();
                BindingDetailsToUi(appointmentsListObject);
                UserDialogs.Instance.HideLoading();
                GetCollectionLineDetails(objCust_num, objCust_seq, objContractId);
            }
            catch (Exception ex)
            {

                
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        void RefreshData()
        {
            GetAppointmentsList();
            var data = con.Table<AppointmentList>().Where(a => a.Ref_num == Details.Contractid);
            foreach (var item in data)
            {
                objCust_seq = item.Cust_seq;
                NotesSubject.Text = item.notes_subject;
                RefNum.Text = item.Ref_num;
                objCustName = item.Name;
                objCust_num = item.Cust_num;
                CustomerName.Text = item.Name;
                AddressLabel.Text = item.Addr1 + " " + item.City + " " + item.State + " " + item.Zip + " " + item.Country;
                objStreet = item.Addr1;
                objCity = item.City;
                objCountry = item.Country;
                PhoneEntry.Text = item.Phone;
                Notes.Text = item.Notes;
                EmailAddress.Text = item.Email;
                var data1 = GetAppType(item.Status);
                AppStatusList.SelectedIndex = GetAppointmentStatus(data1);
                AppointmentType.SelectedIndex = GetAppointmentType(item.Appt_type);
                objContractStatus = item.Cont_stat;
                Duration.Text = item.Hrs.Substring(0,4);
                //AppDate.Date = DateTime.Parse(item.Sched_date);
                DateTime d = DateTime.Parse(AppointmentsHomePage.ApptDate);
                //AppTime.Time = new TimeSpan(d.Hour, d.Minute, d.Second);
                Description.Text = item.Description;
                PartnerName.Text = LoginStorage.Username;
                TaskSeqNumber.Text = item.task_seq;
            }
        }
        void BindingDetailsToUi(ScheduleDetails appointmentsListObject)
        {
                objCust_seq = appointmentsListObject.custSeq;
                NotesSubject.Text = appointmentsListObject.notes_subject;
                RefNum.Text = appointmentsListObject.Contractid;
                objCustName = appointmentsListObject.Name;
                objCust_num = appointmentsListObject.Cust_num;
                CustomerName.Text = appointmentsListObject.Name;
                AddressLabel.Text = appointmentsListObject.Addr1 + " " + appointmentsListObject.City + " " + appointmentsListObject.State + " " + appointmentsListObject.Zip + " " + appointmentsListObject.Country;
                objStreet = appointmentsListObject.Addr1;
                objCity = appointmentsListObject.City;
                objCountry = appointmentsListObject.Country;
                PhoneEntry.Text = appointmentsListObject.Phone;
                Notes.Text = appointmentsListObject.Notes;
                EmailAddress.Text = appointmentsListObject.Email;
                AppStatusList.SelectedIndex = GetAppointmentStatus(AppointmentsHomePage.ApptStatus);
                AppointmentType.SelectedIndex = GetAppointmentType(AppointmentsHomePage.ApptType);
                objContractStatus = appointmentsListObject.Cont_stat;
                Duration.Text = AppointmentsHomePage.ApptDuration;
                Description.Text = AppointmentsHomePage.ApptDescription;
                PartnerName.Text = LoginStorage.PartnerId;
                TaskSeqNumber.Text = appointmentsListObject.task_seq;
                AppDate.Date = DateTime.Parse(AppointmentsHomePage.ApptDate.Substring(0, 10));
                DateTime d = DateTime.Parse(AppointmentsHomePage.ApptDate);
                AppTime.Time = new TimeSpan(d.Hour, d.Minute, d.Second);
        }
        void GetCollectionLineDetails(string cusNumber, string cusSeq, string contNum)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                ObservableCollection<CustomerDetails> customerDetails = new ObservableCollection<CustomerDetails>();
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string getRentalContractAppointmentsByPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetAddressDetailsbyContractID?clm=ERS_GetAddressDetailsbyContractIDSP&clmparam=" + LoginStorage.Site + "," + contNum + "," + cusNumber + "," + cusSeq;
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
                        var data = JsonConvert.DeserializeObject<custDetails>(jtokenString);
                        CustomerListData customerData = new CustomerListData();
                        customerData.address = data.LocationAddressDetails;
                        customerData.Cus_BillingToName = data.BilltoContact;
                        customerData.Cus_BillingToEmail = data.BilltoEmail;
                        customerData.Cus_BillingToContact = data.BilltoPhone;
                        customerData.Cus_OrderContact = data.OrderPhone;
                        customerData.Cus_OrderEmail = data.OrderEmail;
                        customerData.Cus_OrderName = data.OrderContact;
                        customerData.Cus_ShipToContact = data.ShiptoPhone;
                        customerData.Cus_ShipToEmail = data.ShipToEmail;
                        customerData.Cus_ShipToName = data.ShiptoContact;
                        customerData.Cust_num = objContractId;
                        //customerData.CustomerName = data.Customer;
                        customerData.Cus_ShipToID = data.ShipTo;
                        customerData.Cus_ID = data.Customer;
                        con.Insert(customerData);
                    }
                }
            }
        }
        private async void SaveDataLocalDB(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                await Task.Delay(10);
                AppStatusTypeObj _selectedAppStatustypeObj = (AppStatusTypeObj)AppointmentType.SelectedItem;
                var SelectedAppStatusTypeObj = _selectedAppStatustypeObj.DisplayId;
                AppStatusObj _selctedappstatusobj = (AppStatusObj)AppStatusList.SelectedItem;
                var SelectedAppStatusObj = _selctedappstatusobj.StatusId;
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    string Cswitch = "0";
                    if (CompleteSwitch.IsToggled)
                    {
                        Cswitch = "1";
                    }
                    else
                    {
                        Cswitch = "0";
                    }
                    var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                    ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>()
                    {
                    LoginStorage.Site,
                    LoginStorage.PartnerId,
                    "R",
                    Description.Text,
                    Duration.Text,
                    RefNum.Text,
                    AppDate.Date.ToString("yyyyMMdd") + " " + AppTime.Time.ToString(),
                    SelectedAppStatusObj.ToString(),
                    Cswitch,
                    SelectedAppStatusTypeObj.ToString(),
                    TaskSeqNumber.Text,
                    };

                    var client3 = new HttpClient();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Success");
                    var url = dynJson.SelectToken("Success");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr == "true")
                    {
                        await DisplayAlert("", "Saved Successfully!!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("", "Please enter valid data", "Ok");
                    }
                }
                else
                {
                    string Cswitch = "0";
                    if (CompleteSwitch.IsToggled)
                    {
                        Cswitch = "1";
                    }
                    else
                    {
                        Cswitch = "0";
                    }
                    PostAppointmentTemp postAppointmentTemp = new PostAppointmentTemp();
                    postAppointmentTemp.Site = LoginStorage.Site;
                    postAppointmentTemp.PartnerId = LoginStorage.PartnerId;
                    postAppointmentTemp.ContractId = objContractId;
                    postAppointmentTemp.Cswitch = Cswitch;
                    postAppointmentTemp.Description = Description.Text;
                    postAppointmentTemp.Duration = Duration.Text;
                    postAppointmentTemp.RefNum = RefNum.Text;
                    postAppointmentTemp.AppDate = AppDate.Date.ToString("yyyyMMdd") + " " + AppTime.Time.ToString();
                    postAppointmentTemp.AppStatus = SelectedAppStatusObj.ToString();
                    postAppointmentTemp.ApptType = SelectedAppStatusTypeObj.ToString();
                    postAppointmentTemp.TaskSeqNumber = TaskSeqNumber.Text;
                    con.Insert(postAppointmentTemp);
                    await DisplayAlert("", "Saved Successfully!!", "Ok");
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        void UpdateLocalSavedDataToServer()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                int isAnyOfflineSavedDataavailable = con.Table<PostAppointmentTemp>().Where(a => a.ContractId == objContractId).Count();
                if (isAnyOfflineSavedDataavailable > 0)
                {
                    PostAppointmentTemp postAppointmentTemp = con.Table<PostAppointmentTemp>().Where(a => a.ContractId == objContractId).FirstOrDefault();
                    var uploadingUrl = RestApiConstants.BaseUrl+ "/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                    ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>() {
                    LoginStorage.Site,
                    LoginStorage.PartnerId,
                    "R",
                    postAppointmentTemp.Description,
                    postAppointmentTemp.Duration,
                    postAppointmentTemp.RefNum,
                    postAppointmentTemp.AppDate,
                    postAppointmentTemp.AppStatus,
                    postAppointmentTemp.Cswitch,
                    postAppointmentTemp.ApptType,
                    postAppointmentTemp.TaskSeqNumber,
                    };
                    var client3 = new HttpClient();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Success");
                    var url = dynJson.SelectToken("Success");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr == "true")
                    {
                        PostAppointmentTemp delete = con.Table<PostAppointmentTemp>().Where(a => a.ContractId == objContractId).FirstOrDefault();
                        con.Delete(delete);
                    }
                }
            }
        }

        public void GetAppStatusList()
        {
            try
            {
                List<AppStatusObj> appStatusList = new List<AppStatusObj>();
                appStatusList.Add(new AppStatusObj { StatusId = "15Remain", StatusDisplayName = "15 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "30Remain", StatusDisplayName = "30 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "45Remain", StatusDisplayName = "45 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "60Remain", StatusDisplayName = "60 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "75Remain", StatusDisplayName = "75 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "90Remain", StatusDisplayName = "90 Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "Accepted", StatusDisplayName = "Accepted" });
                appStatusList.Add(new AppStatusObj { StatusId = "Arrived", StatusDisplayName = "Arrived" });
                appStatusList.Add(new AppStatusObj { StatusId = "Assigned", StatusDisplayName = "Assigned" });
                appStatusList.Add(new AppStatusObj { StatusId = "Cancelled", StatusDisplayName = "Cancelled" });
                appStatusList.Add(new AppStatusObj { StatusId = "ClockedOff", StatusDisplayName = "Clocked Off" });
                appStatusList.Add(new AppStatusObj { StatusId = "ClockedOn", StatusDisplayName = "Clocked On" });
                appStatusList.Add(new AppStatusObj { StatusId = "Collected", StatusDisplayName = "Collected" });
                appStatusList.Add(new AppStatusObj { StatusId = "Completed", StatusDisplayName = "Completed" });
                appStatusList.Add(new AppStatusObj { StatusId = "Confirmed", StatusDisplayName = "Confirmed Appointment" });
                appStatusList.Add(new AppStatusObj { StatusId = "Delayed", StatusDisplayName = "Delayed Appointment" });
                appStatusList.Add(new AppStatusObj { StatusId = "InRoute", StatusDisplayName = "In Route" });
                appStatusList.Add(new AppStatusObj { StatusId = "InTransit", StatusDisplayName = "In Transit" });
                appStatusList.Add(new AppStatusObj { StatusId = "InProcess", StatusDisplayName = "In-Process Appointment" });
                appStatusList.Add(new AppStatusObj { StatusId = "New", StatusDisplayName = "New Appointment" });
                appStatusList.Add(new AppStatusObj { StatusId = "NewJob", StatusDisplayName = "New Job" });
                appStatusList.Add(new AppStatusObj { StatusId = "NotCollected", StatusDisplayName = "Not Collected" });
                appStatusList.Add(new AppStatusObj { StatusId = "NotStarted", StatusDisplayName = "Not Started" });
                appStatusList.Add(new AppStatusObj { StatusId = "OnSite", StatusDisplayName = "Onsite Service" });
                appStatusList.Add(new AppStatusObj { StatusId = "OtherRemain", StatusDisplayName = "Other Remaining" });
                appStatusList.Add(new AppStatusObj { StatusId = "Rejected", StatusDisplayName = "Rejected" });
                appStatusList.Add(new AppStatusObj { StatusId = "Scheduled", StatusDisplayName = "Scheduled Appointment" });
                AppStatusList.ItemsSource = appStatusList;
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }

        }
        public async void GetAppointmentDetailsOnline()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                if (AppointmentsHomePage.ApptDate != null)
                {
                    CultureInfo englishUSCulture = new CultureInfo("en-US");
                    CultureInfo.DefaultThreadCurrentCulture = englishUSCulture;
                    AppDate.Date = DateTime.Parse(AppointmentsHomePage.ApptDate.Substring(0, 10));
                }
                DateTime d = DateTime.Parse(AppointmentsHomePage.ApptDate);
                AppTime.Time = new TimeSpan(d.Hour, d.Minute, d.Second);
                Duration.Text = AppointmentsHomePage.ApptDuration;
                int s = GetAppointmentStatus(AppointmentsHomePage.ApptStatus);
                AppStatusList.SelectedIndex = s;
                Description.Text = AppointmentsHomePage.ApptDescription;
                PartnerName.Text = LoginStorage.PartnerId;
                AppointmentType.SelectedIndex = GetAppointmentType(AppointmentsHomePage.ApptType);
                ServType.Text = "Rental Contract";
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();

                    string GetContractsDetailsByContractIdURL = RestApiConstants.BaseUrl+ "/IDORequestService/ido/load/ERS_GetContractsDetailsByContractId?clm=ERS_GetContractsDetailsByContractIdSP&clmparam=" + objContractId;
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
                            RefNum.Text = data.Contract;
                            objCustName = data.Name;
                            CustomerName.Text = data.Cust_num;
                            AddressLabel.Text = data.Addr1 + " " + data.City + " " + data.State + " " + data.Zip + " " + data.Country;
                            objStreet = data.Addr1;
                            objCity = data.City;
                            objCountry = data.Country;
                            PhoneEntry.Text = data.Phone;
                            EmailAddress.Text = data.Email;
                            objContractStatus = data.Cont_stat;

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
                            }
                            else
                            {
                                con.DeleteAll<ContractDetails>();
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
                            }

                        }
                    }
                }
                else
                {
                    var isContractExists = con.Table<ContractDetails>().Where(a => a.Contract == objContractId).Count();
                    var appts = con.Table<ContractDetails>().ToList();
                    if (isContractExists > 0)
                    {
                        foreach (var appt in appts)
                        {

                            RefNum.Text = appt.Contract;
                            objCustName = appt.Name;
                            CustomerName.Text = appt.Cust_num;
                            AddressLabel.Text = appt.Addr1 + " " + appt.City + " " + appt.State + " " + appt.Zip + " " + appt.Country;
                            objStreet = appt.Addr1;
                            objCity = appt.City;
                            objCountry = appt.Country;
                            PhoneEntry.Text = appt.Phone;
                            EmailAddress.Text = appt.Email;
                            AppointmentType.SelectedIndex = GetAppointmentType(AppointmentsHomePage.ApptType);
                            objContractStatus = appt.cont_stat;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        public async void GetAppointmentTypeList()
        {
            try
            {
                List<AppStatusTypeObj> appTypeList = new List<AppStatusTypeObj>();
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "40 Wide", DisplayName = "40 Wide Install" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "A1-PreDel", DisplayName = "Pre-Delivery Procedure" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "A2-Delivery", DisplayName = "Delivery" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "A3-Collect", DisplayName = "Collection" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "A4-Return", DisplayName = "Return Procedure" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "B Install", DisplayName = "Barn Install" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "C Install", DisplayName = "Carport Install" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Collection", DisplayName = "Collection" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Emergency", DisplayName = "Emergency Repair" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "INS", DisplayName = "Install" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Inspect", DisplayName = "Inspection" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "IntTrain", DisplayName = "Internal Training" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "S Install", DisplayName = "Standard Install" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Sales", DisplayName = "Sales Call" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Service", DisplayName = "Service" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Standard", DisplayName = "Standard" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "SVC", DisplayName = "Standard Scheduled Appointment" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Training", DisplayName = "Training" });
                appTypeList.Add(new AppStatusTypeObj { DisplayId = "Vacation", DisplayName = "Vacation" });
                AppointmentType.ItemsSource = appTypeList;
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }

        private async void LocationClicked(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var result = (await Geocoding.GetLocationsAsync($"{objStreet}, {objCity}, {objCountry}")).FirstOrDefault();
                    Location location = new Location(result.Latitude, result.Longitude);
                    await Xamarin.Essentials.Map.OpenAsync(location);
                }
                else
                {
                    bool chooseResponse = await DisplayAlert("", "This app collects location data to enable current location to navigate even when the app is closed or not in use.", "Yes", "No");
                    if (chooseResponse == true)
                    {
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                        if (status == PermissionStatus.Granted)
                        {
                            var result = (await Geocoding.GetLocationsAsync($"{objStreet}, {objCity}, {objCountry}")).FirstOrDefault();
                            Location location = new Location(result.Latitude, result.Longitude);
                            await Xamarin.Essentials.Map.OpenAsync(location);
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please connect to internet", "ok");
            }
        }

        public int GetAppointmentType(string objAppType)
        {
            int status = 0;
            try
            {

                switch (objAppType)
                {
                    case "40 Wide Install":
                        status = 0;
                        break;
                    case "A1-PreDel":
                        status = 1;
                        break;
                    case "A2-Deliver":
                        status = 2;
                        break;
                    case "A3-Collect":
                        status = 3;
                        break;
                    case "A4-Return":
                        status = 4;
                        break;
                    case "B Install":
                        status = 5;
                        break;
                    case "C Install":
                        status = 6;
                        break;
                    case "Collection":
                        status = 3;
                        break;
                    case "Emergency":
                        status = 7;
                        break;
                    case "INS":
                        status = 8;
                        break;
                    case "Inspect":
                        status = 9;
                        break;
                    case "IntTrain":
                        status = 10;
                        break;
                    case "S Install":
                        status = 11;
                        break;
                    case "Sales":
                        status = 12;
                        break;
                    case "Service":
                        status = 13;
                        break;
                    case "Standard":
                        status = 14;
                        break;
                    case "SVC":
                        status = 15;
                        break;
                    case "Training":
                        status = 16;
                        break;
                    case "Vacation":
                        status = 17;
                        break;
                    default:
                        status = 0;
                        break;
                }
                return status;
            }

            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
                return 0;
            }
        }

        public int GetAppointmentStatus(string objAppStatus)
        {
            int status = 0;
            try
            {

                switch (objAppStatus)
                {
                    case "15 Remaining":
                        status = 0;
                        break;
                    case "30 Remaining":
                        status = 1;
                        break;
                    case "45 Remaining":
                        status = 2;
                        break;
                    case "60 Remaining":
                        status = 3;
                        break;
                    case "75 Remaining":
                        status = 4;
                        break;
                    case "90 Remaining":
                        status = 5;
                        break;
                    case "Accepted":
                        status = 6;
                        break;
                    case "Arrived":
                        status = 7;
                        break;
                    case "Assigned":
                        status = 8;
                        break;
                    case "Cancelled":
                        status = 9;
                        break;
                    case "Clocked Off":
                        status = 10;
                        break;
                    case "Clocked On":
                        status = 11;
                        break;
                    case "Collected":
                        status = 12;
                        break;
                    case "Completed":
                        status = 13;
                        break;
                    case "Confirmed Appointment":
                        status = 14;
                        break;
                    case "Delayed Appointment":
                        status = 15;
                        break;
                    case "In Route":
                        status = 16;
                        break;
                    case "In Transit":
                        status = 17;
                        break;
                    case "InProcess":
                        status = 18;
                        break;
                    case "New Appointment":
                        status = 19;
                        break;
                    case "New Job":
                        status = 20;
                        break;
                    case "Not Collected":
                        status = 21;
                        break;
                    case "Not Started":
                        status = 22;
                        break;
                    case "Onsite Service":
                        status = 23;
                        break;
                    case "Other Remaining":
                        status = 24;
                        break;
                    case "Rejected":
                        status = 25;
                        break;
                    case "Scheduled Appointment":
                        status = 26;
                        break;
                    default:
                        status = 0;
                        break;
                }
                return status;
            }

            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
                return 0;
            }
        }
        public async void CallClicked(object sender, EventArgs er)
        {
            if (!string.IsNullOrEmpty(PhoneEntry.Text))
            {
                await Call(PhoneEntry.Text);
            }
            else
            {
                await Call("");
            }
        }
        public async Task Call(string number)
        {
            try
            {
                PhoneDialer.Open(number);
            }

            catch (FeatureNotSupportedException ex)
            {
                await DisplayAlert("Warning", "Feature not supported", "Cancel");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", "Enter valid mobile number", "Cancel");
            }
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
                        await DisplayAlert("", "You are Currently Clocked On To: DS00000002" + "\n" + "Operation must exist where Status is Open.", "OK");
                        break;

                    case "Clear Clock On":
                        ClockedOnText.IsVisible = false;
                        ClockedOnTextLabel.IsVisible = false;
                        clockId.Source = "clockiconorange.jpg";
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
            try
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
                        Name = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "_QuadRent_Infor.jpg"
                    }); ;

                if (file == null)
                    return;
                image.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                //file.Dispose();
                return stream;
                });
                if (file == null)
                {
                    await DisplayAlert("Warning", "Could not load the image, try again!", "Cancel");
                    return;
                }
                else
                {
                    Attachments insertAttDetails = new Attachments();
                    var split = file.Path.Split('/');
                    var ImageName = split.Last();
                    string ImageTitle = objContractId + "/images/" + ImageName;
                    insertAttDetails.AttachmentTitle = ImageTitle;
                    insertAttDetails.ContractId = objContractId;
                    insertAttDetails.DataInBytes = File.ReadAllBytes(file.Path);
                    insertAttDetails.DataType = file.GetType().ToString();
                    con.Insert(insertAttDetails);
                    List<Attachments> loacalDbData = con.Table<Attachments>().Where(a => a.ContractId == objContractId).ToList();
                    foreach (var item in loacalDbData)
                    {
                        LocalDBData.Add(new AttachmentsViewModel { Name = item.AttachmentTitle });
                    }
                }
            }
            catch (Exception ex)
            {
               await DisplayAlert("Please check you internet connection.", "", "Ok");
            }
        }
        public string PhotoPath { get; set; } = null;
        private async void GalleryClicked(object sender, EventArgs e)
        {
            try
            {
                var file = await MediaPicker.PickPhotoAsync();
                await LoadPhotoAsync(file);
                if (file == null)
                    return;
                async Task LoadPhotoAsync(FileResult photo)
                {
                    // canceled
                    if (photo == null)
                    {
                        PhotoPath = null;
                        return;
                    }
                    // save the file into local storage
                    var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using (var stream = await photo.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                        await stream.CopyToAsync(newStream);
                    PhotoPath = newFile;
                    Attachments insertAttDetailsa = new Attachments();
                    var split = newFile.Split('/');

                    string ImageName = split.Last();
                    string ImageTitle = objContractId + "/images/" + ImageName;
                    insertAttDetailsa.AttachmentTitle = ImageTitle;
                    insertAttDetailsa.DataInBytes = File.ReadAllBytes(PhotoPath);
                    insertAttDetailsa.DataType = ImageName.Split('.').Last();
                    //con.Insert(insertAttDetailsa);
                    Attachments insertAttDetails = new Attachments();
                    insertAttDetails.ContractId = objContractId;
                    // myImg.Source = PhotoPath;//Show on the front page
                    List<string> selectedImages = new List<string>();
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        DependencyService.Get<IMediaService>().OpenGallery();

                        MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid");
                        MessagingCenter.Subscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid", (s, images) =>
                        {
                            selectedImages = images;
                            if (selectedImages.Count > 0)
                            {
                                foreach (var image in selectedImages)
                                {
                                    var splits = image.Split('/');

                                    string ImageNames = split.Last();
                                    string ImageTitles = objContractId + "/images/" + ImageNames;
                                    insertAttDetails.AttachmentTitle = ImageTitles;
                                    insertAttDetails.DataInBytes = File.ReadAllBytes(image);
                                    insertAttDetails.DataType = ImageNames.Split('.').Last();
                                    con.Insert(insertAttDetails);

                                }
                            }
                        });
                    }







                    //Attachments insertAttDetails = new Attachments();
                    //insertAttDetails.ContractId = objContractId;
                    //var mediaLibrary = await CrossPermissions.Current.CheckPermissionStatusAsync<MediaLibraryPermission>();
                    //var photosStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<PhotosPermission>();
                    //var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                    //List<string> selectedImages = new List<string>();
                    //if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted || photosStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    //{
                    //    bool shouldRequetPermission = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                    //    if (shouldRequetPermission)
                    //    {
                    //        await DisplayAlert("Access Required for Photos", "Photos Required", "OK");
                    //    }

                    //    status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                    //}

                    //if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    //{

                    //    //If we are on Android, call IMediaService.
                    //    if (Device.RuntimePlatform == Device.Android)
                    //    {
                    //        DependencyService.Get<IMediaService>().OpenGallery();

                    //        MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid");
                    //        MessagingCenter.Subscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid", (s, images) =>
                    //        {
                    //            selectedImages = images;
                    //            if (selectedImages.Count > 0)
                    //            {
                    //                foreach (var image in selectedImages)
                    //                {
                    //                    var split = image.Split('/');

                    //                    string ImageName = split.Last();
                    //                    string ImageTitle = objContractId + "/images/" + ImageName;
                    //                    insertAttDetails.AttachmentTitle = ImageTitle;
                    //                    insertAttDetails.DataInBytes = File.ReadAllBytes(image);
                    //                    insertAttDetails.DataType = ImageName.Split('.').Last();
                    //                    con.Insert(insertAttDetails);

                    //                }
                    //            }
                    //        });

                    //    }
                    //}
                    //else
                    //{
                    //    await DisplayAlert("Permission Denied!", "\nPlease go to your app settings and enable permissions.", "Ok");
                    //}
                }
            }
            catch (Exception ex)
            {

            }
            
        }
        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid");
        //    MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectediOS");
        //    GC.Collect();
        //}
        //private async void GalleryClicked(object sender, EventArgs e)
        //{
        //    if (!CrossMedia.Current.IsPickPhotoSupported)
        //    {
        //        await DisplayAlert("Warning", "Cannot select image", "Cancel");
        //        return;
        //    }
        //    var pickedphoto = new Plugin.Media.Abstractions.PickMediaOptions()
        //    {
        //        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
        //    };
        //    var selectedimage = await CrossMedia.Current.PickPhotoAsync(pickedphoto);
        //    if (selectedimage == null)
        //    {
        //        await DisplayAlert("Warning", "Could not load the image, try again!", "Cancel");
        //        return;
        //    }
        //    else
        //    {
        //        Attachments insertAttDetails = new Attachments();
        //        var split = selectedimage.Path.Split('/');

        //        var ImageName = split.Last();
        //        string ImageTitle = objContractId + "/images/" + ImageName;
        //        insertAttDetails.AttachmentTitle = ImageTitle;
        //        insertAttDetails.ContractId = objContractId;
        //        insertAttDetails.DataInBytes = File.ReadAllBytes(selectedimage.Path);
        //        insertAttDetails.DataType = selectedimage.GetType().ToString();
        //        con.Insert(insertAttDetails);
        //        List<Attachments> loacalDbData = con.Table<Attachments>().Where(a => a.ContractId == objContractId).ToList();
        //        foreach (var item in loacalDbData)
        //        {
        //            LocalDBData.Add(new AttachmentsViewModel { Name = item.AttachmentTitle });
        //        }

        //    }
        //}
        public string GetAppType(string appType)
        {
            string AppointmentType = "";
            switch (appType)
            {
                case "15Remain":
                    AppointmentType = "15 Remaining";
                    break;
                case "30Remain":
                    AppointmentType = "30 Remaining";
                    break;
                case "45Remain":
                    AppointmentType = "45 Remaining";
                    break;
                case "60Remain":
                    AppointmentType = "60 Remaining";
                    break;
                case "75Remain":
                    AppointmentType = "75 Remaining";
                    break;
                case "90Remain":
                    AppointmentType = "90 Remaining";
                    break;
                case "Accepted":
                    AppointmentType = "Accepted";
                    break;
                case "Arrived":
                    AppointmentType = "Arrived";
                    break;
                case "Assigned":
                    AppointmentType = "Assigned";
                    break;
                case "Cancelled":
                    AppointmentType = "Cancelled";
                    break;
                case "ClockedOff":
                    AppointmentType = "Clocked Off";
                    break;
                case "ClockedOn":
                    AppointmentType = "Clocked On";
                    break;
                case "Collected":
                    AppointmentType = "Collected";
                    break;
                case "Completed":
                    AppointmentType = "Completed";
                    break;
                case "Confirmed":
                    AppointmentType = "Confirmed Appointment";
                    break;
                case "Delayed":
                    AppointmentType = "Delayed Appointment";
                    break;
                case "InRoute":
                    AppointmentType = "In Route";
                    break;
                case "InTransit":
                    AppointmentType = "In Transit";
                    break;
                case "InProcess":
                    AppointmentType = "In-Process Appointment";
                    break;
                case "New":
                    AppointmentType = "New Appointment";
                    break;
                case "NewJob":
                    AppointmentType = "New Job";
                    break;
                case "NotCollected":
                    AppointmentType = "Not Collected";
                    break;
                case "NotStarted":
                    AppointmentType = "Not Started";
                    break;
                case "Onsite":
                    AppointmentType = "Onsite Service";
                    break;
                case "OtherRemaining":
                    AppointmentType = "Other Remaining";
                    break;
                case "Rejected":
                    AppointmentType = "Rejected";
                    break;
                case "Scheduled":
                    AppointmentType = "Scheduled Appointment";
                    break;
                default:
                    AppointmentType = "15 Remaining";
                    break;
            }
            return AppointmentType;
        }

        private async void AttachmentClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AttachmentsPage(AttachmentList, objContractId));
        }

        private async void SendEmail(object sender, EventArgs e)
        {
            List<string> emailID = new List<string>();
            if (EmailAddress.Text != null)
            {
                emailID.Add(EmailAddress.Text);
            }
            try
            {
                var message = new EmailMessage
                {
                    Subject = "",
                    Body = "",
                    To = emailID
                    //Cc = ccRecipients,
                    //Bcc = bccRecipients
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                // Email is not supported on this device
            }
            catch (Exception ex)
            {
                // Some other exception occurred
            }
        }

        private async void AppointmentMenuClicked(object sender, EventArgs e)
        {
            try
            {
                var action = await DisplayActionSheet("Select Lines", "Cancel", null, "Yard/WH Delivery Lines", "Driver Delivery Lines", "Driver Return Lines", "Yard/WH Return Lines");

                if (action != null)
                {

                    UserDialogs.Instance.ShowLoading("Loading please wait...");
                    await Task.Delay(10);
                    switch (action)
                    {
                        case "Driver Delivery Lines":
                            await Navigation.PushAsync(new LinesPage(objContractId, objContractStatus));
                            break;
                        case "Driver Return Lines":
                            await Navigation.PushAsync(new CollectionLines(objContractId, objContractStatus));
                            break;
                        case "Yard/WH Delivery Lines":
                            await Navigation.PushAsync(new YardPickingLines(objContractId, objContractStatus));
                            break;
                        case "Yard/WH Return Lines":
                            await Navigation.PushAsync(new YardReturnLine(objContractId, objContractStatus));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void RentalContractDetails(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                await Navigation.PushAsync(new RentalContractPage(Details));
            
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void SaveApptClicked(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            await Task.Delay(10);
            AppStatusTypeObj _selectedAppStatustypeObj = (AppStatusTypeObj)AppointmentType.SelectedItem;
            var SelectedAppStatusTypeObj = _selectedAppStatustypeObj.DisplayId;
            AppStatusObj _selctedappstatusobj = (AppStatusObj)AppStatusList.SelectedItem;
            var SelectedAppStatusObj = _selctedappstatusobj.StatusId;
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    string Cswitch = "0";
                    if (CompleteSwitch.IsToggled)
                    {
                        Cswitch = "1";
                    }
                    else
                    {
                        Cswitch = "0";
                    }
                    var uploadingUrl = RestApiConstants.BaseUrl+"/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                    ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>()
                    {
                    LoginStorage.Site,
                    LoginStorage.PartnerId,
                    "R",
                    Description.Text,
                    Duration.Text,
                    RefNum.Text,
                    AppDate.Date.ToString("yyyyMMdd") + " " + AppTime.Time.ToString(),
                    SelectedAppStatusObj.ToString(),
                    Cswitch,
                    SelectedAppStatusTypeObj.ToString(),
                    TaskSeqNumber.Text,

                    };

                    var client3 = new HttpClient();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Success");
                    var url = dynJson.SelectToken("Success");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr == "true")
                    {
                        await DisplayAlert("", "Saved Successfully!!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("", "Please enter valid data", "Ok");
                    }
                }
                else
                {
                    string Cswitch = "0";
                    if (CompleteSwitch.IsToggled)
                    {
                        Cswitch = "1";
                    }
                    else
                    {
                        Cswitch = "0";
                    }
                    PostAppointmentTemp postAppointmentTemp = new PostAppointmentTemp();
                    postAppointmentTemp.Site = LoginStorage.Site;
                    postAppointmentTemp.PartnerId = LoginStorage.PartnerId;
                    postAppointmentTemp.ContractId = objContractId;
                    postAppointmentTemp.Cswitch = Cswitch;
                    postAppointmentTemp.Description = Description.Text;
                    postAppointmentTemp.Duration = Duration.Text;
                    postAppointmentTemp.RefNum = RefNum.Text;
                    postAppointmentTemp.AppDate = AppDate.Date.ToString("yyyyMMdd") + " " + AppTime.Time.ToString();
                    postAppointmentTemp.AppStatus = AppStatusList.SelectedItem.ToString();
                    postAppointmentTemp.ApptType = AppointmentType.SelectedItem.ToString();
                    postAppointmentTemp.TaskSeqNumber = TaskSeqNumber.Text;
                    con.Insert(postAppointmentTemp);
                    await DisplayAlert("", "Saved Successfully!!", "Ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void CustomerDetails(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CustomerDetailsPage(objCust_num, objCust_seq, objContractId, objCustName));
        }

        private async void imgDocuments_Clicked(object sender, EventArgs e)
        {
            try
            {
                var action = await DisplayActionSheet("Choose WorkOrder Type", "Cancel", null, "POD Documents");

                if (action != null)
                {
                    switch (action)
                    {
                        case "POD Documents":
                            await Navigation.PushAsync(new PodDocuments(objContractId));
                            break;
                        default:
                            break;
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
            isRefreshingView = true;
            RefreshData();
            refreshView.IsRefreshing = false;
        }
        public async void GetAppointmentsList()
        {
            con.DeleteAll<AppointmentList>();
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    ObservableCollection<ScheduleDetails> scheduleDetails = new ObservableCollection<ScheduleDetails>();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string getRentalContractAppointmentsByPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_RentalSchedule?clm=RentalgetRentalContractAppointmentsByPartnerIdSp&clmparam=" + LoginStorage.Site + "," + LoginStorage.PartnerId + "," + "R";
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
                             appointmentDetails.Status = data.appt_stat;
                                appointmentDetails.Appt_type = data.appt_type;
                                appointmentDetails.Complete = data.Complete;
                                appointmentDetails.Description = data.description;
                                appointmentDetails.Hrs = data.hrs;
                                appointmentDetails.Ref_num = data.ref_num;
                                appointmentDetails.Sched_date = data.sched_date;
                                appointmentDetails.Partner_id = LoginStorage.PartnerId;
                                appointmentDetails.Start_date = data.Start_date;
                                appointmentDetails.End_date = data.End_date;
                                appointmentDetails.Name = data.name;
                                appointmentDetails.Cust_num = data.Cust_num;
                                appointmentDetails.Addr1 = data.Addr1;
                                appointmentDetails.City = data.City;
                                appointmentDetails.State = data.State;
                                appointmentDetails.Country = data.Country;
                                appointmentDetails.Zip = data.Zip;
                                appointmentDetails.Email = data.Email;
                                appointmentDetails.Phone = data.Phone;
                                appointmentDetails.Notes = data.Notes;
                                appointmentDetails.Billing_type = data.Billing_type;
                                appointmentDetails.Billing_freq = data.Billing_freq;
                                appointmentDetails.Cont_stat = data.Cont_stat;
                                appointmentDetails.ContDescription = data.ContDescription;
                                appointmentDetails.notes_subject = data.notes_subject;
                                appointmentDetails.Notes = data.Notes;
                                appointmentDetails.Cust_seq = data.cust_seq;
                                appointmentDetails.cust_Name = data.Customer_Name;
                                LoginStorage.CustomerName = data.name;
                                con.Insert(appointmentDetails);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }

    }

    public class LineItems
    {
        public string ItemName { get; set; }
        public string LineNum { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
    }

    public class ApptPostObjects
    {
        public object ApptSite { get; set; }
        public object ApptPartnerId { get; set; }
        public object ApptRefType { get; set; }
        public object ApptDescription { get; set; }
        public object ApptDuration { get; set; }
        public object ApptContractNum { get; set; }
        public object ApptDateTime { get; set; }
        public object ApptStatus { get; set; }
        public object ApptComplete { get; set; }
    }
    public class AppStatusTypeObj
    {
        public string DisplayId { get; set; }
        public string DisplayName { get; set; }
    }
    public class AppStatusObj
    {
        public string StatusId { get; set; }
        public string StatusDisplayName { get; set; }
    }
}
