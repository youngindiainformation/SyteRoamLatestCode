using TestApp2.Models.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;

using TestApp2.Common.Helper;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewAppointmentPage : ContentPage
    {
        public string ObjUsername { get; set; }

        private SQLiteConnection con;
        
        public NewAppointmentPage(string title)
        {
            Title = title;
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            GetStatusList();
            ObjUsername = LoginStorage.Username;
            GetNewAppointmentDetails();
        }
        
        public NewAppointmentPage()
        {
            InitializeComponent();
        }
        protected async override void OnAppearing()
        {
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
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send(this, "SetLandscapeModeOff");
            MessagingCenter.Unsubscribe<NewAppointmentPage>(this, "SetLandscapeModeOff");
        }
        public void GetStatusList()
        {
            List<string> newAppStatusList = new List<string>()
            {
                "15 Remaining",
                "30 Remaining",
                "45 Remaining",
                "60 Remaining",
                "75 Remaining",
                "90 Remaining",
                "Accepted",
                "Arrived",
                "Assigned",
                "Cancelled",
                "Clocked Off",
                "Clocked On",
                "Collected",
                "Completed",
                "Confirmed Appointment",
                "Delayed Appointment",
                "In Route",
                "In Transit",
                "In-Process Appointment",
                "New Appointment",
                "New Job",
                "Not Collected",
                "Not Started",
                "Onsite Service",
                "Other Remaining",
                "Rejected",
                "Scheduled Appointment",

            };
            NewAppStatusList.ItemsSource = newAppStatusList;
        }

        public void GetNewAppointmentDetails()
        {
            NewAppDate.Text = DateTime.Now.ToShortDateString();
            NewAppTime.Text = DateTime.Now.ToLongTimeString();
            NewAppPartnerName.Text = ObjUsername;
        }

        protected override bool OnBackButtonPressed()
        {
            Backcalled();
            return true;
        }
        public async void Backcalled()
        {
            if (await DisplayAlert("Exit?", "Do you want to save?", "Yes", "No"))
            {
                //ContractDetails contractDetails = new ContractDetails();
                //contractDetails.Name = NewAppDescription.Text;
                //contractDetails.Start_date = NewAppDate.Text;
                //contractDetails.Cont_stat = NewAppStatusList.Title;
                // con.Insert(contractDetails);
                AppointmentList appointmentDetails = new AppointmentList();
                appointmentDetails.Appt_type = NewAppType.Text;
                appointmentDetails.Status = (string)NewAppStatusList.SelectedItem;
                appointmentDetails.Description = NewAppDescription.Text;
                appointmentDetails.Sched_date = NewAppDate.Text;
                appointmentDetails.Hrs = NewAppDuration.Text;
                appointmentDetails.Complete =NewCompleteSwitch.IsToggled.ToString();

                //con.Insert(appointmentDetails);
                var tr = con.Table<AppointmentList>().ToList();
                await Navigation.PopAsync();
                base.OnBackButtonPressed();
            }
            else
            {
                await Navigation.PopAsync();
                base.OnBackButtonPressed();
            }
        }
    }
}