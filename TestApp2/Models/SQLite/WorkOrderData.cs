using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Models.SQLite
{
   public class WorkOrderData
    {
        [PrimaryKey]
        [AutoIncrement]
        public int WorkerOrderID { get; set; }
        public string ContractID { get; set; }
        public string PartnerID { get; set; }
        public string LineNumbers { get; set; }
        public string Signature { get; set; }
        public byte[] PodDocument { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string SubmitDate { get; set; }
        public string CustomerName { get; set; }
        public string Quantity { get; set; }
    }
}
