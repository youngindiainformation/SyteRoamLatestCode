using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using TestApp2.Models.SQLite;
using TestApp2.Common;
using Xamarin.Essentials;
using Plugin.Media;
using TestApp2.Common.Helper;
using System.Globalization;
using System.Collections.ObjectModel;
using TestApp2.ViewModels;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RentalContractPage : ContentPage
    {

        public static string ContractID { get; set; }
        public SQLiteConnection con;
        public string street { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string customerName { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string objCust_num { get; set; }
        public string objCust_seq { get; set; }
        public string objCustName { get; set; }

        public RentalContractPage(ScheduleDetails appointmentsListObject)
        {
            con = DependencyService.Get<ISQLite>().GetConnection();
            ContractID = appointmentsListObject.Contractid;
            InitializeComponent();
            GetRentalContractStatus();
            GetRentalContractWorkingStatus();
            BindContractDetailsToUI(appointmentsListObject);
        }
        public RentalContractPage()
        {
            InitializeComponent();
        }
        public void BindContractDetailsToUI(ScheduleDetails appointmentsListObject)
        {
            objCust_num = appointmentsListObject.Cust_num;
            objCust_seq = appointmentsListObject.custSeq;
            objCustName = appointmentsListObject.Name;
            RentalContractNumber.Text = appointmentsListObject.Contractid;
            customerName = appointmentsListObject.Name;
            street = appointmentsListObject.Addr1;
            city = appointmentsListObject.City;
            country = appointmentsListObject.Country;
            AddressLabel.Text = appointmentsListObject.Addr1 + " " + appointmentsListObject.City + " " + appointmentsListObject.State + " " + appointmentsListObject.Zip + " " + appointmentsListObject.Country;
            RentalContractDescription.Text = AppointmentsHomePage.ApptDescription;
            RentalContractPartner.Text = LoginStorage.PartnerId;
            RentalContractCustomerName.Text = customerName;
            PhoneEntry.Text = appointmentsListObject.Phone;
            RentalContractStatus.SelectedIndex = GetRentalContractStatusIndex(appointmentsListObject.Cont_stat);
            BillingFrequency.Text = GetBillingFrequency(appointmentsListObject.Billing_freq); ;
            BillingType.Text = "Calculated";
            EmailAddress.Text = appointmentsListObject.Email;
            if (appointmentsListObject.Start_date != null)
            {
                txtStartDate.Text = appointmentsListObject.Start_date?.ToString("MM/dd/yyyy hh:mm:ss tt");
            }
            else
                txtStartDate.Text = "";

            if (appointmentsListObject.End_date != null)
            {
                txtEndDate.Text = appointmentsListObject.End_date?.ToString("MM/dd/yyyy hh:mm:ss tt");
            }
            else
                txtEndDate.Text = "";
        }
        public void GetRentalContractDetails()
        {
            var currentcontract = from ld in con.Table<ContractDetails>().Where(a => a.Contract == ContractID) select ld;
            foreach (var contract in currentcontract)
            {
                var status = contract.cont_stat;
                RentalContractNumber.Text = contract.Contract;
                customerName = contract.Name;
                street = contract.Addr1;
                city = contract.City;
                country = contract.Country;
                address = street + " " + city + " " + country + " " + contract.Zip;
                RentalContractDescription.Text = contract.Description;
                RentalContractPartner.Text = LoginStorage.PartnerId;
                RentalContractCustomerName.Text = customerName;
                RentalContractDescription.Text = contract.ContractDescription.ToString();
                AddressLabel.Text = address;
                BillingFrequency.Text = GetBillingFrequency(contract.Billing_freq); ;
                BillingType.Text = "Calculated";
                if (contract.Start_date != null)
                {
                    StartDate.Text = contract.Start_date.ToString();
                }
                if (contract.End_date != null)
                {
                    EndDate.Text = contract.End_date.ToString();
                }
                RentalContractStatus.SelectedIndex = GetRentalContractStatusIndex(contract.cont_stat);
                if (contract.Contact == null)
                {
                    phone = PhoneEntry.Text;
                }
                else
                {
                    phone = contract.Phone;
                }
                if (contract.Email == null)
                {
                    email = EmailAddress.Text;
                }
                else
                {
                    email = contract.Email;
                }
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
                    status = "Weekly";
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

        public void GetRentalContractStatus()
        {
            List<string> objRentalContractStatus = new List<string>()
            {
            "Open",
            "Closed",
            "Estimate"
            };
            RentalContractStatus.ItemsSource = objRentalContractStatus;
        }

        public int GetRentalContractWorkingStatusIndex(string objRentalContractWorkingStatus)
        {
            int status = 0;
            switch (objRentalContractWorkingStatus)
            {
                case "Awaiting Parts":
                    status = 0;
                    break;
                case "Cancelled":
                    status = 1;
                    break;
                case "Closed":
                    status = 2;
                    break;
                case "Complete":
                    status = 3;
                    break;
                case "Dispatch Assignment":
                    status = 4;
                    break;
                case "Hold For Parts":
                    status = 5;
                    break;
                case "Incident On Hold":
                    status = 6;
                    break;
                case "Invoiced Complete":
                    status = 7;
                    break;
                case "Invoiced Partial":
                    status = 8;
                    break;
                case "Needs to be Quoted":
                    status = 9;
                    break;
                case "Needs to be Scheduled":
                    status = 10;
                    break;
                case "Collected":
                    status = 11;
                    break;
                case "New":
                    status = 12;
                    break;
                case "New Web Request":
                    status = 13;
                    break;
                case "Not Approved":
                    status = 14;
                    break;
                case "On Hold":
                    status = 15;
                    break;
                case "Open Request":
                    status = 16;
                    break;
                case "Order In Process":
                    status = 17;
                    break;
                case "Order Pending Approval":
                    status = 18;
                    break;
                case "Packed":
                    status = 19;
                    break;
                case "Pending Waiting for Review":
                    status = 20;
                    break;
                case "Picked":
                    status = 21;
                    break;
                case "Prospect":
                    status = 22;
                    break;
                case "Quote issued":
                    status = 23;
                    break;
                case "Service Request Order Review":
                    status = 24;
                    break;
                case "Shipped Complete":
                    status = 25;
                    break;
                case "Shipped Partial":
                    status = 26;
                    break;
                case "Suspect":
                    status = 27;
                    break;
                case "Travel":
                    status = 28;
                    break;
                case "Waiting for Approval":
                    status = 29;
                    break;
                case "Warranty Requset Authorized":
                    status = 30;
                    break;
                case "Warranty Request Reimbursed":
                    status = 31;
                    break;
                case "Warranty Request Rejected":
                    status = 32;
                    break;
                case "Working On Site":
                    status = 33;
                    break;
                default:
                    break;

            }
            return status;
        }

        public void GetRentalContractWorkingStatus()
        {
            List<string> objRentalContractWorkingStatus = new List<string>()
            {
                "Awaiting Parts",
                "Cancelled",
                "Closed",
                "Complete",
                "Dispatch Assignment",
                "Hold For Parts",
                "Incident On Hold",
                "Invoiced Complete",
                "Invoiced Partial",
                "Needs to be Quoted",
                "Needs to be Scheduled",
                "New",
                "New Web Request",
                "Not Approved",
                "On Hold",
                "Open Request",
                "Order In Process",
                "Order Pending Approval",
                "Packed",
                "Pending Waiting for Review",
                "Picked",
                "Prospect",
                "Quote issued",
                "Service Request Order Review",
                "Shipped Complete",
                "Shipped Partial",
                "Suspect",
                "Travel",
                "Waiting for Approval",
                "Warranty Requset Authorized",
                "Warranty Request Reimbursed",
                "Warranty Request Rejected",
                "Working On Site"
            };
        }

        private async void CustomerDetails(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CustomerDetailsPage(objCust_num, objCust_seq, ContractID, objCustName));
        }

        private async void CallClicked(object sender, EventArgs e)
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

        private async void SendEmail(object sender, EventArgs e)
        {
            List<string> emailID = new List<string>();
            emailID.Add(email);
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
        private async void LocationClicked(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    var result = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();
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
                            var result = (await Geocoding.GetLocationsAsync($"{street}, {city}, {country}")).FirstOrDefault();
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
        }

        private async void AttachmentClicked(object sender, EventArgs e)
        {
            ObservableCollection<AttachmentsViewModel> AttachmentList = new ObservableCollection<AttachmentsViewModel>();
            await Navigation.PushAsync(new AttachmentsPage(AttachmentList, ContractID));
        }

        private async void NotepadClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkOrderPage());
        }
    }
}