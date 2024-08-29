using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class ProgramSetting
    {
        public int fldID { set; get; }   
        public string fldName { set; get; }
        public string fldEmailAddress { set; get; }
        public string fldEmailPassword { set; get; }
        public string fldRecieveServer { set; get; }
        public int fldRecievePort { set; get; }
        public string fldSendServer { set; get; }
        public int fldSendPort { set; get; }
        public string fldLogo { set; get; }
        public string fldDesc { set; get; }
        public bool fldSSL { set; get; }
        public bool fldDelFax { set; get; }
        public bool fldIsSigner { set; get; }
        public string fldFaxPath { set; get; }
    }
}