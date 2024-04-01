using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Models.SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using System.Collections.ObjectModel;
using TestApp2.Views;
using Acr.UserDialogs;
using TestApp2.ViewModels;
using TestApp2.Common.Helper;
using Xamarin.Essentials;
using TestApp2.Common.APIclasses;
using System.Globalization;
using NativeMedia;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppointmentsHomePage : ContentPage
    {

        private SQLiteConnection con;
        public static string ApptDate { get; set; }
        public static string ApptTime { get; set; }
        public static string ApptType { get; set; }
        public static string ApptDuration { get; set; }
        public static string ApptStatus { get; set; }
        public static string ApptDescription { get; set; }
        public bool IsPullToRefreshEnabled { get; set; }
        public AppointmentsHomePage(string title)
        {
            try
            {
                Title = title;
                con = DependencyService.Get<ISQLite>().GetConnection();
                InitializeComponent();
                //if ((Connectivity.NetworkAccess == NetworkAccess.Internet))
                //{
                //    GetAppointmentsList();
                //}
                //else
                //{
                //    GetAppointmentsFromLocalDB();
                //}
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.ToString(), "ok");
            }
        }

        protected override void OnAppearing()
        {
            if ((Connectivity.NetworkAccess == NetworkAccess.Internet))
            {
                GetAppointmentsList();
            }
            else
            {
                GetAppointmentsFromLocalDB();
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send(this, "SetLandscapeModeOff");
            MessagingCenter.Unsubscribe<AppointmentsHomePage>(this, "SetLandscapeModeOff");
        }
        
        public void GetAppointmentsFromLocalDB()
        {
            ObservableCollection<ScheduleDetails> scheduleDetails = new ObservableCollection<ScheduleDetails>();
            var isApptExists = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).Count();
            var appts = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).ToList();

            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                       scheduleDetails.Add(new ScheduleDetails
                        {
                            ApptType = appt.Appt_type,
                            Date = appt.Sched_date,
                            Duration = appt.Hrs!=null? appt.Hrs.Substring(0, 4) : "",
                            CustomerName = appt.Description,
                            ScheduledTime = appt.Sched_date != null ? appt.Sched_date.Substring(11, 11):"",
                            Status = GetAppointmentType(appt.Status),
                            ScheduledDate = appt.Sched_date != null ? appt.Sched_date.Substring(0, 10) : "",
                            Contractid = appt.Ref_num,
                            notes_subject = appt.notes_subject,
                            Notes = appt.Notes,
                            Country = appt.Country,
                            Phone = appt.Phone,
                            City = appt.City,
                            Addr1 = appt.Addr1,
                            State = appt.State,
                            Zip = appt.Zip,
                            Email = appt.Email,
                            Cust_num = appt.Cust_num,
                            Start_date = appt.Start_date,
                            End_date = appt.End_date,
                            Billing_freq = appt.Billing_freq,
                            Name = appt.Name,
                            task_seq = appt.task_seq,
                        });
                    }
            }
            MenuItemsListView.ItemsSource = scheduleDetails;
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
        public async void GetAppointmentsList()
        {
            try
            {
                var isApptExists = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).Count();
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    ObservableCollection<ScheduleDetails> scheduleDetails = new ObservableCollection<ScheduleDetails>();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string getRentalContractAppointmentsByPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_RentalSchedule?clm=RentalgetRentalContractAppointmentsByPartnerIdSp&clmparam="+LoginStorage.Site + "," + LoginStorage.PartnerId + "," + "R";
                    var rentalContractAppointments = mongooseAPIRequest.ProcessRestAPIRequest(getRentalContractAppointmentsByPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(rentalContractAppointments);
                    var url = dynJson.SelectToken("Items");

                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        JArray array = JArray.Parse(jtokenStr);
                        con.DeleteAll<AppointmentList>();
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
                                appointmentDetails.cust_Name = data.name;
                                appointmentDetails.task_seq = data.task_seq;
                                LoginStorage.CustomerName = data.name;
                                con.Insert(appointmentDetails);
                            scheduleDetails.Add(new ScheduleDetails { ApptType = data.appt_type, Date = data.sched_date, Duration = data.hrs.Substring(0, 4), CustomerName = data.description, ScheduledTime = data.sched_date.Substring(9, 11), Status = GetAppointmentType(data.appt_stat), ScheduledDate = data.sched_date.Substring(0, 10), Contractid = data.ref_num, Start_date = data.Start_date, End_date = data.End_date, Name = data.name, Cust_num = data.Cust_num, Addr1 = data.Addr1, City = data.City, State = data.State, Country = data.Country, Zip = data.Zip, Email = data.Email, Phone = data.Phone, Notes = data.Notes, Cont_stat = data.Cont_stat, Billing_freq = data.Billing_freq, Billing_type = data.Billing_type, ContDescription = data.ContDescription, notes_subject = data.notes_subject , cust_Name  = data.Customer_Name , custSeq = data.cust_seq , task_seq = data.task_seq }); ;
                            MenuItemsListView.ItemsSource = scheduleDetails;

                        }                   
                    }                   
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
            var appts = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).ToList();

        }
        //public async void GetAppointmentListDetails(string objContractId)
        //{
        //    try
        //    {
        //        UserDialogs.Instance.ShowLoading("Loading please wait...");
        //        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        //        {
        //            MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
        //            string GetContractsDetailsByContractIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractsDetailsByContractId?clm=ERS_GetContractsDetailsByContractIdSP&clmparam=" + objContractId;
        //            var RentalContractAppointmentsByContractId = mongooseAPIRequest.ProcessRestAPIRequest(GetContractsDetailsByContractIdURL, "GET", true, null, LoginStorage.AccessToken);
        //            var dynJson = JObject.Parse(RentalContractAppointmentsByContractId);
        //            var url = dynJson.SelectToken("Items");
        //            string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
        //            if (jtokenStr != "null")
        //            {
        //                JArray array = JArray.Parse(jtokenStr);
        //                for (int i = 0; i < array.Count; i++)
        //                {
        //                    JToken elem = array[i];
        //                    // token string
        //                    string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
        //                    var data = JsonConvert.DeserializeObject<ContractDetailsAPI>(jtokenString);
        //                    ContractDetails contractDetails = new ContractDetails();
        //                    var isContractExists = con.Table<ContractDetails>().Where(a => a.Contract == data.Contract).Count();
        //                    if(isContractExists >0 )
        //                    {
        //                        con.DeleteAll<ContractDetails>();
        //                        isContractExists = 0;
        //                    }
        //                    if (isContractExists == 0)
        //                    {
        //                        contractDetails.Contract = data.Contract;
        //                        contractDetails.Description = AppointmentsHomePage.ApptDescription;
        //                        contractDetails.Start_date = data.Start_date;
        //                        contractDetails.End_date = data.End_date;
        //                        contractDetails.Name = data.Name;
        //                        contractDetails.Cust_num = data.Cust_num;
        //                        contractDetails.Addr1 = data.Addr1;
        //                        contractDetails.City = data.City;
        //                        contractDetails.State = data.State;
        //                        contractDetails.Country = data.Country;
        //                        contractDetails.Zip = data.Zip;
        //                        contractDetails.Phone = data.Phone;
        //                        contractDetails.Email = data.Email;
        //                        contractDetails.Status = data.Cont_stat;
        //                        contractDetails.Billing_freq = data.BillingFreq;
        //                        contractDetails.Billing_type = data.BillingType;
        //                        contractDetails.Notes_subject = data.notes_subject;
        //                        contractDetails.appointment_type = AppointmentsHomePage.ApptType;
        //                        con.Insert(contractDetails);
        //                    }                           
        //                }
        //            }
        //        }

        //        UserDialogs.Instance.HideLoading();
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("", ex.Message, "ok");
        //    }
        //}       
        protected override bool OnBackButtonPressed()
        {

            return base.OnBackButtonPressed();

        }
        private async void AddAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                await Navigation.PushAsync(new NewAppointmentPage(Title));
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }
        private async void MenuItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            //GetAppointmentsFromLocalDB();
            UserDialogs.Instance.ShowLoading("Loading please wait...");
            try
            {
                ScheduleDetails objScheduleDetails = (ScheduleDetails)e.Item;
                ApptDate = objScheduleDetails.Date;
                ApptTime = objScheduleDetails.ScheduledTime;
                ApptDuration = objScheduleDetails.Duration;
                ApptStatus = objScheduleDetails.Status;
                ApptType = objScheduleDetails.ApptType;
                ApptDescription = objScheduleDetails.CustomerName;
                var appts = con.Table<AppointmentList>().Where(a => a.Partner_id == LoginStorage.PartnerId).ToList();

                await Navigation.PushAsync(new AppointmentDetailPage(objScheduleDetails));
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
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

        private void AppointmentClicked(object sender, EventArgs e)
        {

        }

        private async void MenuItemsListView_Refreshing(object sender, EventArgs e)
        {
            try
            {
               // UserDialogs.Instance.ShowLoading("Loading please wait...");
                GetAppointmentsList();
                //UserDialogs.Instance.HideLoading();
                MenuItemsListView.EndRefresh();
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.ToString(), "ok");
            }
        }
    }

    public class ScheduleDetails
    {
        public string ScheduledTime { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public string ScheduledDate { get; set; }
        public string Contractid { get; set; }
        public string Date { get; set; }
        public string ApptType { get; set; }
        public string notes_subject { get; set; }
        //Details
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; }
        public string Name { get; set; }
        public string Cust_num { get; set; }
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cont_stat { get; set; }//Working Status
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string Notes { get; set; }
        public string ContDescription { get; set; }
        public string cust_Name { get; set; }
        public string custSeq { get; set; }
        public string customerName { get; set; }
        public string task_seq { get; set; }
    }
}