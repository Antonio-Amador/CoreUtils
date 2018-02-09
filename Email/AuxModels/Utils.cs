using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Email.AuxModels
{
    public class UtilsModels
    {

    }

    public class EmailModel
    {
        [NotMapped]
        public byte[] attachByteArray { get; set; } //[byte[]] NULL,
        [NotMapped]
        public string emailType { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string documentName { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string emailBody { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string emailSubject { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string emailAddressFrom { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string emailAddressTo { get; set; } //[String] NOT NULL,
        [NotMapped]
        public string smtpServer { get; set; } //[String] NOT NULL,
    }
}
