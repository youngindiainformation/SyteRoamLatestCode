using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Plugin.Media;
using Acr.UserDialogs;
using TestApp2.ViewModels;
using TestApp2.Common.Helper;
using SQLite;
using TestApp2.Models.SQLite;
using System.Collections.ObjectModel;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewLinePage : ContentPage
    {
        public List<string> AttachmentList { get; set; } = new List<string>();
        public string objContractID { get; set; }
        public int objLineNumber { get; set; }

        private SQLiteConnection con;
        public NewLinePage()
        {
            InitializeComponent();
        }
        public NewLinePage(string ContractId, int LineCount)
        {
            con = DependencyService.Get<ISQLite>().GetConnection();
            InitializeComponent();
            objContractID = ContractId;
            objLineNumber = LineCount + 1;
            NewLineNum.Text = objLineNumber.ToString();
            NewLineLeadPartner.Text = LoginStorage.Username;
            List<string> newLineStatusList = new List<string>()
            {

                "Open",
                "Closed",
                "Estimate",
                "Hold",
                "Picked",
                "Packed",
                "Shipped",
                "Pickup"
            };
            NewLineStatusList.ItemsSource = newLineStatusList;
        }
        private void LineMenuClicked(object sender, EventArgs e)
        {

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
                var isContractLineSiteData = con.Table<ContractLineSiteData>().Where(a => a.Contract == objContractID && a.Cont_line == NewLineNum.Text && a.siteRef == "DALS").Count();
                if (isContractLineSiteData == 0)
                {
                    ContractLineSiteData contractLineSiteData = new ContractLineSiteData();
                    contractLineSiteData.Cont_line = NewLineNum.Text;
                    contractLineSiteData.Status = (string)NewLineStatusList.SelectedItem;
                    contractLineSiteData.ser_num = NewLineSerialNum.Text;
                    contractLineSiteData.item = NewLineItem.Text;
                    contractLineSiteData.description = NewLineDescription.Text;
                    contractLineSiteData.u_m = NewUM.Text;
                    contractLineSiteData.qty = NewQuantity.Text;
                    contractLineSiteData.rate = NewRate_conv.Text;
                    contractLineSiteData.unit_of_rate = NewUnit_of_rate.Text;
                    contractLineSiteData.prorate_rate_conv = NewProrate_rate_conv.Text;
                    contractLineSiteData.prorate_unit_of_rate = NewProrate_unit_of_rate.Text;
                    //contractLineSiteData.start_date = NewStartDate.Date;
                    //contractLineSiteData.end_date = NewEndDate.Date;
                    //contractLineSiteData.checkInDate = NewCheckInDate.Date;
                    //contractLineSiteData.checkOutDate = NewCheckOutDate.Date;
                    contractLineSiteData.billingFreq = NewBillingFrequency.Text;
                    con.Insert(contractLineSiteData);
                }
                List<ContractLineDetails> contractLinesList = (from ld in con.Table<ContractLineDetails>().Where(a => a.Contract == objContractID && a.Cont_line == NewLineNum.Text) select ld).ToList();
                if (contractLinesList.Count == 0)
                {
                    ContractLineDetails contractLineDetails = new ContractLineDetails();
                    contractLineDetails.Cont_line = NewLineNum.Text;
                    contractLineDetails.Ser_num = NewLineSerialNum.Text;
                    contractLineDetails.Item = NewLineItem.Text;
                    contractLineDetails.Description = NewLineDescription.Text;
                    contractLineDetails.Qty = NewQuantity.Text;
                    contractLineDetails.Contract = objContractID;
                    contractLineDetails.Site_ref = "DALS";
                    con.Insert(contractLineDetails);
                }


                await Navigation.PopAsync();
                base.OnBackButtonPressed();
            }
            else
            {
                await Navigation.PopAsync();
                base.OnBackButtonPressed();
            }
        }
        private void ClockClicked(object sender, EventArgs e)
        {
            if (!(ClockedOnText.IsVisible && ClockedOnTextLabel.IsVisible))
            {
                ClockedOnText.Text = DateTime.Now.ToString();
                ClockedOnTextLabel.IsVisible = true;
                ClockedOnText.IsVisible = true;
                clockId.Source = "clockicongreen.jpg";
            }
            else if (ClockedOnTextLabel.IsVisible && ClockedOnText.IsVisible)
            {
                ClockedOnOptions();
            }
        }
        private async void ClockedOnOptions()
        {
            var action = await DisplayActionSheet("Please select how to proceed with current clock on: ", "Cancel", null, "Clock Off", "Clear Clock On", "Remain Clocked On");

            if (action != null)
            {
                switch (action)
                {
                    case "Clock Off":
                        await DisplayAlert("", "You are Currently Clocked On To: DS00000002" + "\n" + "Operation must exist where Status is Open.", "OK");
                        break;

                    case "Clear Clock On":
                        ClockedOnText.IsVisible = false;
                        ClockedOnTextLabel.IsVisible = false;
                        clockId.Source = "clockiconorange.jpg";
                        break;

                    case "Remain Clocked On":
                        break;

                    default:
                        break;
                }

            }

        }
        private async void CalendarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Notes());
        }
        private async void CameraClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync
                (new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                    Directory = "Pictures",
                    Name = "test.jpg"
                }); ;

            if (file == null)
                return;
            image.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                //file.Dispose();
                return stream;
            });
            AttachmentList.Add(file.Path);
        }

        private async void GalleryClicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Warning", "Cannot select image", "Cancel");
                return;
            }
            var pickedphoto = new Plugin.Media.Abstractions.PickMediaOptions()
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            };
            var selectedimage = await CrossMedia.Current.PickPhotoAsync(pickedphoto);
            if (selectedimage == null)
            {
                await DisplayAlert("Warning", "Could not load the image, try again!", "Cancel");
                return;
            }
            AttachmentList.Add(selectedimage.Path);
        }
        private async void AttachmentClicked(object sender, EventArgs e)
        {
            ObservableCollection<AttachmentsViewModel> AttachmentList = new ObservableCollection<AttachmentsViewModel>();
            await Navigation.PushAsync(new AttachmentsPage(AttachmentList, objContractID));
        }
        private async void NotepadClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkOrderPage());
        }
    }
}