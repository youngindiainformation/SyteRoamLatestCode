using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestApp2.Models
{
    public interface IMediaService
    {
        void SaveImageFromByteAsync(byte[] imageByte, string filename);
        void SavePicture(string name, Stream data, string location = "temp");
        void OpenGallery();
        void ClearFileDirectory();
    }
}
