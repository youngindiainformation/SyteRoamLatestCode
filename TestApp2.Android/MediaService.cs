using System;
using System.Runtime.Remoting.Contexts;
using Plugin.CurrentActivity;
using TestApp2.Models;
using Android.Content;
using Xamarin.Forms;
using TestApp2.Droid;
using System.IO;
using Android.Widget;
using Android.App;

[assembly: Dependency(typeof(MediaService))]
namespace TestApp2.Droid
{
    public class MediaService : IMediaService
    {
        Android.Content.Context CurrentContext => CrossCurrentActivity.Current.Activity;

        void IMediaService.ClearFileDirectory()
		{
            var directory = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), ImageHelpers.collectionName).Path.ToString();

            if (Directory.Exists(directory))
            {
                var list = Directory.GetFiles(directory, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        File.Delete(list[i]);
                    }
                }
            }
		}


        public static int OPENGALLERYCODE = 100;
        public void OpenGallery()
        {
            try
            {
                var imageIntent = new Intent(Intent.ActionPick);
                imageIntent.SetType("image/*");
                imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);
                imageIntent.SetAction(Intent.ActionGetContent);
                ((Activity)Forms.Context).StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), OPENGALLERYCODE);
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Tap and hold to select multiple photos.", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Error. Can not continue, try again.", ToastLength.Long).Show();
            }
        }

        public void SaveImageFromByteAsync(byte[] imageByte, string filename)
        {
            try
            {
                //Java.IO.File storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                //string path = System.IO.Path.Combine(storagePath.ToString(), filename);
                //string path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                string path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                //string path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                //string testpath = Path.Combine(path, "Images", "Infor");
                //path = Path.Combine(path, "Images", "Infor");
                //Directory.CreateDirectory(path);
                string testpath = System.IO.Path.Combine(path, filename);

                System.IO.File.WriteAllBytes(testpath, imageByte);
                var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(testpath)));
                CurrentContext.SendBroadcast(mediaScanIntent);
                Console.WriteLine("Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception = " + ex);
            }
        }

        public void SavePicture(string name, Stream data, string location = "temp")
        {
            //string path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures).AbsolutePath;
            //var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //documentsPath = Path.Combine(documentsPath, "Orders", location);
            //Directory.CreateDirectory(documentsPath);
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            documentsPath = Path.Combine(documentsPath, "Images", location);
                Directory.CreateDirectory(documentsPath);

            //string testPath = Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath + $"{name}.png";

            string filePath = Path.Combine(documentsPath, name);

            byte[] bArray = new byte[data.Length];
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (data)
                {
                    data.Read(bArray, 0, (int)data.Length);
                }
                int length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
        }
    }
}
