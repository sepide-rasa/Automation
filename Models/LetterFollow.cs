using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class LetterFollow
    {
        public long fldLetterID { set; get; }
        public long fldLetterBoxID { set; get; }
        public int fldComisionID { get; set; }
        public int fldID { get; set; }
        public string fldLetterText { set; get; }
        public string fldDesc { set; get; }
    }
}