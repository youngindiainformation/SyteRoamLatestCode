using System;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class CollectionLineData
    {
        [PrimaryKey]
        [AutoIncrement]
        public int collID { get; set; }
        public int CollectionID { get; set; }
        public string Contract { get; set; }
        public string Site_ref { get; set; }
        public string Cont_line { get; set; }
        public string Billed_thru_date { get; set; }
        public string Billing_freq { get; set; }
        public string Billing_type { get; set; }
        public string Cont_basis { get; set; }
        public string Cont_basis_conv { get; set; }
        public string Cust_po { get; set; }
        public string Description { get; set; }
        public string End_date { get; set; }
        public string Item { get; set; }
        public string Paid_thru { get; set; }
        public string Qty { get; set; }
        public string Qty_conv { get; set; }
        public string Rate { get; set; }
        public string Rate_conv { get; set; }
        public string Ser_num { get; set; }
        public string Start_date { get; set; }
        public string Tax_code1 { get; set; }
        public string Tax_code2 { get; set; }
        public string U_m { get; set; }
        public string Unit_of_rate { get; set; }
        public string Start_meter_amt { get; set; }
        public string End_meter_amt { get; set; }
        public string Billed_thru_meter_amt { get; set; }
        public string Current_meter_amt { get; set; }
        public string Meter_rate { get; set; }
        public string Due_date { get; set; }
        public string Total_billed { get; set; }
        public string Incl_waiver_charge { get; set; }
        public string Cont_price_basis { get; set; }
        public string Total_surcharge { get; set; }
        public string Meter_date { get; set; }
        public string Min_bill_thru { get; set; }
        public string Meter_allow { get; set; }
        public string Prorate_rate { get; set; }
        public string Prorate_unit_of_rate { get; set; }
        public string Prorate_rate_conv { get; set; }
        public string Location { get; set; }
        public string Loc_num { get; set; }
        public string Loc_seq { get; set; }
        public string Ref_type { get; set; }
        public string Ref_num { get; set; }
        public string Ref_line_suf { get; set; }
        public string Ref_release { get; set; }
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
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string LineStatus { get; set; }

    }
}
