using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace TestApp2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]    
    public partial class Note : ContentPage
    {
        public Note()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            Backcalled();
            return true;
        }

        


        public async void Backcalled()
        {
            if (await DisplayAlert("Exit?", "Do you want to save?", "Yes", "No"))
            {
                if (!(DateTimeEntry.Text == null))
                {
                    MessagingCenter.Send(this, "ListData", DateTimeEntry.Text);
                    MessagingCenter.Send(this, "Subnotes", EditorText.Text);
                    base.OnBackButtonPressed();
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("", "'Note' field can't be empty!", "Ok");
                }
            }
            else
            {
                await Navigation.PopAsync();
                base.OnBackButtonPressed();
            }
        }
    }
}