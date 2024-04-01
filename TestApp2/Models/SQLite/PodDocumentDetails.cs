using System;
using SQLite;
using Xamarin.Forms;

namespace TestApp2.Models.SQLite
{
    public class PodDocumentDetails 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int podID { get; set; }
        public string PodName { get; set; }
        public string podTime { get; set; }
        public string podDate { get; set; }
        public string podLineNumbers { get; set; }
        public string podContractID { get; set; }
        public byte[] PodDoc { get; set; }
    }
}

