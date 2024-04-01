using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Common.Helper
{
    public class LoginStorage
    {
        public static string Username { get; set; }
        public static string PartnerId { get; set; }
        public static string AccessToken { get; set; }
        public static string Password { get; set; }
        public static string Site { get; set; }
        public static string RefType { get; set; }
        public static string Configurations { get; set; }
        public static int LoginCount { get; set; }
        public static string Site_ref { get; set; }
        public static string CustomerName { get; set; }
        public static bool IsCloudConfig { get; set; }
        public static string CloudBaseURL { get; set; }

    }
}
