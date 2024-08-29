using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class ExternalLetter
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
        public string fldExternalLetterReceiverExternalPartnerID { set; get; }
        public string fldSenderID { set; get; }
        public long fldContentID { set; get; }
        public string fldContentName { set; get; }
        public string fldContentLetterPatternID { set; get; }
        public int fldContentUserID { set; get; }
        public string fldRooneveshtID { set; get; }
        public string fldRooneveshtName { set; get; }
        public int AssignmentId { set; get; }
    }
}