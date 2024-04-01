using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthorizationPage : ContentPage
    {
        
        public AuthorizationPage()
        {
            InitializeComponent();
            ContractID.Text = AppointmentDetailPage.objContractId;
            CustomerName.Text = AppointmentDetailPage.objCustName;
        }

        private async void AuthorizationSubmitClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}