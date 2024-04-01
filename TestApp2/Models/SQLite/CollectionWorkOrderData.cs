using System;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class CollectionWorkOrderData
    {
        [PrimaryKey]
        [AutoIncrement]
        public int CollectionOrderID { get; set; }
        public string CollectionContractID { get; set; }
        public string CollectionPartnerID { get; set; }
        public string CollectionLineNumbers { get; set; }
        public string CollectionSignature { get; set; }
        public byte[] CollectionPocDocument { get; set; }
        public string CollectionDate { get; set; }
        public string CollectionName { get; set; }
        public string CollectionTime { get; set; }
        public string CollectionSubmitDate { get; set; }
        public string CollectionQuantity { get; set; }
    }
}
