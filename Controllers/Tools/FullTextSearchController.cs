using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using Automation.Controllers.Users;

namespace Automation.Controllers.Tools
{
    public class FullTextSearchController : Controller
    {
        //
        // GET: /FullTextSearch/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 111))
            {
            return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Search(string Text)
        {
             string sqlString = "SELECT    tblLetter.fldID, tblLetter.fldYear, tblLetter.fldOrderId, tblLetter.fldSubject, "+
               "  tblLetter.fldLetterNumber ,d.fldTarikh as fldLetterDate,d2.fldTarikh as fldCreatedDate, tblLetter.fldKeywords, tblLetter.fldLetterTypeID "+
                " FROM tblAssignment INNER JOIN tblContentFile "+
                " INNER JOIN tblLetter ON tblContentFile.fldLetterID = tblLetter.fldID ON tblAssignment.fldLetterID = tblLetter.fldID "+
                " INNER JOIN tblInternalAssignmentReceiver ON tblAssignment.fldID = tblInternalAssignmentReceiver.fldAssignmentID" +
                " left join tblDateDim as d on d.fldDate=cast(tblLetter.fldLetterDate as date) "+
				" inner join tblDateDim as d2 on d2.fldDate=cast(tblLetter.fldCreatedDate as date) "+
				" cross apply (SELECT top 1 c.fldid FROM dbo.tblCommision as c "+
				"				inner join dbo.tblUser as u on c.fldStaffID=u.fldStaffID "+
                "				where c.fldID=tblInternalAssignmentReceiver.fldReceiverComisionID  and u.fldID=" + Session["UserId"].ToString() + ")c " +
               " where  freetext(fldLetterText,N'" + Text + "') "+
               "  order by fldCreatedDate desc";
            //string sqlString = "SELECT     tblLetter.fldID, tblLetter.fldYear, tblLetter.fldOrderId, tblLetter.fldSubject," +
            //    " tblLetter.fldLetterNumber,dbo.miladitoshamsi(tblLetter.fldLetterDate)as fldLetterDate, dbo.miladitoshamsi(tblLetter.fldCreatedDate)as fldCreatedDate, tblLetter.fldKeywords," +
            //    " tblLetter.fldLetterTypeID FROM tblAssignment INNER JOIN tblContentFile INNER JOIN "+
            //    "tblLetter ON tblContentFile.fldLetterID = tblLetter.fldID ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN "+
            //    "tblInternalAssignmentReceiver ON tblAssignment.fldID = tblInternalAssignmentReceiver.fldAssignmentID where tblInternalAssignmentReceiver.fldReceiverComisionID "+
            //    "in(SELECT fldid FROM dbo.tblCommision WHERE fldStaffID =(SELECT fldStaffID FROM dbo.tblUser WHERE fldid=" + Session["UserId"].ToString() + "))" +
            //    " and CONTAINS(fldLetterText,N'\"" + Text + "\"')";

            SqlConnection con = new SqlConnection();
            con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AutomationConnectionString"].ConnectionString;
            SqlCommand com = new SqlCommand(sqlString, con);
            List<Models.sp_tblLetterSelect> letter = new List<Models.sp_tblLetterSelect>();

            SqlDataAdapter adap = new SqlDataAdapter(com);
            DataSet dt = new DataSet();
            adap.Fill(dt);
            DataTable dataTableTmp = dt.Tables[0];
            EnumerableRowCollection<DataRow> k = dataTableTmp.AsEnumerable();

            var kk = k.ToList();
            List<Models.sp_tblLetterSelect> p = new List<Models.sp_tblLetterSelect>();
            foreach (var item in kk)
            {
                var m = item.ItemArray;
                Models.sp_tblLetterSelect l = new Models.sp_tblLetterSelect();
                string keyWord = "", LetterNumber = "", LetterDate = "";
                if (m[7] != null)
                    keyWord = m[7].ToString();
                if (m[4] != null)
                    LetterNumber = m[4].ToString();
                if (m[5] != null)
                    LetterDate = m[5].ToString();
                l.fldID = (long)m[0];
                l.fldYear = (int)m[1];
                l.fldOrderId = (long)m[2];
                l.fldSubject = (string)m[3];                
                l.fldLetterNumber = LetterNumber;
                l.fldLetterDate = LetterDate;
                l.fldCreatedDate = (string)m[6];
                l.fldLetterTypeID = (int)m[8];
                l.fldKeywords = keyWord;
                
                p.Add(l);
            }
            return Json(p.ToList(), JsonRequestBehavior.AllowGet);
        }

    }
}
