using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TestApp2.Views
{
    public partial class CollectionLines : ContentPage
    {
        public string objContractId { get; set; }
        public string objCollectionLineNumber { get; set; }
        public string objCollectionContractStat { get; set; }
        public string objCollectionSiteRef { get; set; }
        public int objLinesCount { get; set; }
        public int enable { get; set; }
        private SQLiteConnection con;

        ObservableCollection<CollectionLinesListDetails> collectionLinesListDetails = new ObservableCollection<CollectionLinesListDetails>();

        List<LineItems> Collectionlist = new List<LineItems>();

        public CollectionLines()
        {
            InitializeComponent();
            
            con = DependencyService.Get<ISQLite>().GetConnection();

        }
        public CollectionLines(string objContractID, string objContractStatus)
        {
            InitializeComponent();
           

            con = DependencyService.Get<ISQLite>().GetConnection();
            objContractId = objContractID;
            Contractid.Text = objContractId;
            objCollectionContractStat = objContractStatus;
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetCollectionLines();
            }
            else
            {
                GetCollectionLinesFromLocalDB();
            }

        }
        protected override void OnAppearing()
        {
            Collectionlist = new List<LineItems>();
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                GetCollectionLines();
            }
            else
            {
                GetCollectionLinesFromLocalDB();
            }
            DeliveryLinesProofOfDelivery1.IsEnabled = false;

        }
        private async void DeliveryLinesProofOfDelivery_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new PocFormView(Collectionlist));
        }
        
        public void GetCollectionLinesFromLocalDB()
        {
            collectionLinesListDetails.Clear();

            //ObservableCollection<CollectionLinesListDetails> collectionLinesListDetails = new ObservableCollection<CollectionLinesListDetails>();
            collectionLinesListDetails.Clear();
            var isApptExists = con.Table<CollectionLineData>().Where(a => a.Contract == objContractId).Count();
            var appts = con.Table<CollectionLineData>().Where(a => a.Contract == objContractId).ToList();

            if (isApptExists > 0)
            {
                foreach (var appt in appts)
                {
                    collectionLinesListDetails.Add(new CollectionLinesListDetails { CollectionLineNumber = appt.Cont_line, CollectionSerialNumber = appt.Ser_num, CollectionItem = appt.Item, CollectionDescription = appt.Description, CollectionQuantity = appt.Qty?.Split('.').FirstOrDefault(), Contractid = appt.Contract, CollectionSiteRef = appt.Site_ref, CollectionUnit = appt.Ser_num });
                }
            }
            CollectionLinesListView.ItemsSource = collectionLinesListDetails;
        }
        private async void CollectionLinesListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading please wait...");
                CollectionLinesListDetails objContractLineDetails = (CollectionLinesListDetails)e.Item;
                objContractId = objContractLineDetails.Contractid;
                objCollectionLineNumber = objContractLineDetails.CollectionLineNumber;
                objCollectionSiteRef = objContractLineDetails.CollectionSiteRef;
                await Navigation.PushAsync(new CollectionLineDetailView(objContractId, objCollectionLineNumber, objCollectionSiteRef));
            }
            catch (Exception ex)
            {

               
            }
            finally
            {
                UserDialogs.Instance.HideLoading();

            }
        }
        #region Get Collection Details
        public void GetCollectionLines()
        {
            collectionLinesListDetails.Clear();
            try
            {
                //get network status 
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
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
                        JArray collectionArray = JArray.Parse(collectionResult);
                        con.DeleteAll<CollectionLineData>();
                        for (int i = 0; i < collectionArray.Count; i++)
                        {
                            JToken elem = collectionArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var collectionDeserialize = JsonConvert.DeserializeObject<ContractLineDetails>(jtokenString);
                            collectionLinesListDetails.Add(new CollectionLinesListDetails { CollectionLineNumber = collectionDeserialize.Cont_line, CollectionSerialNumber = collectionDeserialize.Ser_num, CollectionItem = collectionDeserialize.Item, CollectionDescription = collectionDeserialize.Description, CollectionQuantity = collectionDeserialize.Qty?.Split('.').FirstOrDefault(), Contractid = collectionDeserialize.Contract, CollectionSiteRef = collectionDeserialize.Site_ref, CollectionUnit = collectionDeserialize.Ser_num });
                            //bind to listview on UI form
                            CollectionLinesListView.ItemsSource = collectionLinesListDetails;
                            //List<CollectionLineData> collectionLinesList = (from ld in con.Table<CollectionLineData>().Where(a => a.Contract == collectionDeserialize.Contract && a.Cont_line == collectionDeserialize.Cont_line) select ld).ToList();
                            CollectionLineData collectionLines = new CollectionLineData();
                            collectionLines.Cont_line = collectionDeserialize.Cont_line;
                            collectionLines.Ser_num = collectionDeserialize.Ser_num;
                            collectionLines.Item = collectionDeserialize.Item;
                            collectionLines.Description = collectionDeserialize.Description;
                            collectionLines.Qty = collectionDeserialize.Qty;
                            collectionLines.Contract = collectionDeserialize.Contract;
                            collectionLines.Site_ref = collectionDeserialize.Site_ref;
                            con.Insert(collectionLines);
                            GetCollectionLineDetails(collectionDeserialize.Contract, collectionDeserialize.Cont_line);
                        }
                        var data2 = con.Table<CollectionLineData>().ToArray();
                    }
                    else
                        DisplayAlert("Collection Lines Info", "No Collection Lines found for this Contract -"+ objContractId, "OK");
                }                
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "ok");
            }
            var appts = con.Table<CollectionLineData>().Where(a => a.Contract == objContractId).ToList();

        }
        #endregion
        private void GetCollectionLineDetails(string CollectioncontractID,string CollectionLineNum)
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
                    string collectionDetailsURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContLinesCollectionDetailsByContractSP&clmparam=" + LoginStorage.Site + "," + CollectioncontractID + "," + CollectionLineNum;
                    //get collectionDetails response
                    var collectionDetailsResponse = mongooseAPIRequest.ProcessRestAPIRequest(collectionDetailsURL, "GET", true, null, LoginStorage.AccessToken);
                    //convert to json object
                    var collectionDetailsJson = JObject.Parse(collectionDetailsResponse);
                    var getItems = collectionDetailsJson.SelectToken("Items");
                    //remove formatter
                    string finalResult = getItems.ToString(Newtonsoft.Json.Formatting.None);
                    //check total items 
                    if (finalResult != "null")
                    {
                        //convert to array for collection details 
                        JArray collectionDetailsArray = JArray.Parse(finalResult);
                        for (int i = 0; i < collectionDetailsArray.Count; i++)
                        {
                            JToken elem = collectionDetailsArray[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<CollectionLineDetailsTable>(jtokenString);
                            //Insert locat database 
                            CollectionLineDetailsTable contractLineDetails = new CollectionLineDetailsTable();
                            var isContractLineExists = con.Table<CollectionLineDetailsTable>().Where(a => a.Contract == objContractId && a.Cont_line == data.Cont_line).Count();
                            if (isContractLineExists == 0)
                            {
                                contractLineDetails.Contract = objContractId;
                                contractLineDetails.Cont_line = data.Cont_line;
                                contractLineDetails.description = data.description;
                                contractLineDetails.start_date = data.start_date;
                                contractLineDetails.end_date = data.end_date;
                                contractLineDetails.checkInDate = data.checkInDate;
                                contractLineDetails.checkOutDate = data.checkOutDate;
                                contractLineDetails.Billing_freq = data.billingFreq;
                                contractLineDetails.ser_num = data.ser_num;
                                contractLineDetails.item = data.item;
                                contractLineDetails.qty = data.qty;
                                contractLineDetails.u_m = data.u_m;
                                contractLineDetails.lineStatus = data.lineStatus;
                                contractLineDetails.rate_conv = data.rate_conv;
                                contractLineDetails.unit_of_rate = data.UnitOfRate;
                                contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
                                contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
                                con.Insert(contractLineDetails);
                            }
                        }
                    }
                    else
                        DisplayAlert("Collection Lines Details", "No Collection data found for this Contract -" + objContractId, "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("", ex.Message, "Ok");
            }

        }

        private void LineCheck_Changed(object sender, CheckedChangedEventArgs e)
        {
            var ck = (CheckBox)sender;
            var a = ck.BindingContext as CollectionLinesListDetails;
            LineItems items = new LineItems();
            items.ItemName = a.CollectionItem;
            items.LineNum = a.CollectionLineNumber;
            items.Quantity = a.CollectionQuantity?.Split('.').FirstOrDefault();
            items.Unit = a.CollectionSerialNumber;
            if (ck.IsChecked)
            {
                Collectionlist.Add(items);
                enable++;
            }
            else if (!ck.IsChecked)
            {
                var remov = Collectionlist.Where(b => b.ItemName == items.ItemName && b.LineNum == items.LineNum && b.Quantity == items.Quantity?.Split('.').FirstOrDefault());
                Collectionlist.Remove((LineItems)remov.FirstOrDefault());
                enable--;
            }
            if (enable > 0)
            {
                DeliveryLinesProofOfDelivery1.IsEnabled = true;
            }
            else
            {
                DeliveryLinesProofOfDelivery1.IsEnabled = false;
            }
        }

        private async void DeliveryLinesProofOfDelivery1_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PocFormView(Collectionlist));
        }
    }
    public class CollectionLinesListDetails
    {
        public string CollectionLineNumber { get; set; }
        public string CollectionSerialNumber { get; set; }
        public string CollectionItemName { get; set; }
        public string CollectionLineNum { get; set; }
        public string CollectionItem { get; set; }
        public string CollectionDescription { get; set; }
        public string CollectionQuantity { get; set; }
        public string CollectionSiteRef { get; set; }
        public string CollectionRate { get; set; }
        public string Contractid { get; set; }
        public string CollectionUnit { get; set; }
    }
}
