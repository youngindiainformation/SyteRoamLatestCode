using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp2.ViewModels
{
  public class NotesViewModel
    {
        private string listdata;

        public string Listdata
        {
            get { return listdata; }
            set { listdata = value; }
        }
        private string subnotes;

        public string Subnotes
        {
            get { return subnotes; }
            set { subnotes = value; }
        }

        public NotesViewModel()
        {
           
        }
    }
}
