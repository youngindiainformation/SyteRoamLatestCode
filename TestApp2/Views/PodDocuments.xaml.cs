using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using TestApp2.Common.APIclasses;
using TestApp2.Models.SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodDocuments : ContentPage
    {
        private SQLiteConnection con;
        public PodDocuments(string contractId)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            GetProofOfDelivery(contractId);
        }

        public void GetProofOfDelivery(string contractId)
        {
            try
            {
                List<PodDocumentDetails> listWorkOrderData = con.Table<PodDocumentDetails>().Where(a => a.podContractID == contractId).ToList();
                ListPOD.ItemsSource = listWorkOrderData?.OrderByDescending(x => x.podID);
                if (listWorkOrderData?.Count!=0)
                {
                    NoItems.IsVisible=false;
                }
                else
                {
                    NoItems.IsVisible = true;
                }
            }
            catch (Exception ex)
            {

              
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            ImageButton imageButton = (ImageButton)sender;
            Label lblWorkerOrderID = (Label)imageButton.FindByName("WorkerOrderID");
            Navigation.PushAsync(new PodDocumentView(Convert.ToInt32(lblWorkerOrderID.Text)));
        }
    }
}