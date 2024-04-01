using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using TestApp2.Common.APIclasses;
using TestApp2.Common.SQLite;
using System;
using Xamarin.Essentials;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private SQLiteConnection con = DependencyService.Get<ISQLite>().GetConnection();

        public object Arrays { get; private set; }
        public string AccessToken { get; private set; }

        public SettingsPage()
        {
            InitializeComponent();
            //web_Server.Text = "https://mingle-ionapi.inforcloudsuite.com";
            //config_group.Text = "QUADKOR_AX1";
            //inputUserName.Text = "jfredric@quadkor.com";
            //inputPassword.Text = "pass@123";
            con = DependencyService.Get<ISQLite>().GetConnection();
            Configurations conData = con.Table<Configurations>().FirstOrDefault();
            if (conData != null)
            {
                web_Server.Text = conData.webUrl;
                config_group.Text = conData.configGroup;
                inputUserName.Text = conData.UserName;
                var csv = conData.config;
                String[] elements = csv.Split(',');
                foreach (var item in elements)
                {
                    Configuration_Settings_List.Items.Add(item);
                }
            }
        }
        
        void toggleButton_Toggled(object sender, ToggledEventArgs e)
        {
            UserDetailsField.IsVisible = true;
        }

        public async void Submit_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                con = DependencyService.Get<ISQLite>().GetConnection();
                Configurations configData = new Configurations();
                configData.webUrl = web_Server.Text.ToString();
                configData.configGroup = config_group.Text.ToString();
                configData.UserName = inputUserName.Text.ToString();
                configData.UserPassword = inputPassword.Text.ToString();
                RestApiConstants.CongfigurationGroup = config_group.Text.ToString();
                //https://mingle-cqa-ionapi.cqa.inforcloudsuite.com/CSI10DEV1_TST/CSI/IDORequestService/ido/configurations?configgroup=CSI10DEV1_TST
                var httpClient = new HttpClient();
                string generated_url = web_Server.Text.ToString() + "/" + config_group.Text.ToString() + "/CSI/IDORequestService/ido/configurations?configgroup=" + config_group.Text.ToString();
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(generated_url, "GET", false, null, null, 1);
                List<string> configList = JsonConvert.DeserializeObject<List<string>>(properties);
                configData.config = string.Join(",", configList);
                //  con.Insert(configData);
                //var configList = configdata.ConvertAll(x => new Configurations(x));



                var validateUrl = web_Server.Text.ToString() + "/" + config_group.Text.ToString() + "/CSI/IDORequestService/ido/token/" + configList[0] + "/" + inputUserName.Text.ToString() + "/" + inputPassword.Text.ToString();
                LoginStorage.Site = configList[0].ToString();
                LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(validateUrl, "GET", false, null, null);

                if (LoginStorage.AccessToken == null || LoginStorage.AccessToken == string.Empty)
                {
                    await DisplayAlert("Error Message", "Your have entered Invalid data", "Ok");

                }
                else
                {
                    con.DeleteAll<Configurations>();
                    con.Insert(configData);
                    Configurations configurationsData = con.Table<Configurations>().FirstOrDefault();
                    if (configurationsData != null)
                    {
                        var csv = configurationsData.config;
                        Configuration_Settings_List.Items.Clear();
                        String[] elements = csv.Split(',');
                        foreach (var item in elements)
                        {
                            Configuration_Settings_List.Items.Add(item);
                        }

                    }
                    string cloudbaseURl = web_Server.Text + "/" + config_group.Text + "/CSI";
                    Preferences.Set("CloudBaseURL", cloudbaseURl);
                    Preferences.Set("IsCloudConfig", true);
                    RestApiConstants.BaseUrl = cloudbaseURl;
                    RestApiConstants.CongfigurationGroup = config_group.Text;
                    LoginStorage.CloudBaseURL = web_Server.Text;
                    LoginStorage.IsCloudConfig = true;
                    await DisplayAlert("Success Message", "You have been successfully authenticated with system", "Ok");
                    MessagingCenter.Send<SettingsPage>(this, "dropDownData");
                    await Navigation.PopAsync();
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error Message", "Something went wrong please try again", "Ok");
            }

        }
        void Configuration_Settings_List_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }
    }

}
public class LoginDropDown
{
    string data { set; get; }
}
