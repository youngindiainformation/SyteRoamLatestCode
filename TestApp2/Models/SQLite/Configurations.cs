using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Common.SQLite
{
    public class Configurations
    {
        //public Configurations(string con)
        //{
        //    this.config = con;
        //}
        //public Configurations()
        //{

        //}
        [PrimaryKey]
        [AutoIncrement]
        public int ConfigId { get; set; }
        public string config { get; set; }
        public string webUrl { get; set; }
        public string configGroup { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }
}
