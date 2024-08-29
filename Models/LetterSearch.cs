using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class LetterSearch
    {
        public string OrderId { set; get; }
        public string Subject { set; get; }
        public string LetterNumber { set; get; }
        public string LetterDate { set; get; }
        public string SenderName { set; get; }
        public string StartCreatedDate { set; get; }
        public string EndCreatedDate { set; get; }
        public string SecurityType { set; get; }
        public string ImmediacyName { set; get; }
        public string Keywords { get; set; }
        public string LetterTypeID { get; set; }
        public string Desc { get; set; }
        public string ReciverName { get; set; }
    }
}