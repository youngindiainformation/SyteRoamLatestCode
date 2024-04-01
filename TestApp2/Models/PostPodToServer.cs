using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.Models
{
    public class PostPodToServer
    {
        public PostPodToServer()
        {
            Changes = new Changes();
        }
        public Changes Changes { get; set; }
    }

    public class Changes
    {
        public Changes()
        {
            Properties = new List<Properties>();
        }
        public string Action { get; set; }
        public string ItemId { get; set; }
        public List<Properties> Properties { get; set; }
        public string UpdateLocking { get; set; }
        

    }
    public class Properties
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string OriginalValue { get; set; }
        public string Modified { get; set; }
        public string IsNull { get; set; }
        public string IsNestedCollection { get; set; }
            
          
    }
}
