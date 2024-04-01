using TestApp2.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static TestApp2.Views.CustomersResultListPage;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TestApp2.Models.SQLite;
using SQLite;
using SQLitePCL;
using TestApp2.Common.APIclasses;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomerDetailsPage : ContentPage
    {
        private SQLiteConnection con;
        public string objStreet { get; set; }
        public string objCity { get; set; }
        public string objCountry { get; set; }
        public string address { get; set; }
        public string objCustomerName { get; set; }

        public CustomerDetailsPage(string cusNumber,string cusSeq,string contNum,string CustomerName)
        {
            try
            {
                objCustomerName = CustomerName;
                con = DependencyService.Get<ISQLite>().GetConnection();
                InitializeComponent();
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    GetCustomerDetailsList(cusNumber, cusSeq, contNum);
                }
                else
                {
                    GetCustomerDetailsFromLocalDB(contNum);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.ToString(), "ok");
            }

        }
        public async void GetCustomerDetailsList(string cusNumber, string cusSeq, string contNum)
        {
            try
            {
                //var custNumber = cusNumber.Replace(" ", string.Empty);
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    ObservableCollection<CustomerDetails> customerDetails = new ObservableCollection<CustomerDetails>();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string getRentalContractAppointmentsByPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetAddressDetailsbyContractID?clm=ERS_GetAddressDetailsbyContractIDSP&clmparam="+ LoginStorage.Site +"," + contNum + ","+ cusNumber + "," + cusSeq;
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
                            address = data.LocationAddressDetails;
                            CustomerName.Text = objCustomerName;
                            CustomerAddress.Text = data.LocationAddressDetails;
                            OrderContactNumber.Text = data.OrderPhone;
                            OrderContactName.Text = data.OrderContact;
                            OrderContactEmail.Text= data.OrderEmail;
                            ShipToContactEmail.Text=data.ShipToEmail;
                            ShipToContactName.Text = data.ShiptoContact;
                            ShipToContactNumber.Text= data.ShiptoPhone;
                            BillingContactNumber.Text= data.BilltoPhone;
                            BillingContactEmail.Text = data.BilltoEmail;
                            BillingContactName.Text = data.BilltoContact;
                            CustomerID.Text = data.Customer;
                            ShipToID.Text = data.ShipTo;
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }
        }

        public void GetCustomerDetailsFromLocalDB(string contNum)
        {
            var isApptExists = con.Table<CustomerListData>().Where(a => a.Site_ref == LoginStorage.Site_ref && a.Cust_num == contNum).Count();
            var appts = con.Table<CustomerListData>().Where(a => a.Site_ref == LoginStorage.Site_ref && a.Cust_num == contNum).ToList();

            if (isApptExists > 0)
            {
                foreach (var data in appts)
                {
                    address = data.address;
                    CustomerName.Text = objCustomerName;
                    CustomerAddress.Text = data.address;
                    OrderContactNumber.Text = data.Cus_OrderContact;
                    OrderContactName.Text = data.Cus_OrderName;
                    OrderContactEmail.Text = data.Cus_OrderEmail;
                    ShipToContactEmail.Text = data.Cus_ShipToEmail;
                    ShipToContactName.Text = data.Cus_ShipToName;
                    ShipToContactNumber.Text = data.Cus_ShipToContact;
                    BillingContactNumber.Text = data.Cus_BillingToContact;
                    BillingContactEmail.Text = data.Cus_BillingToEmail;
                    BillingContactName.Text = data.Cus_BillingToName;
                    CustomerID.Text = data.Cus_ID;
                    ShipToID.Text = data.Cus_ShipToID;
                }
            }
        }

        private async void LocationClicked(object sender, EventArgs e)
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                    if (status == PermissionStatus.Granted)
                    {
                        var result = (await Geocoding.GetLocationsAsync($"{address}")).FirstOrDefault();
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
                                var result = (await Geocoding.GetLocationsAsync($"{address}")).FirstOrDefault();
                                Location location = new Location(result.Latitude, result.Longitude);
                                var options = new MapLaunchOptions { NavigationMode = NavigationMode.Driving };
                                await Xamarin.Essentials.Map.OpenAsync(location, options);
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Warning", "Please connect to internet", "ok");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Warning", ex.ToString(), "ok");
            }
        }

        private async void CalendarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Notes());
        }

        private async void CustomerMenuClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet(" ", "Cancel", "", "Units", "Incidents", "Rental Contracts");

            if (action != null)
            {
                switch (action)
                {
                    case "Units":
                        
                        break;
                    case "Incidents":

                        break;
                    case "Rental Contracts":
                        await Navigation.PushAsync(new AppointmentsHomePage("Rental Contracts "));
                        break;

                    default:
                        break;
                }
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
        private async void OrderContactNumberClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OrderContactNumber.Text))
            {
                await Call(OrderContactNumber.Text);
            }
            else
            {
                await Call("");
            }
        }
        private async void OrderContactEmailClicked(object sender, EventArgs e)
        {
            List<string> emailID = new List<string>();
            emailID.Add(OrderContactEmail.Text);
            try
            {
                var message = new EmailMessage
                {
                    Subject = "",
                    Body = "",
                    To=emailID                 
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
        private async void ShipToCallClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ShipToContactNumber.Text))
            {
                await Call(ShipToContactNumber.Text);
            }
            else
            {
                await Call("");
            }
        }
        private async void ShipToContactEmailClicked(object sender, EventArgs e)
        {
            List<string> emailID = new List<string>();
            emailID.Add(ShipToContactEmail.Text);
            try
            {
                var message = new EmailMessage
                {
                    Subject = "",
                    Body = "",
                    To=emailID
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
        private async void BillingContactNumberClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(BillingContactNumber.Text))
            {
                await Call(BillingContactNumber.Text);
            }
            else
            {
                await Call("");
            }
        }
        private async void BillingContactEmailClicked(object sender, EventArgs e)
        {
            List<string> emailID = new List<string>();
            emailID.Add(BillingContactEmail.Text);
            try
            {
                var message = new EmailMessage
                {
                    Subject = "",
                    Body = "",
                    To=emailID
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

    }
}
public class CustomerDetails
{
    public string CustomerName { get; set; }
    public string CustomerAddress { get; set; }
    public string OrderContact { get; set; }
    public string OrderContactName { get; set; }
    public string OrderContactNumber { get; set; }
    public string OrderContactEmail { get; set; }
    public string ShipToContact { get; set; }
    public string ShipToContactName { get; set; }
    public string ShipToContactNumber { get; set; }
    public string ShipToContactEmail { get; set; }
    public string BillingContact { get; set; }
    public string BillingContactName { get; set; }
    public string BillingContactNumber { get; set; }
    public string BillingContactEmail { get; set; }
    public string Customer { get; set; }
    public int CustomerID { get; set; }
    public string ShipTo { get; set; }
    public int ShipToID { get; set; }
    public string Site_ref { get; set; }
    public string Cust_num { get; set; }
    public string Cust_seq { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string County { get; set; }
    public string Contract { get; set; }
    //public string OrderContact { get; set; }
    public string ShiptoContact { get; set; }
    public string BilltoContact { get; set; }

    public string OrderPhone { get; set; }
    public string ShiptoPhone { get; set; }
    public string BilltoPhone { get; set; }
    //public string ShipTo { get; set; }
    public string ContDescription { get; set; }
    public string ShipToEmail { get; set; }
    public string BilltoEmail { get; set; }
    public string OrderEmail { get; set; }
    public string name { get; set; }
    //public string Customer { get; set; }
    public string LocationAddressDetails { get; set; }


}
public class custDetails
{
    public string BilltoContact { get; set; }
    public string BilltoEmail { get; set; }
    public string BilltoPhone { get; set; }
    public string ContDescription { get; set; }
    public object CreateDate { get; set; }
    public object CreatedBy { get; set; }
    public string Customer { get; set; }
    public object InWorkflow { get; set; }
    public string LocationAddressDetails { get; set; }
    public string name { get; set; }
    public object NoteExistsFlag { get; set; }
    public string OrderContact { get; set; }
    public string OrderEmail { get; set; }
    public string OrderPhone { get; set; }
    public object Picture { get; set; }
    public object RecordDate { get; set; }
    public object RowPointer { get; set; }
    public string ShipTo { get; set; }
    public string ShiptoContact { get; set; }
    public string ShipToEmail { get; set; }
    public string ShiptoPhone { get; set; }
    public object UpdatedBy { get; set; }
    public string contract { get; set; }
    public object site_ref { get; set; }
    public object _ItemId { get; set; }
    public object _ItemWarnings { get; set; }
    public object UDFShortText1 { get; set; }
    public object UDFShortText2 { get; set; }
    public object UDFShortText3 { get; set; }
    public object UDFShortText4 { get; set; }
    public object UDFShortText5 { get; set; }
    public object UDFShortText6 { get; set; }
    public object UDFShortText7 { get; set; }
    public object UDFMediumText1 { get; set; }
    public object UDFMediumText2 { get; set; }
    public object UDFMediumText3 { get; set; }
    public object UDFMediumText4 { get; set; }
    public object UDFMediumText5 { get; set; }
    public object UDFMediumText6 { get; set; }
    public object UDFLongText1 { get; set; }
    public object UDFLongText2 { get; set; }
    public object UDFDateTime1 { get; set; }
    public object UDFDateTime2 { get; set; }
    public object UDFDateTime3 { get; set; }
    public object UDFDateTime4 { get; set; }
    public object UDFDateTime5 { get; set; }
    public object UDFDateTime6 { get; set; }
    public object UDFInteger1 { get; set; }
    public object UDFInteger2 { get; set; }
    public object UDFInteger3 { get; set; }
    public object UDFInteger4 { get; set; }
    public object UDFInteger5 { get; set; }
    public object UDFInteger6 { get; set; }
    public object UDFDecimal1 { get; set; }
    public object UDFDecimal2 { get; set; }
    public object UDFDecimal3 { get; set; }
    public object UDFDecimal4 { get; set; }
    public object UDFDecimal5 { get; set; }
    public object UDFDecimal6 { get; set; }
    public object UDFRecordDate { get; set; }
    public object UDFRowPointer { get; set; }
}
