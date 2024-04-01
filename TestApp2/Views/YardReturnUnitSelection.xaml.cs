using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YardReturnUnitSelection : ContentPage
    {
        SaveUnitDetails saveUnitDetails = new SaveUnitDetails();
        public string selectedUnitNum { get; set; }
        private SQLiteConnection con;


        public YardReturnUnitSelection(SaveUnitDetails objItem)
        {
            InitializeComponent();
            saveUnitDetails = objItem;
            con = DependencyService.Get<ISQLite>().GetConnection();
            LoadUnitNumbers(saveUnitDetails.item);
        }
        async void LoadUnitNumbers(string item)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string getCongigURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_GetSerailNumbers?clm=ERS_GetSerailNumbersSP&clmparam=" + LoginStorage.Site + "," + item;
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(getCongigURL, "GET", true, null, LoginStorage.AccessToken);
                var dynJson = JObject.Parse(properties);
                var data = JsonConvert.DeserializeObject<UnitsDropDownData>(properties);
                   List<string> UnitsList = new List<string>();
                    UnitNumbers unitsDB = new UnitNumbers();
                    foreach (var units in data.Items)
                    {
                        UnitsList.Add(units.ser_num);
                        Unit_List.Items.Add(units.ser_num);
                        unitsDB.ContractID = saveUnitDetails.ContractID;
                        unitsDB.UnitsList = string.Join(",", UnitsList);
                    }
                
            }
            else
            {
                //var unitsData = con.Table<UnitNumbers>().Where(a => a.ContractID == saveUnitDetails.ContractID).ToString();
                //if (unitsData != null)
                //{
                //    var csv = unitsData;
                //    String[] elements = csv.Split(',');
                //    foreach (var data in elements)
                //    {
                //        Unit_List.Items.Add(data);
                //    }
                //}
            }
        }
        private async void SaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var uploadingUrl = RestApiConstants.BaseUrl + "/IDORequestService/ido/invoke/ERS_GetUpdateSerailNumbers?method=ERS_GetUpdateSerailNumbersSP";
                    ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>()
                    {
                    LoginStorage.Site,
                    saveUnitDetails.ContractID,
                    saveUnitDetails.LineNum,
                    saveUnitDetails.item,
                    Unit_List.SelectedItem.ToString()
                    };

                    var client3 = new HttpClient();
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                    var dynJson = JObject.Parse(properties);
                    var url = dynJson.SelectToken("Success");
                    string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenStr == "true")
                    {
                        await DisplayAlert("", "Saved Successfully!!", "Ok");
                        //MessagingCenter.Send<YardReturnUnitSelection>(this, "Success");
                        MessagingCenter.Send<YardReturnUnitSelection, string>(this, "Hi", Unit_List.SelectedItem.ToString());
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("", "Please enter valid data", "Ok");
                    }
                }
                else
                {

                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("", "Please select unit number to save the data", "Ok");
           }    
        }

        void Unit_List_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            //selectedPickerValue.Text = Unit_List.SelectedItem.ToString();
        }
    }
}
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Item
{
    public object CreateDate { get; set; }
    public object CreatedBy { get; set; }
    public object InWorkflow { get; set; }
    public object NoteExistsFlag { get; set; }
    public object RecordDate { get; set; }
    public object RowPointer { get; set; }
    public object UpdatedBy { get; set; }
    public string ser_num { get; set; }
    public string item { get; set; }
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

public class UnitsDropDownData
{
    public List<Item> Items { get; set; }
}

