using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Email
    {
        public int fldID { get; set; }
        public int fldStaffID { get; set; }
        public bool fldSendTrue_False { get; set; }
        public string fldEmailAddress { get; set; }
        public string fldDesc { get; set; }
    }
}