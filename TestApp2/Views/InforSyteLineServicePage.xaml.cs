
using TestApp2.Verify;
using TestApp2.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SQLite;
using TestApp2.Models.SQLite;
using TestApp2.Common.Helper;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Net.Http;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestApp2SyteLineServicePage : FlyoutPage
    {
        readonly FlyoutLayoutBehavior flyoutLayoutBehavior;
        private SQLiteConnection con;

        public TestApp2SyteLineServicePage()
        {
            InitializeComponent();
            FlyoutPage.ListView.ItemSelected += ListView_ItemSelected;           

        }
        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            con = DependencyService.Get<ISQLite>().GetConnection();
            List<WorkOrderData> workOrderData = con.Table<WorkOrderData>().ToList();
            List<PostAppointmentTemp> appointmentData = con.Table<PostAppointmentTemp>().ToList();
            var item = e.SelectedItem as TestApp2SyteLineServicePageFlyoutMenuItem;
            if (item == null)
                return;
            switch (item.Title)
            {
                case "Appointments":
                    Navigation.PushAsync(new AppointmentsHomePage("Appointment"));
                    break;
                case "Contracts":
                    Navigation.PushAsync(new RentalContractsPage("Rental Contracts Page"));
                    break;
                case "Partners":
                    break;
                case "Customers":
                    Navigation.PushAsync(new CustomersSearchPage());
                    break;
                case "Units":
                    break;
                case "Item Availability":
                    break;
                case "Sync Conflicts":
                    syncLinesData(workOrderData);
                    //syncAppointmentData(appointmentData);
                    break;
                case "Change Site":
                    ChangeSiteClicked();
                    break;
                case "Settings":
                    Navigation.PushAsync(new SettingsPage());
                    break;
                case "Sign Out":
                    //con.DeleteAll<AppointmentList>();
                    //con.DeleteAll<ContractDetails>();
                    //con.DeleteAll<ContractLineDetails>();
                    //con.DeleteAll<ContractLineSiteData>();
                    //con.DeleteAll<CollectionLineData>();
                    Navigation.PushAsync(new TestApp2.Verify.Login());
                    break;
            }
            //var page = (Page)Activator.CreateInstance(item.TargetType);
            //page.Title = item.Title;
            //page.IconImageSource = item.Flyoutouticon;
            //Detail = new NavigationPage(page);
            //IsPresented = false;

            FlyoutPage.ListView.SelectedItem = null;

        }
        public async void ChangeSiteClicked()
        {
            await  Navigation.PushAsync(new ChangeSitePage());
        }
        MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
        string ItemId = null;
        string postResponse = "false";
        string jtokenStr = "false";
        public async void syncLinesData(List<WorkOrderData> workOrderdata)
        {
            
            var workOrderInfo = workOrderdata.ToArray();
            ContractLineDetails ContractData = new ContractLineDetails();
            string conId = "";
            if (workOrderInfo.Length > 0)
            {
                string linedata = "";
               
                foreach (var item in workOrderInfo)
                {
                   
                    List<string> arr = new List<string>();
                    string[] arr2 = item.LineNumbers.Split(',').ToArray();

                    foreach (char c in item.LineNumbers)
                    {
                        if (!(c.Equals(',')))
                        {
                            arr.Add(c.ToString());
                        }
                    }
                    string[] arr1 = arr.ToArray();
                    foreach (var lineNum in arr1)
                    {

                        string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl +"/IDORequestService/ido/load/ERS_RentalContLines?Properties=contract_1&filter=contract_1= '" + item.ContractID + "' and cont_line=" + lineNum;
                        var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                        var dynJson = JObject.Parse(properties);
                        var res = dynJson.SelectToken("Items");
                        string jtokenStr = res.ToString(Newtonsoft.Json.Formatting.None);
                        if (jtokenStr != "null")
                        {
                            ObservableCollection<GetItemId> getItemId = new ObservableCollection<GetItemId>();
                            JArray array = JArray.Parse(jtokenStr);
                            for (int j = 0; j < array.Count; j++)
                            {
                                JToken elem = array[j];
                                // token string
                                string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                                var data = JsonConvert.DeserializeObject<GetItemId>(jtokenString);
                                ItemId = data._ItemId;
                            }
                        }

                        var postDataURl = RestApiConstants.BaseUrl +"/IDORequestService/ido/update/ERS_RentalContLines?refresh=true";
                        var accessToken = LoginStorage.AccessToken;
                        string jsonData = "{\"Changes\":[{\"Action\":2,\"ItemId\":\"PBT=[fs_cont_line]cntln.ID=[cdf7a50b-e4d8-490a-a7ac-6e95ab09975c]cntln.DT=[2022-02-2320:37:40.670]ERS_cntln.DT=[2022-02-2311:37:23.030]ERS_cntln.ID=[9a1fe2a3-d494-4fe0-8edb-c71dc470f4ad]\",\"Properties\":[{\"Name\":\"CustDelSignature\",\"Value\":\"\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"DelDate\",\"Value\":\"2021-11-2216:03:25.670\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"LineStatus\",\"Value\":\"Delivered\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"CustDelName\",\"Value\":\"Sumandatetest\",\"OriginalValue\":null,\"Modified\":true,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"contract_1\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false},{\"Name\":\"cont_line\",\"Value\":2,\"OriginalValue\":null,\"Modified\":false,\"IsNull\":false,\"IsNestedCollection\":false}],\"UpdateLocking\":1}]}";
                        var approveData = JsonConvert.DeserializeObject<AprooveData>(jsonData);
                        approveData.Changes[0].ItemId = ItemId;
                        approveData.Changes[0].Properties[0].Value = item.Signature;
                        approveData.Changes[0].Properties[1].Value = item.SubmitDate;
                        approveData.Changes[0].Properties[3].Value = item.Name;

                        jsonData = JsonConvert.SerializeObject(approveData);
                        var properties1 = mongooseAPIRequest.ProcessRestAPIRequest(postDataURl, "POST", true, jsonData, LoginStorage.AccessToken);
                        var dynJson1 = JObject.Parse(properties);
                        var url1 = dynJson1.SelectToken("Success");
                        postResponse = url1.ToString(Newtonsoft.Json.Formatting.None);
                        //if(postResponse == "true")
                        //{
                        //    if(ContractData.Contract == item.ContractID)
                        //    {
                        //        ContractData.LineStatus = "Delivered";
                        //    }
                        //    con.Update(ContractData);                          
                        //}
                    }

                }
                if (postResponse == "true")
                {
                    await DisplayAlert("", "Saved Successfully!!", "Ok");

                    //con.DeleteAll<WorkOrderData>();

                }
                else
                {
                    await DisplayAlert("", "Please enter valid data", "Ok");
                }
            }
            else
            {
                await DisplayAlert("", "Your data is already updated", "Ok");
            }
        }
        public  async void syncAppointmentData(List<PostAppointmentTemp> postappdata)
        {
            var appointdata = postappdata.ToArray();
            foreach (var item in appointdata)
            {
                var uploadingUrl = RestApiConstants.BaseUrl +"/IDORequestService/ido/invoke/ERS_RentalAppointmentSchedule?method=ERS_UpdateRentalContractAppointmentsByPartnerIdSp";
                ObservableCollection<string> objApptPostObjects = new ObservableCollection<string>() {item.Site,
                item.PartnerId,
                item.PartnerId,
                 "R",
                 item.Description,
                 item.Duration,
                 item.RefNum,
                 item.AppDate,
                 item.AppStatus,
                 item.Cswitch,
                 item.ApptType, };
                 //Description.Text,
                //Duration.Text,
                 //RefNum.Text,
                //AppDate.Date.ToString("yyyyMMdd") + " " + AppTime.Time.ToString(),
               // AppStatusList.SelectedItem.ToString(),
               //Cswitch,AppointmentType.SelectedItem.ToString() };

                var client3 = new HttpClient();
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();

                string jsonData = JsonConvert.SerializeObject(objApptPostObjects);
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(uploadingUrl, "POST", true, jsonData, LoginStorage.AccessToken);
                var dynJson = JObject.Parse(properties);
                var res = dynJson.SelectToken("Success");
                var url = dynJson.SelectToken("Success");
                string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                
            }
            if (jtokenStr == "true")
            {
                await DisplayAlert("", "Saved Successfully!!", "Ok");
            }
            else
            {
                await DisplayAlert("", "Please enter valid data", "Ok");
            }
        }


    }
}