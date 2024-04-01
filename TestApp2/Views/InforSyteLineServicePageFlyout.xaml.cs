using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
using TestApp2.Verify;
using TestApp2.ViewModels;
using TestApp2.Views;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestApp2SyteLineServicePageFlyout : ContentPage
    {
        public ListView ListView;

        public TestApp2SyteLineServicePageFlyout()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = new TestApp2SyteLineServicePageFlyoutViewModel();
            ListView = MenuItemsListView;
            Username.Text = LoginStorage.Username;           
        }      
        class TestApp2SyteLineServicePageFlyoutViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<TestApp2SyteLineServicePageFlyoutMenuItem> MenuItems { get; set; }
            
            public TestApp2SyteLineServicePageFlyoutViewModel()
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    MenuItems = new ObservableCollection<TestApp2SyteLineServicePageFlyoutMenuItem>(new[]
                    {
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 0, Title = "Appointments",Flyoutouticon="appointment.jpeg",TargetType=typeof(AppointmentsHomePage)},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 1, Title = "Contracts",Flyoutouticon="incident.png",TargetType = typeof(RentalContractsPage)},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 2, Title = "Partners",Flyoutouticon="partners.png"},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 3, Title = "Customers",Flyoutouticon="customer.png",TargetType=typeof(CustomersSearchPage)},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 4, Title = "Units",Flyoutouticon="units.png"},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 5, Title = "Item Availability",Flyoutouticon="items.jpeg"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 2, Title = "Sync",Flyoutouticon="sync.png"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 3, Title = "Change Site",Flyoutouticon="sitechange.png"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 4, Title = "Settings", Flyoutouticon="settings.png" },
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 5, Title = "Sign Out", Flyoutouticon="signouticon.png", TargetType=typeof(Login)  }
                    });
                }
                else
                {
                    MenuItems = new ObservableCollection<TestApp2SyteLineServicePageFlyoutMenuItem>(new[]
                    {
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 0, Title = "Appointments",Flyoutouticon="appointment.jpeg",TargetType=typeof(AppointmentsHomePage)},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 1, Title = "Contracts",Flyoutouticon="incident.png"},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 2, Title = "Partners",Flyoutouticon="partners.png"},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 3, Title = "Customers",Flyoutouticon="customer.png",TargetType=typeof(CustomersSearchPage)},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 4, Title = "Units",Flyoutouticon="units.png"},
                        //new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 5, Title = "Item Availability",Flyoutouticon="items.jpeg"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 2, Title = "Sync",Flyoutouticon="sync.png"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 3, Title = "Change Site",Flyoutouticon="sitechange.png"},
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 4, Title = "Settings", Flyoutouticon="settings.png" },
                        new TestApp2SyteLineServicePageFlyoutMenuItem { Id = 5, Title = "Sign Out", Flyoutouticon="signouticon.png", TargetType=typeof(Login)  }
                    });
                }

            }
            
            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
           
        }
    }
}