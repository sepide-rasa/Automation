using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class tblLetterModel
    {
        public long fldId { get; set; }
        public long fldOrderId { get; set; }
        public string fldLetterDate { get; set; }
        public string fldCreatedDate { get; set; }
        public string fldLetterNumber { get; set; }
        public string fldSubject { get; set; }
        public string fldKeywords { get; set; }
        public byte fldLetterTypeID { get; set; }
        public string fldSecurityType { get; set; }
        public string fldImmediacyName { get; set; }
        public string fldSenderName { get; set; }
        public string Desc { get; set; }
        public string ReciverName { get; set; }
        public int ComisionId { get; set; }
        public Int64 fldAssignmentID { get; set; }
    } 
}