using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using Automation.Models;
using System.Collections;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class SearchLetterBayganiController : Controller
    {
        //
        // GET: /SearchLetterBaygani/

        public ActionResult Index()
        {
             if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 94))
            {
            return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult GetSecurityTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblImmediacySelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetLetterTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblLetterTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult Reload(List<Models.tblLetterModel> SearchModel)
        //{
        //    Models.AutomationEntities p = new Models.AutomationEntities();

        //    if (SearchModel != null)
        //    {
        //        var q = p.sp_SelectLettersInCartabl(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["Year"])).AsQueryable();
        //        foreach (var item in SearchModel)
        //        {
        //            switch (item.ParamName)
        //            {
        //                case "Number":
        //                    q = q.Where(h => h.fldOrderId == Convert.ToInt32(item.Value));
        //                    break;
        //                case "BetweenDate":
        //                    q = q.Where(h => h.fldCreatedDateEn >= MyLib.Shamsi.Shamsi2miladiDateTime(item.Value) && h.fldCreatedDateEn <= MyLib.Shamsi.Shamsi2miladiDateTime(item.Value));
        //                    break;
        //            }
        //        }
        //        return Json(q.ToList());
        //    }
        //    return Json("");
        //}

        string Where(Models.tblLetterModel SearchModel)
        {
            ArrayList ar = new ArrayList();
            string s = "";
            if (SearchModel.fldOrderId != 0 && SearchModel.fldOrderId != null)
            {
                ar.Add("(tblLetter.fldOrderId like N'%" + SearchModel.fldOrderId + "%')");
            }
            if (SearchModel.fldSubject != "" && SearchModel.fldSubject != null)
            {
                ar.Add("(tblLetter.fldSubject like N'%" + SearchModel.fldSubject + "%')");
            }
            if (SearchModel.fldLetterNumber != "" && SearchModel.fldLetterNumber != null)
            {
                ar.Add("(tblLetter.fldLetterNumber like N'%" + SearchModel.fldLetterNumber + "%')");
            }
            if (SearchModel.fldLetterDate != "" && SearchModel.fldLetterDate != null)
            {
                ar.Add("(tblLetter.fldLetterDate like N'%" + SearchModel.fldLetterDate + "%')");
            }
            if (SearchModel.fldLetterTypeID != 0 && SearchModel.fldLetterTypeID != null)
            {
                ar.Add("(tblLetter.fldLetterTypeID like N'%" + SearchModel.fldLetterTypeID + "%')");
            }

            if (SearchModel.fldKeywords != "" && SearchModel.fldKeywords != null)
            {
                ar.Add("(tblLetter.fldKeywords like N'%" + SearchModel.fldKeywords + "%')");
            }
            if (SearchModel.fldSecurityType != "" && SearchModel.fldSecurityType != null)
            {
                ar.Add("(tblLetter.fldSecurityTypeID like N'%" + SearchModel.fldSecurityType + "%')");
            }
            if (SearchModel.fldSenderName != "" && SearchModel.fldSenderName != null)
            {
                ar.Add("(dbo.GetLetterSender(tblLetter.fldID) like N'%" + SearchModel.fldSenderName + "%') or (dbo.GetLetterReciever(tblLetter.fldID) like N'%" + SearchModel.ReciverName + "%')");
            }
            //if (SearchModel.ReciverName != "" && SearchModel.ReciverName != null)
            //{
            //    ar.Add("(dbo.GetLetterReciever(tblLetter.fldID) like N'%" + SearchModel.ReciverName + "%')");
            //}
            if (SearchModel.fldCreatedDate != "" && SearchModel.fldCreatedDate != null)
            {
                var d=SearchModel.fldCreatedDate.Split('|');
                ar.Add("(dbo.MiladiTOShamsi(tblLetter.fldCreatedDate)>='" + d[0] + "') and (dbo.MiladiTOShamsi(tblLetter.fldCreatedDate)<='" +d[1] + "')");
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

        public ActionResult Search(Models.tblLetterModel SearchModel)
        {
            string sqlString = "declare @StaffId int " +
                      "declare @Comision table(fldComId int) " +
                      "set @StaffId=(select fldStaffID from tblUser where fldId=" + Session["UserId"].ToString() + ")" +
                      "insert into @Comision " +
                      "select fldID from tblCommision where fldStaffID=@StaffId " +

                      "SELECT     tblLetter.fldID, tblLetter.fldYear, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber, tblLetter.fldLetterDate, dbo.MiladiTOShamsi(tblLetter.fldCreatedDate) " +
                      "AS fldCreatedDate, tblLetter.fldCreatedDate AS fldCreatedDateEn, tblLetter.fldKeywords, tblLetter.fldLetterStatusID, tblLetter.fldComisionID, tblLetter.fldImmediacyID," +
                      "tblLetter.fldSecurityTypeID, tblLetter.fldLetterTypeID, tblLetter.fldDate, tblLetter.fldUserID, tblLetter.fldDesc, tblInternalAssignmentReceiver.fldReceiverComisionID, " +
                      "tblImmediacy.fldName, tblSecurityType.fldType, dbo.GetLetterSender(tblLetter.fldID) AS LetterSender, dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers, " +
                      "tblLetterType.fldType AS fldLetterType " +
                      "FROM         tblInternalAssignmentReceiver INNER JOIN " +
                      "tblAssignment ON tblInternalAssignmentReceiver.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                      "tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN " +
                      "tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                      "tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                      "tblLetterType ON tblLetter.fldLetterTypeID = tblLetterType.fldID " +
                      "where tblInternalAssignmentReceiver.fldReceiverComisionID in (select * from @Comision) and tblLetter.fldYear=" + Session["Year"].ToString() + Where(SearchModel)
                      +"union all SELECT     tblLetter.fldID, tblLetter.fldYear, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber, tblLetter.fldLetterDate, dbo.MiladiTOShamsi(tblLetter.fldCreatedDate) " +
                      "AS fldCreatedDate, tblLetter.fldCreatedDate AS fldCreatedDateEn, tblLetter.fldKeywords, tblLetter.fldLetterStatusID, tblLetter.fldComisionID, tblLetter.fldImmediacyID," +
                      "tblLetter.fldSecurityTypeID, tblLetter.fldLetterTypeID, tblLetter.fldDate, tblLetter.fldUserID, tblLetter.fldDesc, tblInternalAssignmentSender.fldSenderComisionID, " +
                      "tblImmediacy.fldName, tblSecurityType.fldType, dbo.GetLetterSender(tblLetter.fldID) AS LetterSender, dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers, " +
                      "tblLetterType.fldType AS fldLetterType " +
                      "FROM         tblInternalAssignmentSender INNER JOIN " +
                      "tblAssignment ON tblInternalAssignmentSender.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                      "tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN " +
                      "tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                      "tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                      "tblLetterType ON tblLetter.fldLetterTypeID = tblLetterType.fldID " +
                      "where tblInternalAssignmentSender.fldSenderComisionID in (select * from @Comision) and tblLetter.fldYear=" + Session["Year"].ToString() + Where(SearchModel);


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
            List<sp_SelectLettersInCartabl> p = new List<sp_SelectLettersInCartabl>();
            foreach (var item in kk)
            {
                var m = item.ItemArray;
                sp_SelectLettersInCartabl l = new sp_SelectLettersInCartabl();
                string keyWord = "", LetterNumber = "";DateTime? LetterDate=null;
                if (m[8] != null)
                    keyWord = m[8].ToString();
                if (m[4] != null)
                    LetterNumber = m[4].ToString();
                if (m[5].ToString() != "")
                    LetterDate = Convert.ToDateTime(m[5]);
                l.fldID = (long)m[0];
                l.fldOrderId = (long)m[2];
                l.fldSubject = (string)m[3];
                l.fldLetterNumber = LetterNumber;
                l.fldLetterDate = LetterDate;
                l.fldKeywords = keyWord;
                l.fldSecurityTypeID = Convert.ToInt32(m[12]);
                l.fldImmediacyID = Convert.ToInt32(m[11]);
                l.LetterSender = (string)m[20];
                l.LetterRecievers = (string)m[21];
                p.Add(l);
            }
            return Json(p.ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}
