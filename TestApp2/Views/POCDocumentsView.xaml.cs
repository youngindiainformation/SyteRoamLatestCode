using Plugin.Permissions;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp2.Models;
using TestApp2.Models.SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestApp2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class POCDocumentsView : ContentPage
    {
        private SQLiteConnection con;
        int downloadCollectionOrderID = 0;
        public POCDocumentsView(int CollectionOrderID)
        {
            InitializeComponent();
            con = DependencyService.Get<ISQLite>().GetConnection();
            PocDocumentDetails collectionWorkOrderData = con.Table<PocDocumentDetails>().Where(a => a.POCOrderID == CollectionOrderID).FirstOrDefault();

            downloadCollectionOrderID = CollectionOrderID;
            Stream stream = new MemoryStream(collectionWorkOrderData.PocDoc);
            srcPocDocument.Source = ImageSource.FromStream(() => { return stream; });

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
                    PocDocumentDetails collectionWorkOrderData = con.Table<PocDocumentDetails>().Where(a => a.POCOrderID == downloadCollectionOrderID).FirstOrDefault();
                    String myDate = DateTime.Now.ToString("ddMMyyyy_HHmmSS");
                    Stream imageStream = new MemoryStream(collectionWorkOrderData.PocDoc);

                    //DependencyService.Get<IMediaService>().SavePicture("Infor_" + myDate + ".jpeg", imageStream, "Download"); ;

                    DependencyService.Get<IMediaService>().SaveImageFromByteAsync(collectionWorkOrderData.PocDoc, "Infor_" + myDate + ".jpeg");
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