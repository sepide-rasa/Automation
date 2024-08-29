using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using Automation.Models;
using System.Collections;

namespace Automation.Controllers
{
    public class Search_Letter2Controller : Controller
    {
        //
        // GET: /Search_Letter2/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            return PartialView();
        }
        string Where(LetterSearch LetterSearch)
        {
            ArrayList ar = new ArrayList();
            string s = "";
            if (LetterSearch.OrderId != "" && LetterSearch.OrderId != null)
            {
                ar.Add("(tblLetter.fldOrderId like N'%" + LetterSearch.OrderId + "%')");
            }
            if (LetterSearch.Subject != "" && LetterSearch.Subject != null)
            {
                ar.Add("(tblLetter.fldSubject like N'%" + LetterSearch.Subject + "%')");
            }
            if (LetterSearch.LetterNumber != "" && LetterSearch.LetterNumber != null)
            {
                ar.Add("(tblLetter.fldLetterNumber like N'%" + LetterSearch.LetterNumber + "%')");
            }
            if (LetterSearch.LetterDate != "" && LetterSearch.LetterDate != null)
            {
                ar.Add("(tblLetter.fldLetterDate like N'%" + LetterSearch.LetterDate + "%')");
            }
            if (LetterSearch.SecurityType != "" && LetterSearch.SecurityType != null)
            {
                ar.Add("(tblSecurityType.fldType like N'%" + LetterSearch.SecurityType + "%')");
            }

            if (LetterSearch.Keywords != "" && LetterSearch.Keywords != null)
            {
                ar.Add("(tblLetter.fldKeywords like N'%" + LetterSearch.Keywords + "%')");
            }
            if (LetterSearch.ImmediacyName != "" && LetterSearch.ImmediacyName != null)
            {
                ar.Add("(tblImmediacy.fldName like N'%" + LetterSearch.ImmediacyName + "%')");
            }
            if (LetterSearch.SenderName != "" && LetterSearch.SenderName != null)
            {
                ar.Add("(dbo.GetLetterSender(tblLetter.fldID) like N'%" + LetterSearch.SenderName + "%')");
            }
            if (LetterSearch.ReciverName != "" && LetterSearch.ReciverName != null)
            {
                ar.Add("(dbo.GetLetterReciever(tblLetter.fldID) like N'%" + LetterSearch.ReciverName + "%')");
            }
            if (LetterSearch.StartCreatedDate != "" && LetterSearch.EndCreatedDate != "" && LetterSearch.StartCreatedDate != null && LetterSearch.EndCreatedDate != null)
            {
                ar.Add("(dbo.MiladiTOShamsi(tblLetter.fldCreatedDate)>='" + LetterSearch.StartCreatedDate + "') and (dbo.MiladiTOShamsi(tblLetter.fldCreatedDate)<='" + LetterSearch.EndCreatedDate + "')");
            }
            if (ar.Count > 1)
            {
                int i;
                for (i = 0; i < ar.Count - 1; i++)
                {
                    s = s + ar[i].ToString() + " and ";
                }
                s = " and " + s + ar[i].ToString();
            }
            else if (ar.Count == 1)
                s = " and " + ar[0].ToString();
            return s;

        }

        public ActionResult Search(LetterSearch LetterSearch)
        {
            string sqlString = "select * from(SELECT     tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate, tblLetter.fldKeywords, tblLetter.fldLetterTypeID, " +
                      "tblSecurityType.fldType AS fldSecurityType, tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName,tblLetter.fldDesc,dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers" +
                        " FROM         tblInternalAssignmentReceiver INNER JOIN " +
                      "tblAssignment ON tblInternalAssignmentReceiver.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                      "tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN " +
                      "tblCommision ON tblInternalAssignmentReceiver.fldReceiverComisionID = tblCommision.fldID INNER JOIN " +
                      "tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN " +
                      "tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                      "tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                      "tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID where tblInternalAssignmentReceiver.fldReceiverComisionID " +
                      "in(SELECT fldid FROM dbo.tblCommision WHERE fldStaffID =(SELECT fldStaffID FROM dbo.tblUser WHERE fldid=" + Session["UserId"].ToString() + ")) " + Where(LetterSearch)
                      + " union SELECT     tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject," +
                      " tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate," +
                      " tblLetter.fldKeywords, tblLetter.fldLetterTypeID, tblSecurityType.fldType AS fldSecurityType," +
                      " tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName," +
                      " tblLetter.fldDesc, dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers FROM " +
                      "tblInternalAssignmentSender INNER JOIN tblAssignment ON tblInternalAssignmentSender.fldAssignmentID" +
                      " = tblAssignment.fldID INNER JOIN tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER " +
                      "JOIN tblCommision ON dbo.tblInternalAssignmentSender.fldsenderComisionID = tblCommision.fldID INNER " +
                      "JOIN tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN tblImmediacy ON tblLetter.fldImmediacyID " +
                      "= tblImmediacy.fldID INNER JOIN tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID" +
                      " INNER JOIN tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID WHERE     " +
                      "(tblInternalAssignmentSender.fldSenderComisionID IN (SELECT     fldid FROM          tblCommision WHERE     " +
                      " (fldStaffID = (SELECT     fldStaffID FROM          tblUser WHERE      (fldid = " + Session["UserId"].ToString() + "))))) " + Where(LetterSearch) +
            ")t order by tblLetter.fldLetterDate desc";
            SqlConnection con = new SqlConnection();
            con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AutomationConnectionString"].ConnectionString;
            SqlCommand com = new SqlCommand(sqlString, con);
            List<tblLetterModel> letter = new List<tblLetterModel>();

            SqlDataAdapter adap = new SqlDataAdapter(com);
            DataSet dt = new DataSet();
            adap.Fill(dt);
            DataTable dataTableTmp = dt.Tables[0];
            EnumerableRowCollection<DataRow> k = dataTableTmp.AsEnumerable();

            var kk = k.ToList();
            List<tblLetterModel> p = new List<tblLetterModel>();
            foreach (var item in kk)
            {
                var m = item.ItemArray;
                tblLetterModel l = new tblLetterModel();
                string keyWord = "", LetterNumber = "", LetterDate = "";
                if (m[5] != null)
                    keyWord = m[5].ToString();
                if (m[3] != null)
                    LetterNumber = m[3].ToString();
                if (m[4] != null)
                    LetterDate = m[4].ToString();
                l.fldId = (long)m[0];
                l.fldOrderId = (long)m[1];
                l.fldSubject = (string)m[2];
                l.fldLetterNumber = LetterNumber;
                l.fldLetterDate = LetterDate;
                l.fldKeywords = keyWord;
                l.fldSecurityType = (string)m[7];
                l.fldImmediacyName = (string)m[8];
                l.fldSenderName = (string)m[9];
                l.Desc = (string)m[10];
                l.ReciverName = (string)m[11];
                p.Add(l);
            }
            return Json(p.ToList(), JsonRequestBehavior.AllowGet);
        }

    }
}
