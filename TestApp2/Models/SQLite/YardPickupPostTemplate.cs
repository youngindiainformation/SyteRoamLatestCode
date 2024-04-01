using System;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class YardPickupPostTemplate
    {
        [PrimaryKey]
        [AutoIncrement]
        public int PostYardPickupTempID { get; set; }
        public string SiteId { get; set; }
        public string SelectedStatus { get; set; }
        public string contractID { get; set; }
        public string LineNumber { get; set; }
        public string Signature { get; set; }
        public string SignedTime { get; set; }
        public string objQty { get; set; }
        public string Flag { get; set; }
    }
}
