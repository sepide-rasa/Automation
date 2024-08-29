using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Signer
    {
        public long fldID { set; get; }
        public string fldSignerComisionID { set; get; }
        public long fldLetterID { set; get; }
        public string fldDesc { set; get; }
        public int CreatorComId { set; get; }
    }
}