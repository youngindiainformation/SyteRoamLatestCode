using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Common.APIclasses
{
   public class AppointmentsList
    {

        public string appt_stat { get; set; }
        public string appt_type { get; set; }
        public string Complete { get; set; }
        public string hrs { get; set; }
        public string ref_num { get; set; }
        public string sched_date { get; set; }
        public string description { get; set; }
        //details page objects
        public DateTime? Start_date { get; set; }
        public DateTime? End_date { get; set; }
        public string name { get; set; }
        public string Cust_num { get; set; }
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cont_stat { get; set; }//Working Status
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string Notes { get; set; }
        public string ContDescription { get; set; }
        public string notes_subject { get; set; }
        public string Customer_Name { get; set; }
        public string cust_seq { get; set; }
        public string task_seq { get; set; }

    }
}
