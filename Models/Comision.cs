using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Comision
    {
        public int fldID { set; get; }
        public int fldStaffID { set; get; }
        public int fldOrganicRoleID { set; get; }
        public string fldStartDate { set; get; }
        public string fldEndDate { set; get; }
        public string fldOraganicNumber { set; get; }
        public string fldDesc { set; get; }
        public int fldUserID { set; get; }
    }
}