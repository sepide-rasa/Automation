using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automation.Models
{
    public class Letter
    {
        public long fldID { set; get; }
        public string fldSubject { set; get; }
        public string fldLetterNumber { set; get; }
        public string fldLetterDate { set; get; }
        public string fldCreatedDate { set; get; }       
        public string fldKeywords { set; get; }
        public int fldLetterStatusID { set; get; }
        public int fldComisionID { set; get; }
        public int fldImmediacyID { set; get; }
        public int fldSecurityTypeID { set; get; }
        public int fldLetterTypeID { set; get; }
        public string fldDesc { set; get; }
        public int fldUserID { set; get; }
        public long fldExternalLetterReceiverfldID { set; get; }
        public long fldExternalLetterReceiverLetterID { set; get; }
        public string fldExternalLetterReceiverExternalPartnerID { set; get; }
        public int fldExternalLetterReceiverUserID{ set; get; }
        public string fldExternalLetterReceiverDesc { set; get; }
        public string fldLetterSenderId { get; set; }
        public long fldSignerID { set; get; }
        public long fldSignerLetterID { set; get; }
        public int fldSignerIndexerID { set; get; }
        public string fldFirstSigner { set; get; }
        public string fldSignerComisionID { set; get; }
        public string fldSignerDesc { set; get; }
        public int fldSignerUserID { set; get; }
        public string fldRooneveshtID { set; get; }
        public string fldRooneveshtName { set; get; }
        public string fldRooneveshtAssTypeID { set; get; }
        public string fldRooneveshtAssDesc { set; get; }
        public byte fldSignType { get; set; }
    }
}
