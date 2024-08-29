using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Premissions
    {
        public int fldID { set; get; }
        public int fldUserGroupID { set; get; }
        public int fldApplicationPartID { set; get; }
        public string fldDesc { set; get; }
        public int fldUserID { set; get; }
    }
}