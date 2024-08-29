using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Substitue
    {
        public int fldID { set; get; }
        public int fldSenderComisionID { set; get; }
        public int fldReceiverComisionID { set; get; }
        public string fldStartDate { set; get; }
        public string fldEndDate { set; get; }
        public TimeSpan fldStartTime { set; get; }
        public TimeSpan fldEndTime { set; get; }
        public bool fldIsSigner { set; get; }
        public bool fldShowReceiverName { set; get; }
        public string fldDesc { set; get; }
        public int fldUserID { set; get; }
    }
}