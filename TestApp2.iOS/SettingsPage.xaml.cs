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
            web_Server.Text = "https://mingle-portal.inforcloudsuite.com";
            config_group.Text = "QUADKOR_AX1";
            inputUserName.Text = "jfredric@quadkor.com";
            inputPassword.Text = "mYK]39%v34";
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

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            SQLiteConnection con;
            con = DependencyService.Get<ISQLite>().GetConnection();
            con.DeleteAll<LoginDetails>();
            con.DeleteAll<UserDetails>();
            con.DeleteAll<AppointmentList>();
            con.DeleteAll<ContractDetails>();
            con.DeleteAll<ContractLineDetails>();
            //con.DeleteAll<WorkOrderData>();
            con.DeleteAll<ContractLineSiteData>();
            DisplayAlert("", "Cleared Local Database successfully!!", "ok");
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
                //https://mingle-cqa-ionapi.cqa.inforcloudsuite.com/CSI10DEV1_TST/CSI/IDORequestService/ido/configurations?configgroup=CSI10DEV1_TST
                var httpClient = new HttpClient();
                string generated_url = web_Server.Text.ToString() + "/" + config_group.Text.ToString() + "/CSI/IDORequestService/ido/configurations?configgroup=" + config_group.Text.ToString();
                RestApiConstants.BaseUrl = generated_url;
                MongooseAPIRequest mongooseAPIRequest = new MongooseAPIRequest();
                var properties = mongooseAPIRequest.ProcessRestAPIRequest(generated_url, "GET", false, null, null, 1);
                List<string> configList = JsonConvert.DeserializeObject<List<string>>(properties);
                configData.config = string.Join(",", configList);
                con.Insert(configData);
                //var configList = configdata.ConvertAll(x => new Configurations(x));



                var validateUrl = web_Server.Text.ToString() + "/" + config_group.Text.ToString() + "/CSI/IDORequestService/ido/token/" + configList[0] + "/" + inputUserName.Text.ToString() + "/" + inputPassword.Text.ToString();
                LoginStorage.AccessToken = mongooseAPIRequest.ProcessRestAPIRequest(validateUrl, "GET", false, null, null);
                Configurations configurationsData = con.Table<Configurations>().FirstOrDefault();

                if (configurationsData != null && Configuration_Settings_List == null)
                {
                    var csv = configurationsData.config;
                    String[] elements = csv.Split(',');
                    foreach (var item in elements)
                    {
                        Configuration_Settings_List.Items.Add(item);
                    }
                }
                if (LoginStorage.AccessToken == null || LoginStorage.AccessToken == string.Empty)
                {
                    LoginStorage.IsCloudConfig = false;
                    await DisplayAlert("Error Message", "Your have entered Invalid data", "Ok");

                }
                else
                {
                    LoginStorage.IsCloudConfig = true;
                    await DisplayAlert("Success Message", "You have been successfully authenticated with system", "Ok");
                    MessagingCenter.Send<SettingsPage>(this, "dropDownData");
                    await Navigation.PopAsync();
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("", ex.Message, "ok");
            }

        }
    }
}

public class LoginDropDown
{
    string data { set; get; }
}