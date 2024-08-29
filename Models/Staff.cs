using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Staff
    {
        public int fldId { set; get; }
        public string fldName { set; get; }
        public string fldFamily { set; get; }
        public string fldMelliCode { set; get; }
        public string fldNameFather { set; get; }
        public string fldMobile { set; get; }
        public string fldAddress { set; get; }
        public string fldDesc { set; get; }
        public string fldBirthDate { set; get; }
        public string fldEmailAddress { set; get; }
        public string fldStaffPicture { set; get; }
        public string fldSignPicture { set; get; }
        public bool fldSign { get; set; }
        public bool fldNotify { get; set; }
        public short fldLetterLoadNum { get; set; }
    }
} 