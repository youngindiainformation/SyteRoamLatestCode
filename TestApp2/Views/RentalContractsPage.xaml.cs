using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RentalContractsPage : ContentPage
    {
        private SQLiteConnection con;
        public static string ApptDate { get; set; }
        public static string ApptTime { get; set; }
        public static string ApptType { get; set; }
        public static string ApptDuration { get; set; }
        public static string ApptStatus { get; set; }
        public static string ApptDescription { get; set; }
        public bool IsPullToRefreshEnabled { get; set; }
        public RentalContractsPage(string title)
        {
            try
            {
                Title = title;
                con = DependencyService.Get<ISQLite>().GetConnection();
                InitializeComponent();
                //GetAppointmentsFromLocalDB();
                //GetAppointmentsList();
                //UserDialogs.Instance.ShowLoading("Loading please wait...");
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    GetRentalAppointmentsList();
                }
                else
                {
                    GetAppointmentsFromLocalDB();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.ToString(), "ok");
            }

        }
        public RentalContractsPage()
        {

        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send(this, "SetLandscapeModeOff");
            MessagingCenter.Unsubscribe<RentalContractsPage>(this, "SetLandscapeModeOff");
        }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);

        //    if (DeviceInfo.IsOrientationPortrait() && width > height || !DeviceInfo.IsOrientationPortrait() && width < height)
        //    {
        //        if (width > height)
        //        {
        //            MessagingCenter.Send(this, "SetLandscapeModeOff");
        //        }
        //        else
        //        {
        //            MessagingCenter.Send(this, "SetLandscapeModeOn");
        //        }
        //    }
        //}
        public void GetAppointmentsFromLocalDB()
        {
            ObservableCollection<ContractScheduleDetails> contractScheduleDetails = new ObservableCollection<ContractScheduleDetails>();
            var isApptExists = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).Count();
            var appts = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).ToList();

            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                    if (appt.Hrs == null || appt.Sched_date == null)
                    {
                        contractScheduleDetails.Add(new ContractScheduleDetails
                        {
                            ApptType = appt.Appt_type,
                            Date = appt.Sched_date,
                            Duration = "",
                            CustomerName = appt.Description,
                            ScheduledTime = appt.Sched_date.Substring(11, 11),
                            Status = appt.Status,
                            ScheduledDate = appt.Sched_date.Substring(0, 10),
                            Contractid = appt.Ref_num
                        });
                    }
                    else
                    {
                        contractScheduleDetails.Add(new ContractScheduleDetails
                        {
                            ApptType = appt.Appt_type,
                            Date = appt.Sched_date,
                            Duration = appt.Hrs.Substring(0, 4),
                            CustomerName = appt.Description,
                            ScheduledTime = appt.Sched_date.Substring(11, 11),
                            Status = appt.Status,
                            ScheduledDate = appt.Sched_date.Substring(0, 10),
                            Contractid = appt.Ref_num
                        });
                    }
                }
            }
            MenuItemsListView.ItemsSource = contractScheduleDetails;
        }


        public string GetAppointmentType(string appType)
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
        public async void GetRentalAppointmentsList()
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    ObservableCollection<ContractScheduleDetails> scheduleDetails = new ObservableCollection<ContractScheduleDetails>();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string getRentalContractAppointmentsByPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetRentalContractsByPartnerId?clm=ERS_GetRentalContractsByPartnerIdSp&clmparam=" + LoginStorage.Site + "," + LoginStorage.PartnerId;
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
                            var data = JsonConvert.DeserializeObject<ContractDetails>(jtokenString);
                            ContractDetails appointmentDetails = new ContractDetails();
                            var isApptExists = con.Table<ContractDetails>().Where(a => a.Contact == data.Contact).Count();
                            if (isApptExists > 0)
                            {
                                con.DeleteAll<ContractDetails>();
                                isApptExists = 0;
                            }
                            if (isApptExists == 0)
                            {
                                appointmentDetails.Appt_stat = data.Appt_stat;
                                appointmentDetails.Appt_type = data.appointment_type;
                                appointmentDetails.Status = data.Status;
                                appointmentDetails.Complete = data.Complete;
                                appointmentDetails.CustomerName = data.CustomerName;
                                appointmentDetails.Description = data.Description;
                                appointmentDetails.Hrs = data.Hrs;
                                appointmentDetails.Ref_num = data.Contact;
                                appointmentDetails.Contact = data.Contact;
                                appointmentDetails.Sched_date = data.Sched_date;
                                appointmentDetails.Partner_id = LoginStorage.PartnerId;
                                appointmentDetails.Start_date = data.Start_date;
                                appointmentDetails.End_date = data.End_date;
                                appointmentDetails.Name = data.Name;
                                appointmentDetails.Cust_num = data.Cust_num;
                                appointmentDetails.Addr1 = data.Addr1;
                                appointmentDetails.City = data.City;
                                appointmentDetails.State = data.State;
                                appointmentDetails.Country = data.Country;
                                appointmentDetails.Zip = data.Zip;
                                appointmentDetails.Email = data.Email;
                                appointmentDetails.Phone = data.Phone;
                                appointmentDetails.Billing_type = data.Billing_type;
                                appointmentDetails.Billing_freq = data.Billing_freq;
                                appointmentDetails.cont_stat = data.cont_stat;

                                con.Insert(appointmentDetails);
                                //GetAppointmentListDetails(data.ref_num);
                            }

                            // DateTime d = (DateTime.ParseExact(data.sched_date.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture));

                            scheduleDetails.Add(new ContractScheduleDetails 
                            { ApptType = data.appointment_type,
                                Date = data.Sched_date,
                               // Duration = data.Hrs.Substring(0, 4),
                                CustomerName = data.CustomerName,
                               // ScheduledTime = data.Sched_date.Substring(11, 11), 
                                Status = data.cont_stat,
                            //  ScheduledDate = data.Sched_date.Substring(0, 10), 
                               Contractid = data.Contract, 
                                Start_date = data.Start_date,
                                End_date = data.End_date,
                                Name = data.Name,
                                Cust_num = data.Cust_num, 
                                Addr1 = data.Addr1, City = data.City, 
                                State = data.State, Country = data.Country, 
                                Zip = data.Country, Email = data.Email, Phone = data.Phone, 
                                cont_stat = data.cont_stat, 
                                Billing_freq = data.Billing_freq,
                                ContractDescription = data.ContractDescription,
                                Billing_type = "Calculated"
                            });

                            MenuItemsListView.ItemsSource = scheduleDetails;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }
        public int GetRentalContractStatusIndex(string objRentalContractStatus)
        {
            int status = 0;
            switch (objRentalContractStatus)
            {
                case "Open":
                    status = 0;
                    break;
                case "Closed":
                    status = 1;
                    break;
                case "Estimate":
                    status = 2;
                    break;
                default:
                    break;
            }
            return status;
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
                            if (isContractExists > 0)
                            {
                                con.DeleteAll<ContractDetails>();
                                isContractExists = 0;
                            }
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
        protected async override void OnAppearing()
        {
            try
            {
                // GetContractDetails(swToggle.IsToggled);
               // GetRentalAppointmentsList();
                if (!DeviceInfo.IsOrientationPortrait())
                {
                    MessagingCenter.Send(this, "SetLandscapeModeOn");
                }
                else
                {
                    MessagingCenter.Send(this, "SetLandscapeModeOff");
                }
                base.OnAppearing();

            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }
        protected override bool OnBackButtonPressed()
        {

            return base.OnBackButtonPressed();

        }

        private async void MenuItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                //ContractScheduleDetails objContractScheduleDetails = (ContractScheduleDetails)e.Item;
                ContractScheduleDetails objScheduleDetails = (ContractScheduleDetails)e.Item;
                ApptDate = objScheduleDetails.Date;
                ApptTime = objScheduleDetails.ScheduledTime;
                ApptDuration = objScheduleDetails.Duration;
                ApptStatus = objScheduleDetails.Status;
                ApptType = objScheduleDetails.ApptType;
                ApptDescription = objScheduleDetails.ContractDescription;
                await Navigation.PushAsync(new RentalContractDetails(objScheduleDetails));
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }

        private async void swToggle_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                Switch @switch = sender as Switch;
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                // GetContractDetails(@switch.IsToggled);
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }



        private async void MenuItemsListView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                // UserDialogs.Instance.ShowLoading("Loading please wait...");
                GetRentalAppointmentsList();
                //UserDialogs.Instance.HideLoading();
                MenuItemsListView.EndRefresh();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }

        private void AppointmentClicked(object sender, EventArgs e)
        {

        }
    }
    public class ContractScheduleDetails
    {
        public string ScheduledTime { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public string ScheduledDate { get; set; }
        public string Contractid { get; set; }
        public string Date { get; set; }
        public string ApptType { get; set; }
        //Details
        public string Start_date { get; set; }
        public string End_date { get; set; }
        public string Name { get; set; }
        public string Cust_num { get; set; }
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string cont_stat { get; set; }//Working Status
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string ContractDescription { get; set; }
    }
}
