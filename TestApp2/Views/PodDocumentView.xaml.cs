using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using TestApp2.Models.SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
//using Android;
//using Android.Content.PM;
using Xamarin.Essentials;
using TestApp2.Models;
using Plugin.Permissions;
using System.Windows.Input;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodDocumentView : ContentPage
    {
        private SQLiteConnection con;
        int downloadWorkOrderId = 0;
        public PodDocumentView(int workOrderId)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            PodDocumentDetails workOrderData = con.Table<PodDocumentDetails>().Where(a => a.podID == workOrderId).FirstOrDefault();

            downloadWorkOrderId = workOrderId;
            Stream stream = new MemoryStream(workOrderData.PodDoc);
            srcPodDocument.Source = ImageSource.FromStream(() => { return stream; });

        }

        private async void btnDownload_Clicked(object sender, EventArgs e)
        {

            try
            {
                var mediaLibrary = await CrossPermissions.Current.CheckPermissionStatusAsync<MediaLibraryPermission>();
                var photosStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<PhotosPermission>();
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();

                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    bool shouldRequetPermission = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                    if (shouldRequetPermission)
                    {
                        await DisplayAlert("Access Required for Photos", "Photos Required", "OK");
                    }

                    status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                }

                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    PodDocumentDetails workOrderData = con.Table<PodDocumentDetails>().Where(a => a.podID == downloadWorkOrderId).FirstOrDefault();
                    String myDate = DateTime.Now.ToString("ddMMyyyy_HHmmSS");
                    Stream imageStream = new MemoryStream(workOrderData.PodDoc);

                    //DependencyService.Get<IMediaService>().SavePicture("Infor_" + myDate + ".jpeg", imageStream, "Download"); ;

                    DependencyService.Get<IMediaService>().SaveImageFromByteAsync(workOrderData.PodDoc, "Infor_"+ myDate + ".jpeg");
                    await DisplayAlert("Notification", "Successfully Downloaded", "OK");

                    //MessagingCenter.Subscribe<Droid.MediaService>(this, "Downloaded", async (sender) =>
                    //{
                    //    await DisplayAlert("Notification", "Successfully Downloaded", "OK");
                    //});
                }
                else if (!(status == Plugin.Permissions.Abstractions.PermissionStatus.Unknown))
                {
                    //Storage Permission denied
                    await DisplayAlert("Access Required for Photos", "Photos Required", "OK");
                }
            }
            catch (Exception ex)
            {
                //Something went wrong
            }

        }

    }
}
