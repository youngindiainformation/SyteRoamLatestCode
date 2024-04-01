using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomersResultListPage : ContentPage
    {
        private SQLiteConnection con;
        public CustomersResultListPage()
        {
            InitializeComponent();
        }
        //public CustomersResultListPage()
        //{

        //    con = DependencyService.Get<ISQLite>().GetConnection();
        //    InitializeComponent();
        //    GetCustomerDetails(custNum);
        //    Title = "Ship_Tos " + custName;
        //}
        
        private void MenuItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //var searchresult = scheduleDetails.Where(a => a.CustomerName.ToLower().Contains(CustomerSearch.Text.ToLower()));
            //MenuItemsListView.ItemsSource = searchresult;
        }
    }
}