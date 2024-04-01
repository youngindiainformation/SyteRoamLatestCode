using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Models.SQLite
{
    public class PostAppointmentTemp
    {
        [PrimaryKey]
        [AutoIncrement]
        public int PostAppointmentTempID { get; set; }
        public string Site { get; set; }
        public string ContractId { get; set; }
        public string PartnerId { get; set; }
        public string Cswitch { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public string RefNum { get; set; }
        public string AppDate { get; set; }
        public string AppStatus { get; set; }
        public string ApptType { get; set; }
        public string TaskSeqNumber { get; set; }
    }
}
