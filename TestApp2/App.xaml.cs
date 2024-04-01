using System;
using SQLite;
using TestApp2.Common.Helper;
using TestApp2.Common.SQLite;
using TestApp2.Verify;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2
{
    public partial class App : Application
    {
        private SQLiteConnection con;
        public App()
        {
            InitializeComponent();

            var currentVersion = VersionTracking.CurrentVersion;
            var previousVersion = VersionTracking.PreviousVersion;
            con = DependencyService.Get<ISQLite>().GetConnection();
            if (Preferences.Get("IsCloudConfig", false) == true)
            {
                LoginStorage.IsCloudConfig = true;
                LoginStorage.PartnerId = Preferences.Get("PareternID", LoginStorage.PartnerId);
                RestApiConstants.BaseUrl = Preferences.Get("CloudBaseURL", RestApiConstants.BaseUrl);
            }
            MainPage = new NavigationPage(new Login());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
