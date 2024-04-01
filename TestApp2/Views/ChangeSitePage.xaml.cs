using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TestApp2.Views
{
    public partial class ChangeSitePage : ContentPage
    {
        private SQLiteConnection con;

        public ChangeSitePage()
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();           
            currentSite.Text = LoginStorage.Site;
            GetDropDownData();
        }
        public void GetDropDownData()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                // http://13.90.252.161/IDORequestService/ido/configurations
                string getCongigURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/configurations";
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(getCongigURL, "GET", true, null, null);
                var dynJson = JObject.Parse(properties);
                var data = JsonConvert.DeserializeObject<DropDownData>(properties);
                if (data.Success == true)
                {
                    foreach (var item in data.Configurations)
                    {
                        var currentSite = "Demo_" + LoginStorage.Site;
                        if (!item.Equals(currentSite))
                        {
                            Config_List.Items.Add(item);
                        }
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
                        Config_List.Items.Add(item);
                    }
                }
            }
        }
        async void ChangeSiteClicked(System.Object sender, System.EventArgs e)
        {
            bool response = await Authentication();
            if (response == true)
            {
                await DisplayAlert("Success Message", "Your site configuration has been changed successfully.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error Message", "Please turn on internet connecetion to use this feature.", "OK");
            }
        }
        private async Task<bool> Authentication()
        {
            try
            {
                bool retVal = false;
                LoginDetails loginDetails = new LoginDetails();
                LoginStorage.Site = Config_List.SelectedItem.ToString().Remove(0, 5);
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                    var config = Config_List.SelectedItem.ToString();
                    string loginURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/token/" + config + "/" + LoginStorage.Username + "/" + LoginStorage.Password;
                    LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL, "GET", false, null, null);
                    if (LoginStorage.AccessToken != null && LoginStorage.AccessToken != "")
                    {
                        retVal = true;
                        var config1 = Config_List.SelectedItem.ToString();
                        string loginURL1 = RestApiConstants.BaseUrl + "/IDORequestService/ido/token/" + config + "/sa";
                        LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(loginURL1, "GET", false, null, null);
                        var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == LoginStorage.Username.ToLower() && a.UserPassword == LoginStorage.Password).Count();
                        if (isLoginExists == 1)
                        {
                            loginDetails.Username = LoginStorage.Username.ToString();
                            loginDetails.UserPassword = LoginStorage.Password.ToString();
                            string selectedconfig = Config_List.SelectedItem.ToString();
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
                    var isLoginExists = con.Table<LoginDetails>().Where(a => a.Username.ToLower() == LoginStorage.Username.ToLower() && a.UserPassword == LoginStorage.Password && a.Site == LoginStorage.Site).Count();
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
        public async void GetUserDetails()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                string UserDataAndPartnerIdURL = RestApiConstants.BaseUrl + "/IDORequestService/ido/load/ERS_UserDataAndPartnerIdBySiteRef?clm=ERS_UserDataAndPartnerIdBySiteRefSP&clmparam=" + LoginStorage.Username + "," + LoginStorage.Site;
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(UserDataAndPartnerIdURL, "GET", true, null, LoginStorage.AccessToken);
                if (properties == null || properties == string.Empty)
                {
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
                        for (int i = 0; i < array.Count; i++)
                        {
                            JToken elem = array[i];
                            // token string
                            string jtokenString = elem.ToString(Newtonsoft.Json.Formatting.None);
                            var data = JsonConvert.DeserializeObject<UserDetails>(jtokenString);
                            LoginStorage.Username = data.Username;
                            LoginStorage.PartnerId = data.PartnerId;
                            UserDetails userDetails = new UserDetails();
                            if (data.PartnerId != null)
                            {
                                userDetails.PartnerId = data.PartnerId;
                                userDetails.UserId = data.UserId;
                                userDetails.Username = data.Username;
                                userDetails.UserPassword = LoginStorage.Password;
                                userDetails.ApptCount = data.AppointmentCount;
                                userDetails.Site = data.Site_ref;
                                con.Update(userDetails);
                            }
                        }
                    }
                }
            }

        }
    }
}
