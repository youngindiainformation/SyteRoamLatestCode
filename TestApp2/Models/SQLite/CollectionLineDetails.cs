using System;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class CollectionLineDetailsTable
    {
        [PrimaryKey]
        [AutoIncrement]
        public int DBContractDataID { get; set; }
        public string Contract { get; set; }
        public string Site_ref { get; set; }
        public string Cont_line { get; set; }
        public string Billed_thru_date { get; set; }
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string cont_basis { get; set; }
        public string cont_basis_conv { get; set; }
        public string cust_po { get; set; }
        public string description { get; set; }
        public string end_date { get; set; }
        public string item { get; set; }
        public string paid_thru { get; set; }
        public string qty { get; set; }
        public string qty_conv { get; set; }
        public string rate { get; set; }
        public string rate_conv { get; set; }
        public string ser_num { get; set; }
        public string start_date { get; set; }
        public string tax_code1 { get; set; }
        public string tax_code2 { get; set; }
        public string u_m { get; set; }
        public string unit_of_rate { get; set; }
        public string start_meter_amt { get; set; }
        public string end_meter_amt { get; set; }
        public string billed_thru_meter_amt { get; set; }
        public string current_meter_amt { get; set; }
        public string meter_rate { get; set; }
        public string due_date { get; set; }
        public string total_billed { get; set; }
        public string incl_waiver_charge { get; set; }
        public string cont_price_basis { get; set; }
        public string total_surcharge { get; set; }
        public string meter_date { get; set; }
        public string min_bill_thru { get; set; }
        public string meter_allow { get; set; }
        public string prorate_rate { get; set; }
        public string prorate_unit_of_rate { get; set; }
        public string prorate_rate_conv { get; set; }
        public string location { get; set; }
        public string loc_num { get; set; }
        public string loc_seq { get; set; }
        public string ref_type { get; set; }
        public string ref_num { get; set; }
        public string ref_line_suf { get; set; }
        public string ref_release { get; set; }
        public string charfld1 { get; set; }
        public string charfld2 { get; set; }
        public string charfld3 { get; set; }
        public string datefld { get; set; }
        public string decifld1 { get; set; }
        public string decifld2 { get; set; }
        public string decifld3 { get; set; }
        public string logifld { get; set; }
        public string noteExistsFlag { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
        public string createDate { get; set; }
        public string inWorkflow { get; set; }
        public string siteRef { get; set; }
        public string billingFreq { get; set; }
        public string lineStatus { get; set; }
        public string checkInDate { get; set; }
        public string checkOutDate { get; set; }
        public string Status { get; set; }
        public string statusCode { get; set; }
        public string UnitOfRate { get; set; }
    }
}
