﻿using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class ContractDetailsAPI
    {
        [PrimaryKey]
        [AutoIncrement]
        public int DBContractID { get; set; }
        public string Description { get; set; }
        public string appt_type { get; set; }
        public string Status { get; set; }//success
        public string StatusCode { get; set; }
        public string Date { get; set; }
        public string Contract { get; set; }
        public string Site_ref { get; set; }
        public string Amount { get; set; }
        public string Billed_from { get; set; }
        public string Billed_thru_date { get; set; }
        public string BillingFreq { get; set; }
        public string BillingType { get; set; }
        public string Cust_num { get; set; }
        public string Cust_po { get; set; }
        public string Cust_seq { get; set; }
        public string End_date { get; set; }
        public string End_user_type { get; set; }
        public string Exch_rate { get; set; }
        public string Fixed_rate { get; set; }
        public string Last_process_date { get; set; }
        public string Paid_thru { get; set; }
        public string Product_code { get; set; }
        public string Prorate { get; set; }
        public string Renewal_mo_day { get; set; }
        public string Serv_type { get; set; }
        public string Slsman { get; set; }
        public string Start_date { get; set; }
        public string Tax_code1 { get; set; }
        public string Tax_code2 { get; set; }
        public string Terms_code { get; set; }
        public string Waiver_charge { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Addr3 { get; set; }
        public string Addr4 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Fax_num { get; set; }
        public string Username { get; set; }//Lead Partner
        public string Total_billed { get; set; }
        public string Cont_stat { get; set; }//Working Status
        public string Cont_allow_custaddr_override { get; set; }
        public string Name { get; set; }
        public string Total_waiver_charge { get; set; }
        public string Total_surcharge { get; set; }
        public string Total_tax1 { get; set; }
        public string Total_tax2 { get; set; }
        public string Drivers_license { get; set; }
        public string Ssn { get; set; }
        public string Whse { get; set; }
        public string Phone2 { get; set; }
        public string Prior_code { get; set; }
        public string Time_zone { get; set; }
        public string Override_bus_hrs { get; set; }
        public string Charfld1 { get; set; }
        public string Charfld2 { get; set; }
        public string Charfld3 { get; set; }
        public string Datefld { get; set; }
        public string Decifld1 { get; set; }
        public string Decifld2 { get; set; }
        public string Decifld3 { get; set; }
        public string Logifld { get; set; }
        public string NoteExistsFlag { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string CreateDate { get; set; }
        public string InWorkflow { get; set; }
        public string notes_subject { get; set; }

    }
}
