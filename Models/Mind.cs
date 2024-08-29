using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Mind
    {
        public Int64 fldID { set; get; }
        public int fldCreatedComisionID { set; get; }
        public String fldMindDate { set; get; }
        public string fldSubject { set; get; }
        public string fldText { set; get; }
        public int fldUserID { set; get; }
        public string fldDesc { set; get; }
    }
}