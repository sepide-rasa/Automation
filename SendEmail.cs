using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace Automation
{
    public static class SendEmail
    {
        public static void send(string StaffID,long LetterID)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblProgramSettingSelect("", "", 0,0,"").FirstOrDefault();
            string serverName = q.fldSendServer;
            string username = q.fldEmailAddress.ToString();
            string password = q.fldEmailPassword.ToString();
            int port = q.fldSendPort;

            var S = p.sp_tblStaffSelect("fldId", StaffID.ToString(), 0, 0, "").FirstOrDefault();
            var E = p.sp_tblLetterSelect("fldId", LetterID.ToString(),0,0,"").FirstOrDefault();
            var Content = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();

            foreach (var Item in Content)
            {
               
            }

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(serverName);

            mail.From = new MailAddress(username);
            mail.To.Add(S.fldEmailAddress);
            mail.Subject = E.fldSubject;
            mail.Body = "نامه در پیوست می باشد.";

            //var Att = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
            //foreach(var item in Att)
            //mail.Attachments=item.fldLetterText

            SmtpServer.Port = port;
            SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
            return;
        }
    }
}