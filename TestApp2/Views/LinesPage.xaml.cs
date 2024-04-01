using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using TestApp2.Models.SQLite;
using System.Collections.ObjectModel;
using TestApp2.Common.Helper;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestApp2.Common.APIclasses;
using Xamarin.Essentials;
using Acr.UserDialogs;


namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinesPage : ContentPage
    {
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string objContractStat { get; set; }
        public string objSiteRef { get; set; }
        public int objLinesCount { get; set; }
        public int enable { get; set; }
        ObservableCollection<ContractLinesListDetails> contractLinesListDetails = new ObservableCollection<ContractLinesListDetails>();

        private SQLiteConnection con;
        
        List<LineItems> list = new List<LineItems>();
        public LinesPage()
        {
            InitializeComponent();
        }
        public LinesPage(string objContractID, string objContractStatus)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            objContractId = objContractID;
            Contractid.Text = objContractId;
            objContractStat = objContractStatus;
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                GetDeliveryLines();
            }
            else
            {
                GetDeliveryLinesFromLocalDB();
            }
        }
        protected override void OnAppearing()
        {
            list = new List<LineItems>();
            GoToProofOfDelivery.IsEnabled = false;
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                GetDeliveryLines();
            }
            else
            {
                GetDeliveryLinesFromLocalDB();
            }
        }
        public void GetDeliveryLinesFromLocalDB()
        {
            contractLinesListDetails.Clear();
            ObservableCollection<ContractLineDetails> ContractLinesList = new ObservableCollection<ContractLineDetails>();
            var isApptExists = con.Table<ContractLineDetails>().Where(a => a.Contract == objContractId).Count();
            var appts = con.Table<ContractLineDetails>().Where(a => a.Contract == objContractId).ToList();

            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                    contractLinesListDetails.Add(new ContractLinesListDetails { LineNumber = appt.Cont_line, SerialNumber = appt.Ser_num, Item = appt.Item, Description = appt.Description, Quantity = appt.Qty?.Split('.').FirstOrDefault(), Contractid = appt.Contract, SiteRef = appt.Site_ref, Unit = appt.Ser_num });
                }
            }
            MenuItemsListView.ItemsSource = contractLinesListDetails;
        }
        public void GetDeliveryLines()
        {
            contractLinesListDetails.Clear();
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {

                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl +"/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContractLinesBasicInfoByContractIdSP&clmparam=" + objContractId;
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var res = dynJson.SelectToken("Items");
                    string jtokenStr = res.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr != "null")
                    {
                        //contractLinesListDetails = new ObservableCollection<ContractLinesListDetails>();
                        JArray array = JArray.Parse(jtokenStr);
                        con.DeleteAll<ContractLineDetails>();
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<ContractLinesData>(jtokenString);
                            contractLinesListDetails.Add(new ContractLinesListDetails { LineNumber = data.cont_line, SerialNumber = data.ser_num, Item = data.item, Description = data.description, Quantity = data.qty?.Split('.').FirstOrDefault(), Contractid = data.contract, SiteRef = data.site_ref, Unit = data.ser_num });
                            MenuItemsListView.ItemsSource = contractLinesListDetails;

                            //List<ContractLineDetails> contractLinesList = (from ld in con.Table<ContractLineDetails>().Where(a => a.Contract == data.contract && a.Cont_line == data.cont_line) select ld).ToList();
                            //var LinesCount = contractLinesList.Count();
                            //if (LinesCount > 0)
                            //{
                            //    con.DeleteAll<ContractLineDetails>();
                            //    LinesCount = 0;
                            //}
                            //if (LinesCount == 0)
                            //{
                                ContractLineDetails contractLineDetails = new ContractLineDetails();
                                contractLineDetails.Cont_line = data.cont_line;
                                contractLineDetails.Ser_num = data.ser_num;
                                contractLineDetails.Item = data.item;
                                contractLineDetails.Description = data.description;
                                contractLineDetails.Qty = data.qty;
                                contractLineDetails.Contract = data.contract;
                                contractLineDetails.Site_ref = data.site_ref;
                                con.Insert(contractLineDetails);
                                GetDeliveryLineDetails(data.contract, data.cont_line);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }

        }
        public void GetDeliveryLineDetails(string objContractId, string objLineNumber)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContractLinesDetailsByContractIdSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
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

                            var data = JsonConvert.DeserializeObject<LineData>(jtokenString);
                            ContractLineSiteData contractLineDetails = new ContractLineSiteData();
                            var isContractLineExists = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == data.cont_line).Count();
                            if(isContractLineExists >0)
                            {
                                con.DeleteAll<ContractLineSiteData>();
                                isContractLineExists = 0;
                            }
                            if (isContractLineExists == 0)
                            {
                                contractLineDetails.Contract = objContractId;
                                contractLineDetails.Cont_line = data.cont_line;
                                contractLineDetails.description = data.description;
                                contractLineDetails.start_date = data.start_date;
                                contractLineDetails.end_date = data.end_date;
                                contractLineDetails.checkInDate = data.checkInDate ;
                                contractLineDetails.checkOutDate = data.checkOutDate;
                                contractLineDetails.Billing_freq = data.Billingfreq;
                                contractLineDetails.lineStatus = data.lineStatus;
                                contractLineDetails.ser_num = data.ser_num;
                                contractLineDetails.item = data.item;
                                contractLineDetails.qty = data.qty;
                                contractLineDetails.u_m = data.u_m;
                                contractLineDetails.rate_conv = data.rate_conv;
                                contractLineDetails.unit_of_rate = data.UnitOfRate;
                                contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
                                contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
                                con.Insert(contractLineDetails);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "Ok");
            }
        }
        private async void AddLineClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewLinePage(objContractId, objLinesCount));
        }
        private async void MenuItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                ContractLinesListDetails objContractLineDetails = (ContractLinesListDetails)e.Item;
                objContractId = objContractLineDetails.Contractid;
                objLineNumber = objContractLineDetails.LineNumber;
                objSiteRef = objContractLineDetails.SiteRef;
                await Navigation.PushAsync(new LinePage(objContractId, objLineNumber, objSiteRef));
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
        private void LineCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var ck = (CheckBox)sender;
            var a = ck.BindingContext as ContractLinesListDetails;
            LineItems items = new LineItems();
            items.ItemName = a.Item;
            items.LineNum = a.LineNumber;
            items.Quantity = a.Quantity?.Split('.').FirstOrDefault(); ;
            items.Unit = a.SerialNumber;
            if (ck.IsChecked)
            {
                list.Add(items);
                enable++;
            }
            else if (!ck.IsChecked)
            {
                var remov = list.Where(b => b.ItemName == items.ItemName && b.LineNum == items.LineNum && b.Quantity == items.Quantity?.Split('.').FirstOrDefault());
                list.Remove((LineItems)remov.FirstOrDefault());
                enable--;
            }
            if (enable > 0)
            {
                GoToProofOfDelivery.IsEnabled = true;
            }
            else
            {
                GoToProofOfDelivery.IsEnabled = false;
            }
        }
        private async void GoToProofOfDeliveryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkOrderPage(list));
        }
    }
    public class ContractLinesListDetails
    {
        public string LineNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string SiteRef { get; set; }
        public string Rate { get; set; }
        public string Contractid { get; set; }
        public string Unit { get; set; }
    }
}