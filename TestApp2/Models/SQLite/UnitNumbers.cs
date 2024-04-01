using System;
using SQLite;
using Xamarin.Forms;

namespace TestApp2.Models.SQLite
{
    public class UnitNumbers 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int UnitId{ get; set; }
        public string UnitsList { get; set; }
        public string ContractID { get; set; }
    }
}

