using System;
using SQLite;
using Xamarin.Forms;

namespace TestApp2.Models.SQLite
{
    public class YardPickingLineDetails
    {
        [PrimaryKey]
        [AutoIncrement]
        public int YardPickUpLineDetails{ get; set; }
        public string Billingfreq { get; set; }
        public DateTime? checkInDate { get; set; }
        public DateTime? checkOutDate { get; set; }
        public string CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string description { get; set; }
        public DateTime? end_date { get; set; }
        public string item { get; set; }
        public string lineStatus { get; set; }
        public string prorate_rate_conv { get; set; }
        public string prorate_unit_of_rate { get; set; }
        public string qty { get; set; }
        public string rate_conv { get; set; }
        public string RecordDate { get; set; }
        public string ser_num { get; set; }
        public DateTime? start_date { get; set; }
        public string UnitOfRate { get; set; }
        public string UpdatedBy { get; set; }
        public string contract { get; set; }
        public string cont_line { get; set; }
        public string site_ref { get; set; }
        public string _ItemId { get; set; }
        public string u_m { get; set; }
        public string unit_of_rate { get; set; }


    }
}

