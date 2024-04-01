using System;
using SQLite;
using Xamarin.Forms;

namespace TestApp2.Models.SQLite
{
    public class PocDocumentDetails 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int POCOrderID { get; set; }
        public string CollectionName { get; set; }
        public string CollectionTime { get; set; }
        public string CollectionDate { get; set; }
        public string CollectionLineNumbers { get; set; }
        public string CollectionContractID { get; set; }
        public byte[] PocDoc { get; set; }
    }
}

