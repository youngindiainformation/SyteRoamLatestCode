using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Essentials;
using TestApp2.Common.Helper;
using SQLite;
using TestApp2.Models.SQLite;
using Acr.UserDialogs;
using Xamarin.Forms.Xaml;
using TestApp2.Common.APIclasses;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using TestApp2.Common.SQLite;
using System.Text;
using Plugin.Permissions;
using ZXing.Net.Mobile.Forms;
using DeviceOrientation.Forms.Plugin.Abstractions;

namespace TestApp2.Verify
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        private SQLiteConnection con;

        public string objUserID { get; set; }
        public string objContractId { get; set; }
        public string objLineNumber { get; set; }
        public string JsonDataFromDocument { get; set; }
        public string BaseUrl = "";
        public int _logincount = 0;

        public Login()
        {
            InitializeComponent();
            //------- Local Instance Code --------\\
            con = DependencyService.Get<ISQLite>().GetConnection();

            //UsernameInput.Text = "garry";
            //PasswordInput.Text = "123";
            
        }
        protected override void OnAppearing()
        {
            Configurations configData = con.Table<Configurations>().FirstOrDefault();
            if (configData == null)
            {
                UsernameInput.IsVisible = true;
                PasswordInput.IsVisible = true;

                
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    if (LoginStorage.Configurations != null)
                    {
                        var csv = LoginStorage.Configurations;
                        String[] elements = csv.Split(',');
                        foreach (var item in elements)
                        {
                            Configuration_List.Items.Add(item);
                        }
                    }
                }
                else
                {
                    if (LoginStorage.Configurations != null)
                    {
                        var csv = LoginStorage.Configurations;
                        String[] elements = csv.Split(',');
                        foreach (var item in elements)
                        {
                            Configuration_List.Items.Add(item);
                        }
                    }
                }
            }
            else
            {
                //---------Cloud Config Code -----------\\
                RestApiConstants.CongfigurationGroup = configData.configGroup;
                con = DependencyService.Get<ISQLite>().GetConnection();
                if (configData != null)
                {
                    BaseUrl = configData.webUrl + "/" + configData.configGroup + "/CSI/IDORequestService/ido/";
                }
                MessagingCenter.Subscribe<SettingsPage>(this, "dropDownData", (sender) =>
                {
                    UsernameInput.IsVisible = false;
                    PasswordInput.IsVisible = false;
                    Configurations conData = con.Table<Configurations>().FirstOrDefault();

                    if (conData != null)
                    {
                        LoginStorage.Username = conData.UserName;
                        LoginStorage.Password = conData.UserPassword;
                        Configuration_List.Items.Clear();
                        var csv = conData.config;
                        String[] elements = csv.Split(',');
                        foreach (var item in elements)
                        {
                            Configuration_List.Items.Add(item);
                        }
                    }
                });
                if (configData != null)
                {
                    UsernameInput.IsVisible = false;
                    PasswordInput.IsVisible = false;
                    Configuration_List.Items.Clear();
                    var csv = configData.config;
                    String[] elements = csv.Split(',');
                    foreach (var item in elements)
                    {
                        Configuration_List.Items.Add(item);
                    }
                }

            }
        }

        public static byte[] ReadBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        private async void LogInClicked(object sender, EventArgs e)
        {
            bool response = false;
            if (!LoginStorage.IsCloudConfig)
            {
                response = await LocalAuthentication();
            }
            else
            {
                response = await CloudAuthentication();
            }

            UserDialogs.Instance.ShowLoading("Loading please wait...");
            await Task.Delay(2500);
            if (response == true)
            {
                LoginStorage.LoginCount = _logincount++;
                await Navigation.PushAsync(new TestApp2SyteLineServicePage());
            }
            else
            {
                await DisplayAlert("Invalid Credentials!", "Username or Password is incorrect", "Ok");
            }
            UserDialogs.Instance.HideLoading();
        }
        //--------Cloud config Authentication method -----------//
        private async Task<bool> CloudAuthentication()
        {
            try
            {
                bool retVal = false;
                LoginDetails loginDetails = new LoginDetails();
                LoginStorage.Site = Configuration_List.SelectedItem.ToString().Replace(RestApiConstants.CongfigurationGroup.Trim()+"_","");
                if ((Connectivity.NetworkAccess == NetworkAccess.Internet))
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    var config = Configuration_List.SelectedItem.ToString();                    
                    loginDetails.Site = Configuration_List.SelectedItem.ToString().Replace(RestApiConstants.CongfigurationGroup.Trim() + "_", "");
                    con.Insert(loginDetails);
                    GetUserDetails();
                    retVal = true;
                    //if (LoginStorage.AccessToken == null)
                    //{
                    //    retVal = true;
                    //    string loginURL = BaseUrl + "token/" + config + "/" + LoginStorage.Username + "/" + LoginStorage.Password;
                    //    LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL, "GET", false, null, null);
                    //    //string loginURL1 = "http://117.203.102.179:8085/IDORequestService/ido/token/" + config + "/sa";
                    //    //LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL1, "GET", false, null, null);
                    //    var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == UsernameInput.Text.ToLower() && a.UserPassword == PasswordInput.Text && a.Site == LoginStorage.Site).Count();
                    //    if (isLoginExists == 0)
                    //    {
                    //        loginDetails.Username = UsernameInput.Text;
                    //        loginDetails.UserPassword = PasswordInput.Text;
                    //        loginDetails.Site = Configuration_List.SelectedItem.ToString();
                    //        con.Insert(loginDetails);
                    //    }
                    //    var e = con.Table<LoginDetails>().ToList();
                    //    GetUserDetails();

                    //    retVal = true;
                    //}
                    //else if (LoginStorage.AccessToken != null)
                    //{
                    //    var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == UsernameInput.Text.ToLower() && a.UserPassword == PasswordInput.Text && a.Site == LoginStorage.Site).Count();
                    //    if (isLoginExists == 0)
                    //    {
                    //        loginDetails.Username = UsernameInput.Text;
                    //        loginDetails.UserPassword = PasswordInput.Text;
                    //        loginDetails.Site = Configuration_List.SelectedItem.ToString();
                    //        con.Insert(loginDetails);
                    //    }
                    //    var e = con.Table<LoginDetails>().ToList();
                    //    GetUserDetails();
                    //    retVal = true;

                    //}
                    //else
                    //{
                    //    retVal = false;
                    //}
                }
                else
                {
                    var isLoginExists = con.Table<LoginDetails>().Where(a => a.Site == LoginStorage.Site).Count();
                    var d = con.Table<LoginDetails>().ToList();
                    if (isLoginExists > 0)
                    {
                        retVal = true;
                        GetUserDetails();
                    }
                    else
                    {
                        retVal = false;
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //--------Cloud config Authentication method -----------//


        //---------Local Instance Authentication method ------------\\

        private async Task<bool> LocalAuthentication()
        {
            try
            {
                bool retVal = false;
                LoginDetails loginDetails = new LoginDetails();
                LoginStorage.Site = Configuration_List.SelectedItem.ToString().Remove(0, 5); 
                var name = UsernameInput.Text;
                var pwd = PasswordInput.Text;               
                LoginStorage.Username = name;
                LoginStorage.Password = pwd;
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    var config =  Configuration_List.SelectedItem.ToString();
                    string loginURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/token/" + config + "/" + name + "/" + pwd;
                    LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL, "GET", false, null, null);
                    if (LoginStorage.AccessToken != null && LoginStorage.AccessToken != "")
                    {
                        retVal = true;
                        var config1 =  Configuration_List.SelectedItem.ToString();
                        string loginURL1 = RestApiConstants.BaseUrl + "/IDORequestService/ido/token/" + config + "/sa";
                        LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL1, "GET", false, null, null);
                        var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == UsernameInput.Text.ToLower() && a.UserPassword == PasswordInput.Text && a.Site == LoginStorage.Site).Count();
                        if (isLoginExists == 0)
                        {
                            loginDetails.Username = UsernameInput.Text;
                            loginDetails.UserPassword = PasswordInput.Text;
                            string selectedconfig = Configuration_List.SelectedItem.ToString();
                            loginDetails.Site = selectedconfig.Remove(0, 5);
                            con.Insert(loginDetails);
                        }
                        var e = con.Table<LoginDetails>().ToList();
                        GetUserDetails();

                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
                else
                {
                    var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == UsernameInput.Text.ToLower() && a.UserPassword == PasswordInput.Text && a.Site == LoginStorage.Site).Count();
                    var d = con.Table<LoginDetails>().ToList();
                    if (isLoginExists > 0)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //---------Local Instance Authentication method ------------\\


        
        public async void IonAPIClicked(object sender, EventArgs e)
        {
            var file = await FilePicker.PickAsync();
            var fileExtension = file.FileName.Split('.').Last();
            if (fileExtension == "ionapi")
            {
                var fileName = file.FileName;
                var stream = await file.OpenReadAsync();
                var buffer = new byte[16 * 1024];
                var arr = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    arr = ms.ToArray();
                }
                JsonDataFromDocument = BytesToString(arr);
                var JsonDatadata = JsonConvert.DeserializeObject<JsonDocumentData>(JsonDataFromDocument);
                var type = file.ContentType;
            }
            else
            {
                await DisplayAlert("Error Message", "Invalid File", "OK");
            }
        }
        static string BytesToString(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
        public void GetDropDownData()
        {
            LoginDetails loginDetails = new LoginDetails();
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                // http://13.90.252.161/IDORequestService/ido/configurations
                string getCongigURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/configurations";
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(getCongigURL, "GET", true, null, null);
                var dynJson = JObject.Parse(properties);
                var data = JsonConvert.DeserializeObject<DropDownData>(properties);
                if(data.Success == true)
                {
                    List<string> configList = new List<string>();
                    foreach(var item in data.Configurations)
                    {
                        configList.Add(item);
                        //dropDownConfigData.Add(string.Join(",", configList));
                        LoginStorage.Configurations = string.Join(",", configList);                                          
                    }                
                }               
            }
        }
        public async void GetUserDetails()
        {
            try
            {
                string UserDataAndPartnerIdURL = "";
                var configData = con.Table<Configurations>().FirstOrDefault();
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_UserDataAndPartnerIdBySiteRef?clm=ERS_UserDataAndPartnerIdBySiteRefSP&clmparam=" + configData.UserName + "," + LoginStorage.Site;
                    var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                    var resJson = JObject.Parse(properties);
                    var resObj = resJson.SelectToken("Items");
                    string jtokenResStr = resObj.ToString(Newtonsoft.Json.Formatting.None);
                    if (jtokenResStr == "[]" || jtokenResStr == null || properties == string.Empty)
                    {
                        LoginStorage.Username = configData.UserName;
                        UserDetails userDetails = new UserDetails();
                        userDetails.Username = configData.UserName;
                        userDetails.ApptCount = "0";
                        userDetails.Site = LoginStorage.Site;
                        userDetails.Site_ref = LoginStorage.Site;
                        con.Insert(userDetails);
                        await Navigation.PushAsync(new TestApp2SyteLineServicePage());
                    }
                    else
                    {
                        var userdata = JsonConvert.DeserializeObject<UserDataProperties>(properties);
                        var dynJson = JObject.Parse(properties);
                        var url = dynJson.SelectToken("Items");
                        string jtokenStr = url.ToString(Newtonsoft.Json.Formatting.None);
                        if (jtokenStr != "null")
                        {
                            JArray array = JArray.Parse(jtokenStr);
                            con.DeleteAll<UserDetails>();
                            if (array.Count > 0)
                            {
                                for (int i = 0; i < array.Count; i++)
                                {
                                    JToken elem = array[i];
                                    // token string
                                    string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                                    var data = JsonConvert.DeserializeObject<UserDetails>(jtokenString);
                                    LoginStorage.Username = data.Username;
                                    Preferences.Set("PareternID", data.PartnerId);
                                    LoginStorage.PartnerId = data.PartnerId;
                                    UserDetails userDetails = new UserDetails();

                                    userDetails.PartnerId = data.PartnerId;
                                    userDetails.UserId = data.UserId;
                                    userDetails.Username = data.Username;
                                    userDetails.UserPassword = LoginStorage.Password;
                                    userDetails.ApptCount = data.AppointmentCount;
                                    userDetails.Site = data.Site_ref;
                                    userDetails.Site_ref = data.Site_ref;
                                    con.Insert(userDetails);
                                }
                            }
                            var Details = con.Table<UserDetails>().ToList();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error Message", "Something went wrong please try again", "Ok");
            }

        }

        //public void GetContractLineDetails(string objContractId, string objLineNumber)
        //{
        //    try
        //    {
        //        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        //        {
        //            LineItems items = new LineItems();
        //            MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
        //            string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl+"/IDORequestService/ido/load/ERS_GetContractLinesByContractId?clm=ERS_GetContractLinesDetailsByContractIdSP&clmparam=" + LoginStorage.Site + "," + objContractId + "," + objLineNumber;
        //            var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
        //            var dynJson = JObject.Parse(properties);
        //            var res = dynJson.SelectToken("Items");
        //            string jtokenStr = res.ToString(Newtonsoft.Json.Formatting.None);
        //            if (jtokenStr != "null")
        //            {
        //                JArray array = JArray.Parse(jtokenStr);
        //                for (int i = 0; i < array.Count; i++)
        //                {
        //                    JToken elem = array[i];
        //                    // token string
        //                    string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);

        //                    var data = JsonConvert.DeserializeObject<LineData>(jtokenString);
        //                    ContractLineSiteData contractLineDetails = new ContractLineSiteData();
        //                    var isContractLineExists = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractId && a.Cont_line == data.cont_line).Count();
        //                    if (isContractLineExists == 0)
        //                    {
        //                        contractLineDetails.Contract = objContractId;
        //                        contractLineDetails.Cont_line = data.cont_line;
        //                        contractLineDetails.description = data.description;
        //                        contractLineDetails.start_date = data.start_date;
        //                        contractLineDetails.end_date = data.end_date;
        //                        contractLineDetails.checkInDate = data.checkInDate;
        //                        contractLineDetails.checkOutDate = data.checkOutDate;
        //                        contractLineDetails.Billing_freq = data.Billingfreq;
        //                        contractLineDetails.ser_num = data.ser_num;
        //                        contractLineDetails.item = data.item;
        //                        contractLineDetails.qty = data.qty;
        //                        contractLineDetails.rate_conv = data.rate_conv;
        //                        contractLineDetails.unit_of_rate = data.UnitOfRate;
        //                        contractLineDetails.prorate_rate_conv = data.prorate_rate_conv;
        //                        contractLineDetails.prorate_unit_of_rate = data.prorate_unit_of_rate;
        //                        con.Insert(contractLineDetails);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        DisplayAlert("", ex.Message, "Ok");
        //    }
        //}

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        //void ZXingScannerView_OnScanResult(ZXing.Result result)
        //{
        //    Device.BeginInvokeOnMainThread(() => { var res = result.Text  });
        //}

        async void  qRCodeScanner_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<CameraPermission>();

                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted )
                {
                    bool shouldRequetPermission = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Camera);
                    if (shouldRequetPermission)
                    {
                        await DisplayAlert("Access Required for Camera", "Camera Required", "OK");
                    }

                    status = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();
                }

                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    var scanner = new ZXingScannerPage();
                    await Navigation.PushAsync(scanner);
                    scanner.OnScanResult += (result) => {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Navigation.PopAsync();
                            JsonDataFromDocument = result.Text;
                            
                        });
                    };
                }


            }
            catch
            {
                await DisplayAlert("ErrorMessage", "Something went wrong!!Try Again", "Ok");
            }

        }

        void PasswordInput_Completed(System.Object sender, System.EventArgs e)
        {
            GetDropDownData();
        }
    }
}
public class JsonDocumentData
{
    public string ti { get; set; }
    public string cn { get; set; }
    public string dt { get; set; }
    public string ci { get; set; }
    public string cs { get; set; }
    public string iu { get; set; }
    public string pu { get; set; }
    public string oa { get; set; }
    public string ot { get; set; }
    public string or { get; set; }
    public string ru { get; set; }
    public string ev { get; set; }
    public string v { get; set; }
}
public class DropDownData
{
    public object Message { get; set; }
    public bool Success { get; set; }
    public List<string> Configurations { get; set; }
}