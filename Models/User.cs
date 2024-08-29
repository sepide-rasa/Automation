using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class User
    {
        public int fldID { set; get; }
        public int fldStaffID { set; get; }
        public string fldUserName { set; get; }
        public string fldPassword { set; get; }
        public bool fldActive_Deactive { set; get; }
        public string fldDesc { set; get; }
    }
}