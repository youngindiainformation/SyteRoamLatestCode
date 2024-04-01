using System;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class YardReturnPostTemplate
    {
        [PrimaryKey]
        [AutoIncrement]
        public int PostYardReturnTempID { get; set; }
        public string siteId { get; set; }
        public string SelectedStatus { get; set; }
        public string contractID { get; set; }
        public string LineNumber { get; set; }
        public string Signature { get; set; }
        public string SignedTime { get; set; }
        public string objQty { get; set; }
        public bool isOnHold { get; set; }

    }
}
