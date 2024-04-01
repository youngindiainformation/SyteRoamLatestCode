using Acr.UserDialogs;
using TestApp2.Common.APIclasses;
using TestApp2.Common.Helper;
using TestApp2.Models.SQLite;
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
    public partial class CustomersSearchPage : ContentPage
    {
        ObservableCollection<CustScheduleDetails> scheduleDetails = new ObservableCollection<CustScheduleDetails>();
        
        private SQLiteConnection con;
        public CustomersSearchPage()
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            var list = con.Table<TestApp2.Models.SQLite.CustomerListData>().ToList();
            int count = 10;
            foreach (var item in list)
            {
                if (count > 0)
                {
                    scheduleDetails.Add(new CustScheduleDetails { CustomerName = item.Name, Address = item.State, CustNum=item.Cust_num});
                    count--;
                }
            }
            if (scheduleDetails.Count > 0)
            {
                MenuItemsListView.ItemsSource = scheduleDetails;
            }
        }               
        private void CustomerClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CustomersResultListPage());
        }
        private async void MenuItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ScheduleDetails objScheduleDetails = (ScheduleDetails)e.Item;
            //await  Navigation.PushAsync(new CustomersResultListPage(objScheduleDetails.CustomerName, objScheduleDetails.CustNum));
        }
        private async void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            //{
            //    UserDialogs.Instance.ShowLoading("Loading results please wait...");
            //    Uri url = new Uri(APIUrls.GetCustomersListBySiteRef + "DALS");
            //    HttpClient client = new HttpClient();
            //    var content = await client.GetStringAsync(url.AbsoluteUri);
            //    var tr = JsonConvert.DeserializeObject<GetCustomersListBySiteRefAPI>(content);
            //    List<TestApp2.Common.APIclasses.CustomerListData> custlist = tr.CustomerListData;
            //   // var a = new TestApp2.Common.APIclasses.CustomerListData();
            //    if (custlist.Count>0)
            //    {
            //        try
            //        {
            //            ObservableCollection<ScheduleDetails> searchDetails = new ObservableCollection<ScheduleDetails>();
            //            foreach (var item in custlist)
            //            {
            //               // a = item;
            //                if ((item.Name != null) && (item.Name.ToLower().Contains(CustomerSearch.Text.ToLower())))
            //                {
            //                    searchDetails.Add(new ScheduleDetails { CustomerName = item.Name, Address = item.State, CustNum = item.Cust_num });
            //                }
            //            }
                        
            //            MenuItemsListView.ItemsSource = searchDetails;
            //        }
            //        catch(Exception ex)
            //        {
            //           // var b = a;
            //        }
            //    }
            //    UserDialogs.Instance.HideLoading();
            //}
            //else
            //{
            //    var searchresult = scheduleDetails.Where(a => a.CustomerName.ToLower().Contains(CustomerSearch.Text.ToLower()));
            //    MenuItemsListView.ItemsSource = searchresult;
            //}
        }
    }
    public class CustScheduleDetails
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }

        public string CustNum { get; set; }
    }
}
