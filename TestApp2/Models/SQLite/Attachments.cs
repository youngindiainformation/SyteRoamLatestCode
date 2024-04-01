using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TestApp2.Models.SQLite
{
    public class Attachments
    {
        [PrimaryKey]
        [AutoIncrement]
        public int AttachmentsID { get; set; }
        public string ContractId { get; set; }
        public string DataType { get; set; }
        public string AttachmentTitle { get; set; }
        public byte[] DataInBytes { get; set; }

        

    }
}
