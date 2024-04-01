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
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YardReturnLine : ContentPage
    {
        public string objContractId { get; set; }
        public string objYardReturLineNumber { get; set; }
        public string objYardReturContractStat { get; set; }
        public string objYardReturSiteRef { get; set; }
        public int objLinesCount { get; set; }
        public int enable { get; set; }
        private SQLiteConnection con;

        ObservableCollection<YardReturnLinesListView> ReturnLinesList = new ObservableCollection<YardReturnLinesListView>();

        List<LineItems> YardReturnlist = new List<LineItems>();
        public YardReturnLine()
        {
            InitializeComponent();
        }


        public YardReturnLine(string objContractID, string objContractStatus)
        {
            InitializeComponent();
            YardReturnlist = new List<LineItems>();
            con = DependencyService.Get<ISQLite>().GetConnection();
            
            objContractId = objContractID;
            Contractid.Text = objContractId;
            objYardReturSiteRef = objContractStatus;

            //GetYardReturnLinesDetails();
        }
        protected override void OnAppearing()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetYardReturnLinesDetails();
            }
            else
            {
                GetYardReturnLinesFromLocalDB();
            }
        }
        public void GetYardReturnLinesFromLocalDB()
        {
            //ObservableCollection<YardReturnLinesListView> yardReturnLinesListView = new ObservableCollection<YardReturnLinesListView>();
            var isApptExists = con.Table<YardReturnLineData>().Where(a => a.Contract == objContractId).Count();
            var appts = con.Table<YardReturnLineData>().Where(a => a.Contract == objContractId).ToList();
            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                    ReturnLinesList.Add(new YardReturnLinesListView
                    {
                        YardReturnLineNumber = appt.Cont_line,
                        YardReturnSerialNumber = appt.Ser_num,
                        YardReturnItem = appt.Item,
                        YardReturnDescription = appt.Description,
                        YardReturnQuantity = appt.Qty?.Split('.').FirstOrDefault(),
                        Contractid = appt.Contract,
                        YardReturnSiteRef = appt.Site_ref,
                        YardReturnUnit = appt.Ser_num,
                        YardStatus = appt.YardStatus
                    }); ;
                }
            }
            YardReturnLinesListView.ItemsSource = ReturnLinesList;
        }
        private async void YardReturnLinesListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading Please wait...");
                YardReturnLinesListView objyardReturnLineData = (YardReturnLinesListView)e.Item;
                objContractId = objyardReturnLineData.Contractid;
                objYardReturLineNumber = objyardReturnLineData.YardReturnLineNumber;
                objYardReturSiteRef = objyardReturnLineData.YardReturnSiteRef;
                await Navigation.PushAsync(new NewYardReturnlines(objContractId, objYardReturLineNumber, objYardReturSiteRef));
            }
            catch (Exception ex)
            {

           
            }
            finally { UserDialogs.Instance.HideLoading(); }
        }
        public void GetYardReturnLinesDetails()
        {
            ReturnLinesList.Clear();
            try
            {
                
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //get Collection API URK
                    string collectionBasicInfoURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardReturnlinesByContract?clm=ERS_GetYardReturnlinesByContractSP&clmparam=" + objContractId;
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
                        //ObservableCollection<YardReturnLinesListView> yardReturnLinesListView = new ObservableCollection<YardReturnLinesListView>();
                        con.DeleteAll<YardReturnLineData>();
                        JArray collectionArray = JArray.Parse(collectionResult);
                        for (int i = 0; i < collectionArray.Count; i++)
                        {
                            JToken elem = collectionArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var yardReturnDeserialize = JsonConvert.DeserializeObject<YardReturnLineData>(jtokenString);
                            ReturnLinesList.Add(new YardReturnLinesListView { YardReturnLineNumber = yardReturnDeserialize.Cont_line, YardReturnSerialNumber = yardReturnDeserialize.Ser_num, YardReturnItem = yardReturnDeserialize.Item, YardReturnDescription = yardReturnDeserialize.Description, YardReturnQuantity = yardReturnDeserialize.Qty?.Split('.').FirstOrDefault(), Contractid = yardReturnDeserialize.Contract, YardReturnSiteRef = yardReturnDeserialize.Site_ref, YardReturnUnit = yardReturnDeserialize.Ser_num,YardStatus = yardReturnDeserialize.LineStatus });
                            //bind to listview on UI form
                            YardReturnLinesListView.ItemsSource = ReturnLinesList;
                            //List<YardReturnLineData> yardReturnLine = (from ld in con.Table<YardReturnLineData>().Where(a => a.Contract == yardReturnDeserialize.Contract && a.Cont_line == yardReturnDeserialize.Cont_line) select ld).ToList();
                            //var LinesCount = yardReturnLine.Count();

                            //if (LinesCount > 0)
                            //{
                            //    con.DeleteAll<YardReturnLineData>();
                            //    LinesCount = 0;
                            //}
                            ////bind local table 
                            //if (LinesCount == 0)
                            //{
                                YardReturnLineData yardReturnLineData = new YardReturnLineData();
                                yardReturnLineData.Cont_line = yardReturnDeserialize.Cont_line;
                                yardReturnLineData.Ser_num = yardReturnDeserialize.Ser_num;
                                yardReturnLineData.Item = yardReturnDeserialize.Item;
                                yardReturnLineData.Description = yardReturnDeserialize.Description;
                                yardReturnLineData.Qty = yardReturnDeserialize.Qty;
                                yardReturnLineData.Contract = yardReturnDeserialize.Contract;
                                yardReturnLineData.Site_ref = yardReturnDeserialize.Site_ref;
                                yardReturnLineData.YardStatus = yardReturnDeserialize.LineStatus;
                                con.Insert(yardReturnLineData);
                                GetYardReturnLinesDetails(yardReturnDeserialize.Contract, yardReturnDeserialize.Cont_line);
                            //}
                            //List<ContractLineDetails> uniqueContractLinesList = contractLinesList.Distinct().ToList();

                        }

                    }
                    else
                        DisplayAlert("Yard Return Lines Info", "No Yard Return Lines found for this Contract -" + objContractId, "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }
        }
        private void GetYardReturnLinesDetails(string objContractId, string objLineNumber)
        {
            try
            {
                //check internet status
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    LineItems items = new LineItems();
                    //intilize API request
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    //collect collection details API url
                    string collectionDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetYardReturnlinesByContract?clm=ERS_GetYardReturnlineDetailByContractSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
                    //get collectionDetails response
                    var collectionDetailsResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionDetailsURL, "GET", true, null, LoginStorage.AccessToken);
                    //convert to json object
                    var collectionDetailsJson = JObject.Parse(collectionDetailsResponse);
                    var getItems = collectionDetailsJson.SelectToken("Items");
                    //remove formatter
                    string fainalResult = getItems.ToString(Newtonsoft.Json.Formatting.None);
                    //check total items 
                    if (fainalResult != "null")
                    {
                        //convert to array for collection details 
                        JArray collectionDetailsArray = JArray.Parse(fainalResult);
                        for (int i = 0; i < collectionDetailsArray.Count; i++)
                        {
                            JToken elem = collectionDetailsArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<YardReturnLineDetails>(jtokenString);
                            YardReturnLineDetails yardReturnLineDetails = new YardReturnLineDetails();
                            var isContractLineExists = con.Table<YardReturnLineDetails>().Where(a => a.Contract == objContractId && a.Cont_line == data.Cont_line).Count();
                            if (isContractLineExists == 0)
                            {
                                yardReturnLineDetails.Contract = objContractId;
                                yardReturnLineDetails.Cont_line = data.Cont_line;
                                yardReturnLineDetails.Description = data.Description;
                                yardReturnLineDetails.Start_date = data.Start_date;
                                yardReturnLineDetails.End_date = data.End_date;
                                yardReturnLineDetails.CheckInDate = data.CheckInDate;
                                yardReturnLineDetails.CheckOutDate = data.CheckOutDate;
                                yardReturnLineDetails.Billingfreq = data.Billingfreq;
                                yardReturnLineDetails.Ser_num = data.Ser_num;
                                yardReturnLineDetails.Item = data.Item;
                                yardReturnLineDetails.Qty = data.Qty;
                                yardReturnLineDetails.U_m = data.U_m;
                                yardReturnLineDetails.Rate_conv = data.Rate_conv;
                                yardReturnLineDetails.unit_of_rate = data.unit_of_rate;
                                yardReturnLineDetails.Prorate_rate_conv = data.Prorate_rate_conv;
                                yardReturnLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
                                con.Insert(yardReturnLineDetails);
                            }
                        }
                    }
                    else
                        DisplayAlert("YardReturn Lines Details", "No YardReturn data found for this Contract -" + objContractId, "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "Ok");
            }

        }


    }



    public class YardReturnLinesListView
    {
        public string YardReturnLineNumber { get; set; }
        public string YardReturnSerialNumber { get; set; }
        public string YardReturnItemName { get; set; }
        public string YardReturnLineNum { get; set; }
        public string YardReturnItem { get; set; }
        public string YardReturnDescription { get; set; }
        public string YardReturnQuantity { get; set; }
        public string YardReturnSiteRef { get; set; }
        public string YardReturnRate { get; set; }
        public string Contractid { get; set; }
        public string YardReturnUnit { get; set; }
        public string YardStatus { get; set; }
    }
}
