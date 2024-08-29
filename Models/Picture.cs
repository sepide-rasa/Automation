using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Picture
    {
        public int fldID { set; get; }
        public string fldDesc { set; get; }
        public string fldStaffPicture { set; get; }
        public string fldSignPicture { set; get; }
        public int fldStaffID { set; get; }
    }
}