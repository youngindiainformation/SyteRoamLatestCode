using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Models.SQLite
{
    public class LoginDetails
    {
        [PrimaryKey]
        [AutoIncrement]
        public int LoginDetailsID { get; set; }
        public string Username { get; set; }
        public string UserPassword { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
        public string Site { get; set; }
        public string Configurations { get; set; }
        
    }
}
