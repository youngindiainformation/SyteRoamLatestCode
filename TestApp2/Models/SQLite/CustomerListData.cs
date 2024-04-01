using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Models.SQLite
{
   public class CustomerListData
    {
        [PrimaryKey]
        [AutoIncrement]
        public int CustomerID { get; set; }
        public string Site_ref { get; set; }
        public string Cust_num { get; set; }
        public string Cust_seq { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string address { get; set; }
        public string Cus_OrderName { get; set; }
        public string Cus_OrderContact { get; set; }
        public string Cus_OrderEmail { get; set; }
        public string Cus_ShipToName { get; set; }
        public string Cus_ShipToContact { get; set; }
        public string Cus_ShipToEmail { get; set; }
        public string Cus_BillingToName { get; set; }
        public string Cus_BillingToContact { get; set; }
        public string Cus_BillingToEmail { get; set; }
        public string CustomerName { get; set; }
        public string Cus_ID { get; set; }
        public string Cus_ShipToID { get; set; }


    }
}
