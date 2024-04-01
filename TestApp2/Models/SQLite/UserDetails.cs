using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Models.SQLite
{
    public class UserDetails
    {
        [PrimaryKey]
        [AutoIncrement]
        public int UserDetailsID { get; set; }
        public string ApptCount { get; set; }
        public string Status { get; set; }
        public string Site { get; set; }
        public string StatusCode { get; set; }
        public string AppointmentCount { get; set; }
        public string Site_ref { get; set; }
        public string PartnerId { get; set; }
        public string Date { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string EditLevel { get; set; }
        public string SuperUserFlag { get; set; }
        public string UserPassword { get; set; }
        public string SqlServerLogin { get; set; }
        public string SqlServerPassword { get; set; }
        public string UserDesc { get; set; }
        public string WorkstationLogin { get; set; }
        public string NoteExistsFlag { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string CreateDate { get; set; }
        public string ConcurrentSessions { get; set; }
        public string ConcurrentSessionsSpec { get; set; }
        public string InWorkflow { get; set; }
        public string PasswordExpirationDate { get; set; }
        public string LoginFailures { get; set; }
    }
}
