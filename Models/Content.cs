using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Content
    {
        public long fldLetterID { set; get; }
        public int fldLetterPatternID { get; set; }
        public int fldID { get; set; }
        public string fldName { set; get; }
        public string fldDesc { set; get; }
    }
}