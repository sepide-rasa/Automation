using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class LetterAssign
    {
        public string fldDesc { get; set; }
        public string fldName { get; set; }
        public int fldID { get; set; }
        public long fldAssignmentID { set; get; }
        public long fldAssignmentLetterID { set; get; }
        public string fldAssignmentDate { set; get; }
        public string fldAssignmentAnswerDate { set; get; }
        public string fldAssignmentDesc { set; get; }
        public int fldAssignmentUserID { set; get; }
        public string fldInternalAssignmentReciverComisionID { set; get; }
        public string fldInternalAssignmentSenderfldBoxID { set; get; }
        public string fldInternalAssignmentSenderAssignmentAnswerDate { set; get; }
        public string fldInternalAssignmentSenderDesc { set; get; }
        public int fldInternalAssignmentSenderUserID { set; get; }
        public int fldComisionID { get; set; }
        public int fldInternalAssignmentSenderID { get; set; }
        public string fldInternalAssignmentSenderComision { set; get; }
        public DateTime? fldLetterReadDate { get; set; }
        public string fldAssignmentTypeID { get; set; }
        public long fldLetterID { set; get; }
        public long fldLetterBoxID { set; get; }
        public int fldAssignID { set; get; }
    }
}