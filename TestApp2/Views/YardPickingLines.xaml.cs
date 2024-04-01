using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TestApp2.Common.SQLite;
using TestApp2.Common.APIclasses;
using TestApp2.Common;
using Acr.UserDialogs;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YardPickingLines : ContentPage
    {
       
        public string objContractId { get; set; }
        public string objYardPickingLineNumber { get; set; }
        public string objYardPickingContractStat { get; set; }
        public string objYardPickingSiteRef { get; set; }
        public int objLinesCount { get; set; }
        public int enable { get; set; }
        private SQLiteConnection con;

        ObservableCollection<YardPickingLinesListDetails> ListData = new ObservableCollection<YardPickingLinesListDetails>();
        //List<CollectionLinesListDetails> list = new List<CollectionLinesListDetails>();
        List<LineItems> YardPickinglist = new List<LineItems>();
        public YardPickingLines()
        {
            InitializeComponent();
        }
        public YardPickingLines(string objContractID, string objContractStatus)
        {
            InitializeComponent();
            YardPickinglist = new List<LineItems>();
            con = DependencyService.Get<ISQLite>().GetConnection();
            objContractId = objContractID;
            Contractid.Text = objContractId;
            objYardPickingSiteRef = objContractStatus;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetYardPickingLines();
            }
            else
            {
                GetYardPickingLinesFromLocalDB();
            }

        }
        protected override void OnAppearing()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetYardPickingLines();
            }
            else
            {
                GetYardPickingLinesFromLocalDB();
            }
        }
        public void GetYardPickingLinesFromLocalDB()
        {
            var selectedStatus = "";
            MessagingCenter.Subscribe<YardPickingLineDetail, string>(this, "Hi", async (sender, arg) =>
            {
                selectedStatus = arg;
                //await DisplayAlert("Message received", "arg=" + arg, "OK");
            });
            ListData.Clear();
            var isApptExists = con.Table<YardPickingLinesData>().Where(a => a.Contract == objContractId).Count();
            var appts = con.Table<YardPickingLinesData>().Where(a => a.Contract == objContractId).ToList();

            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                    //if (appt.LineStatus != selectedStatus)
                    //{
                    //    ListData.Add(new YardPickingLinesListDetails { YardPickingLineNumber = appt.Cont_line, YardPickingSerialNumber = appt.Ser_num, YardPickingItem = appt.Item, YardPickingDescription = appt.Description, YardPickingQuantity = appt.Qty.Substring(0, 1), Contractid = appt.Contract, YardPickingSiteRef = appt.Site_ref, YardPickingUnit = appt.Ser_num, YardPickupStatus = selectedStatus });
                    //}
                    //else
                    //{
                        ListData.Add(new YardPickingLinesListDetails { YardPickingLineNumber = appt.Cont_line, YardPickingSerialNumber = appt.Ser_num, YardPickingItem = appt.Item, YardPickingDescription = appt.Description, YardPickingQuantity = !string.IsNullOrEmpty(appt.Qty)? appt.Qty: Constants.YardPickinglineDetailsQTY, Contractid = appt.Contract, YardPickingSiteRef = appt.Site_ref, YardPickingUnit = appt.Ser_num, YardPickupStatus = appt.YardPickupStatus });
                    //}
                }
            }
            YardPickingLinesListView.ItemsSource = ListData;
        }
        private async void YardPickingLinesListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                YardPickingLinesListDetails objContractLineDetails = (YardPickingLinesListDetails)e.Item;
                objContractId = objContractLineDetails.Contractid;
                objYardPickingLineNumber = objContractLineDetails.YardPickingLineNumber;
                objYardPickingSiteRef = objContractLineDetails.YardPickingSiteRef;
                await Navigation.PushAsync(new YardPickingLineDetail(objContractId, objYardPickingLineNumber, objYardPickingSiteRef));
            }
            catch (Exception ex)
            {

            }
            finally { UserDialogs.Instance.HideLoading(); }
        }
        #region Get Yard Picking Line Details
        public void GetYardPickingLines()
        {
            //ListData = new ObservableCollection<YardPickingLinesListDetails>();
            ListData.Clear();
            try
            {
                //get network status 
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //get Collection API URK
                    string yardPickupBasicInfoURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardPickinglineDetailByContract?clm=ERS_GetYardPickinglinesByContractSP&clmparam=" + objContractId;
                    //get result data
                    var getYardPickupLinesData = mongooseAPIRequest.ProcessRestAPIRequest(yardPickupBasicInfoURL, "GET", true, null, LoginStorage.AccessToken);
                    //pars json
                    var yardPickupLinesJson = JObject.Parse(getYardPickupLinesData);
                    //select items 
                    var YardPickupItems = yardPickupLinesJson.SelectToken("Items");
                    //remove formater 
                    string YardPickupResult = YardPickupItems.ToString(Newtonsoft.Json.Formatting.None);
                    if (YardPickupResult != "null")
                    {
                        con.DeleteAll<YardPickingLinesData>();
                        //ObservableCollection<YardPickingLinesListDetails> collectionLinesListDetails = new ObservableCollection<YardPickingLinesListDetails>();
                        JArray collectionArray = JArray.Parse(YardPickupResult);
                        for (int i = 0; i < collectionArray.Count; i++)
                        {
                            JToken elem = collectionArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var yardPickingDeserialize = JsonConvert.DeserializeObject<YardPickingLinesData>(jtokenString);
                            ListData.Add(new YardPickingLinesListDetails { YardPickingLineNumber = yardPickingDeserialize.Cont_line, YardPickingSerialNumber = yardPickingDeserialize.Ser_num, YardPickingItem = yardPickingDeserialize.Item, YardPickingDescription = yardPickingDeserialize.Description, YardPickingQuantity = !string.IsNullOrEmpty(yardPickingDeserialize.Qty) ? yardPickingDeserialize.Qty : Constants.YardPickinglineDetailsQTY , Contractid = yardPickingDeserialize.Contract, YardPickingSiteRef = yardPickingDeserialize.Site_ref, YardPickingUnit = yardPickingDeserialize.Ser_num,YardPickupStatus = yardPickingDeserialize.LineStatus });
                            //bind to listview on UI form
                            YardPickingLinesListView.ItemsSource = ListData;
                            //List<YardPickingLinesData> yardpickingLinesList = (from ld in con.Table<YardPickingLinesData>().Where(a => a.Contract == yardPickingDeserialize.Contract && a.Cont_line == yardPickingDeserialize.Cont_line) select ld).ToList();
                            ////bind local table
                            //var yardpickingLinesCount = yardpickingLinesList.Count;
                            //if (yardpickingLinesCount > 0)
                            //{
                            //    con.DeleteAll<YardPickingLinesData>();
                            //    yardpickingLinesCount = 0;
                            //}
                            //if (yardpickingLinesCount == 0)
                            //{
                                YardPickingLinesData yardPickingLineData = new YardPickingLinesData();
                                yardPickingLineData.Cont_line = yardPickingDeserialize.Cont_line;
                                yardPickingLineData.Ser_num = yardPickingDeserialize.Ser_num;
                                yardPickingLineData.Item = yardPickingDeserialize.Item;
                                yardPickingLineData.Description = yardPickingDeserialize.Description;
                                yardPickingLineData.Qty = yardPickingDeserialize.Qty;
                                yardPickingLineData.Contract = yardPickingDeserialize.Contract;
                                yardPickingLineData.Site_ref = yardPickingDeserialize.Site_ref;
                                yardPickingLineData.YardPickupStatus = yardPickingDeserialize.LineStatus;
                                con.Insert(yardPickingLineData);
                                GetYardPickupLineDetails(yardPickingDeserialize.Contract, yardPickingDeserialize.Cont_line);
                            //}
                        }
                    }
                    else
                        DisplayAlert("YardPicking Lines Info", "No Collection Lines found for this Contract -" + objContractId, "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }       
        #endregion           
        }
        public void GetYardPickupLineDetails(string objContractId, string objLineNumber)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string yardPickupDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardPickinglineDetailByContract?clm=ERS_GetYardPickinglineDetailByContractSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(yardPickupDetailsURL, "GET", true, null, LoginStorage.AccessToken);
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
                            YardPickingLineDetails contractLineDetails = new YardPickingLineDetails();
                            var isContractLineExists = con.Table<YardPickingLineDetails>().Where(a => a.contract == objContractId && a.cont_line == data.cont_line).Count();
                            if (isContractLineExists > 0)
                            {
                                con.DeleteAll<YardPickingLineDetails>();
                                isContractLineExists = 0;
                            }
                            if (isContractLineExists == 0)
                            {
                                contractLineDetails.contract = objContractId;
                                contractLineDetails.cont_line = data.cont_line;
                                contractLineDetails.description = data.description;
                                contractLineDetails.start_date = DateTimeDataConverter.StringtoDateTimedata(data.start_date);
                                contractLineDetails.end_date = DateTimeDataConverter.StringtoDateTimedata(data.end_date);
                                contractLineDetails.checkInDate = DateTimeDataConverter.StringtoDateTimedata(data.checkInDate);
                                contractLineDetails.checkOutDate = DateTimeDataConverter.StringtoDateTimedata(data.checkOutDate);
                                contractLineDetails.Billingfreq = data.Billingfreq;
                                contractLineDetails.ser_num = data.ser_num;
                                contractLineDetails.item = data.item;
                                contractLineDetails.qty = data.qty;
                                Constants.YardPickinglineDetailsQTY = data.qty;
                                contractLineDetails.u_m = data.u_m;
                                contractLineDetails.rate_conv = data.rate_conv;
                                contractLineDetails.unit_of_rate = data.unit_of_rate;
                                contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
                                contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
                                contractLineDetails.lineStatus = data.lineStatus;
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
    }
    public class YardPickingLinesListDetails
    {
      
        public string YardPickingLineNumber { get; set; }
        public string YardPickingSerialNumber { get; set; }
        public string YardPickingItemName { get; set; }
        public string YardPickingLineNum { get; set; }
        public string YardPickingItem { get; set; }
        public string YardPickingDescription { get; set; }
        public string YardPickingQuantity { get; set; }
        public string YardPickingSiteRef { get; set; }
        public string YardPickingRate { get; set; }
        public string Contractid { get; set; }
        public string YardPickingUnit { get; set; }
        public string YardPickupStatus { get; set; }
    }
}

    
