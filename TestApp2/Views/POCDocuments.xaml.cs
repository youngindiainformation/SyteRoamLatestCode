using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Models.SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class POCDocuments : ContentPage
    {
        private SQLiteConnection con;
        public POCDocuments(string CollectionContractID)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            GetProofOfCollection(CollectionContractID);
        }

        public void GetProofOfCollection(string CollectionContractID)
        {
            List<PocDocumentDetails> listWorkOrderData = con.Table<PocDocumentDetails>().Where(a => a.CollectionContractID == CollectionContractID).ToList();
            ListPOC.ItemsSource = listWorkOrderData.OrderByDescending(x => x.CollectionContractID);
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            ImageButton imageButton = (ImageButton)sender;
            Label lblCollectionOrderID = (Label)imageButton.FindByName("CollectionOrderID");
            Navigation.PushAsync(new POCDocumentsView(Convert.ToInt32(lblCollectionOrderID.Text)));
        }
    }
}