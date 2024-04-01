using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class AppointmentList
    {

        [PrimaryKey]
        [AutoIncrement]
        public int AppointmentDetailsID { get; set; }
        public string Partner_id { get; set; }
        public string Description { get; set; }
        public string Event_date { get; set; }
        public string Hrs { get; set; }
        public string Inc_num { get; set; }
        public string Ref_line_suf { get; set; }
        public string Ref_num { get; set; }
        public string Ref_release { get; set; }
        public string Ref_type { get; set; }
        public string Sched_date { get; set; }
        public string Seq { get; set; }
        public string Start_time { get; set; }
        public string Day_seq { get; set; }
        public string task_seq { get; set; }
        public string Sub_id { get; set; }
        public string Appt_type { get; set; }
        public string Do_expand { get; set; }
        public string Status { get; set; }
        public string Check_conflict { get; set; }
        public string Awaiting_parts { get; set; }
        public string Complete { get; set; }
        public string SiteRef  { get; set; }
        public string notes_subject { get; set; }
        public string Notes { get; set; }
        public string Charfld1 { get; set; }
        public string Charfld2 { get; set; }
        public string Charfld3 { get; set; }
        public string Datefld { get; set; }
        public string Decifld1 { get; set; }
        public string Decifld2 { get; set; }
        public string Decifld3 { get; set; }
        public string Logifld { get; set; }
        public string NoteExistsFlag { get; set; }
        public string InWorkflow { get; set; }
        //Details
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; }
        public string Name { get; set; }
        public string Cust_num { get; set; }
        public string Cust_seq { get; set; }
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string c { get; set; }
        public string Cont_stat { get; set; }//Working Status
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string ContDescription { get; set; }
        public string Phone { get; set; }
        public string cust_Name { get; set; }
    }
}
