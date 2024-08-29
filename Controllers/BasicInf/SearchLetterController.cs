using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using Automation.Models;
using System.Collections;

namespace Automation.Controllers.BasicInf
{
    

    [Authorize]
    public class SearchLetterController : Controller
    {
        //
        // GET: /SearchLetter/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            return PartialView();
        }
       /* string Where(LetterSearch LetterSearch)
        {
            ArrayList ar = new ArrayList();
            string s = "";
            if (LetterSearch.OrderId != "" && LetterSearch.OrderId != null)
            {
                ar.Add("(fldOrderId like N'%" + LetterSearch.OrderId + "%')");
            }
            if (LetterSearch.Subject != "" && LetterSearch.Subject != null)
            {
                ar.Add("(fldSubject like N'%" + LetterSearch.Subject + "%')");
            }
            if (LetterSearch.LetterNumber != "" && LetterSearch.LetterNumber != null)
            {
                ar.Add("(fldLetterNumber like N'%" + LetterSearch.LetterNumber + "%')");
            }
            if (LetterSearch.LetterDate != "" && LetterSearch.LetterDate != null)
            {
                ar.Add("(fldLetterDate  like N'%" + LetterSearch.LetterDate + "%')");
            }
            if (LetterSearch.SecurityType != "" && LetterSearch.SecurityType != null)
            {
                ar.Add("(fldSecurityType like N'%" + LetterSearch.SecurityType + "%')");
            }

            if (LetterSearch.Keywords != "" && LetterSearch.Keywords != null)
            {
                ar.Add("(fldKeywords like N'%" + LetterSearch.Keywords + "%')");
            }
            if (LetterSearch.ImmediacyName != "" && LetterSearch.ImmediacyName != null)
            {
                ar.Add("(fldImmediacyName like N'%" + LetterSearch.ImmediacyName + "%')");
            }
            if (LetterSearch.SenderName != "" && LetterSearch.SenderName != null)
            {
                ar.Add("(fldSenderName like N'%" + LetterSearch.SenderName + "%')");
            }
            if (LetterSearch.ReciverName != "" && LetterSearch.ReciverName != null)
            {
                ar.Add("(LetterRecievers like N'%" + LetterSearch.ReciverName + "%')");
            }
            if (LetterSearch.StartCreatedDate != "" && LetterSearch.EndCreatedDate != "" && LetterSearch.StartCreatedDate != null && LetterSearch.EndCreatedDate != null)
            {
                ar.Add("(dbo.MiladiTOShamsi(fldCreatedDate)>='" + LetterSearch.StartCreatedDate + "') and (dbo.MiladiTOShamsi(fldCreatedDate)<='" + LetterSearch.EndCreatedDate + "')");
            }
            if (ar.Count > 1)
            {
                int i;
                for (i = 0; i < ar.Count - 1; i++)
                {
                    s = s + ar[i].ToString() + " and ";
                }
                s = " where " + s + ar[i].ToString();
            }
            else if (ar.Count == 1)
                s = " where " + ar[0].ToString();
           // s = " and " + ar[0].ToString();
            return s;
             
        }*/
        string Where(LetterSearch LetterSearch)
        {
            ArrayList ar = new ArrayList();
            string s = "";

            if (LetterSearch.LetterDate != "" && LetterSearch.LetterDate != null)
            {
                ar.Add("(d.fldTarikh  like N'%" + LetterSearch.LetterDate + "%')");
            }
            if (LetterSearch.StartCreatedDate != "" && LetterSearch.EndCreatedDate != "" && LetterSearch.StartCreatedDate != null && LetterSearch.EndCreatedDate != null)
            {
                ar.Add("(d2.fldTarikh>='" + LetterSearch.StartCreatedDate + "') and (d2.fldTarikh<='" + LetterSearch.EndCreatedDate + "')");
            }
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
            if (LetterSearch.SecurityType != "" && LetterSearch.SecurityType != null)
            {
                ar.Add("(tblSecurityType.fldId like N'%" + LetterSearch.SecurityType + "%')");
            }

            if (LetterSearch.Keywords != "" && LetterSearch.Keywords != null)
            {
                ar.Add("(tblLetter.fldKeywords like N'%" + LetterSearch.Keywords + "%')");
            }
            if (LetterSearch.ImmediacyName != "" && LetterSearch.ImmediacyName != null)
            {
                ar.Add("(tblImmediacy.fldId like N'%" + LetterSearch.ImmediacyName + "%')");
            }
            //if (LetterSearch.SenderName != "" && LetterSearch.SenderName != null)
            //{
            //    ar.Add("(fldSenderName like N'%" + LetterSearch.SenderName + "%')");
            //}
            //if (LetterSearch.ReciverName != "" && LetterSearch.ReciverName != null)
            //{
            //    ar.Add("(LetterRecievers like N'%" + LetterSearch.ReciverName + "%')");
            //}
            if (ar.Count > 1)
            {
                int i;
                for (i = 0; i < ar.Count - 1; i++)
                {
                    s = s + ar[i].ToString() + " and ";
                }
                s = " where " + s + ar[i].ToString();
            }
            else if (ar.Count == 1)
                s = " where " + ar[0].ToString();
            // s = " and " + ar[0].ToString();
            return s;

        }
        string WhereSenderReciever(LetterSearch LetterSearch)
        {
            ArrayList ar = new ArrayList();
            string s = "";

            if (LetterSearch.SenderName != "" && LetterSearch.SenderName != null)
            {
                ar.Add("(fldSenderName like N'%" + LetterSearch.SenderName + "%')");
            }
            if (LetterSearch.ReciverName != "" && LetterSearch.ReciverName != null)
            {
                ar.Add("(LetterRecievers like N'%" + LetterSearch.ReciverName + "%')");
            }
        
            if (ar.Count > 1)
            {
                int i;
                for (i = 0; i < ar.Count - 1; i++)
                {
                    s = s + ar[i].ToString() + " and ";
                }
                s = " where " + s + ar[i].ToString();
            }
            else if (ar.Count == 1)
                s = " where " + ar[0].ToString();
            // s = " and " + ar[0].ToString();
            return s;

        }

        public ActionResult Search(LetterSearch LetterSearch)
        {
           //string sqlString = "SELECT  fldID,fldOrderId,fldSubject,fldLetterNumber,fldLetterDate, "+
           // " fldKeywords,fldLetterTypeID,fldSecurityType,fldImmediacyName,fldSenderName, "+
           // " fldDesc,LetterRecievers,ComisionId,fldAssignmentID,fldCreatedDate " +
           // "  from (select tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject,  "+
           // " tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate,  "+
           // " tblLetter.fldKeywords, tblLetter.fldLetterTypeID, tblSecurityType.fldType AS fldSecurityType,  "+
           // " tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName, "+
           //"  tblLetter.fldDesc,dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers, "+
           // " tblInternalAssignmentReceiver.fldReceiverComisionID as ComisionId,tblInternalAssignmentReceiver.fldAssignmentID,tblLetter.fldCreatedDate " +
           // "  FROM tblInternalAssignmentReceiver INNER JOIN tblAssignment "+
           //  "  ON tblInternalAssignmentReceiver.fldAssignmentID = tblAssignment.fldID  "+
           //  "  INNER JOIN tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN tblCommision  "+
           // "   ON tblInternalAssignmentReceiver.fldReceiverComisionID = tblCommision.fldID  "+
           // "   INNER JOIN tblStaff ON tblCommision.fldStaffID = tblStaff.fldID  "+
           //  "  INNER JOIN tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID  "+
           //  "  INNER JOIN tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID  "+
           //  "  INNER JOIN tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID   "+
           //  "  where exists (select c.fldid from tblCommision c inner join tblUser u "+
           //   " on c.fldStaffID=u.fldStaffID  "+
           //  "  where u.fldID= " + Session["UserId"].ToString() + " and tblInternalAssignmentReceiver.fldReceiverComisionID =c.fldid) "+
           //  "   union all "+
           //   "  SELECT tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject,  "+
           //   "  tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate, "+
           //   "   tblLetter.fldKeywords, tblLetter.fldLetterTypeID, tblSecurityType.fldType AS fldSecurityType,  "+
           //   "   tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName,  "+
           //   "   tblLetter.fldDesc, dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers, "+
           //   "   tblInternalAssignmentSender.fldSenderComisionID as ComisionId,tblInternalAssignmentSender.fldAssignmentID,tblLetter.fldCreatedDate  " +
           //    "  FROM tblInternalAssignmentSender  "+
           //   "   INNER JOIN tblAssignment ON tblInternalAssignmentSender.fldAssignmentID = tblAssignment.fldID  "+
           //   "   INNER JOIN tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID  "+
           //   "   INNER JOIN tblCommision ON dbo.tblInternalAssignmentSender.fldsenderComisionID = tblCommision.fldID  "+
           //   "   INNER JOIN tblStaff ON tblCommision.fldStaffID = tblStaff.fldID  "+
           //   "   INNER JOIN tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID  "+
           //   "   INNER JOIN tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID  "+
           //   "   INNER JOIN tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID  "+
           //   "   WHERE  exists (select c.fldid from tblCommision c inner join tblUser u "+
           //  "  on c.fldStaffID=u.fldStaffID  "+
           //  "  where u.fldID= " + Session["UserId"].ToString() + " and tblInternalAssignmentSender.fldSenderComisionID =c.fldid) "+
           // " )t  "+ Where(LetterSearch);

            string sqlString = " SELECT        fldID, fldOrderId, fldSubject, fldLetterNumber, fldLetterDate, fldKeywords, fldLetterTypeID, fldSecurityType, fldImmediacyName " +
" , fldSenderName, fldDesc, LetterRecievers, ComisionId, fldAssignmentID, fldCreatedDate " +
" FROM            (SELECT        tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber " +
        " 					,d.fldTarikh   AS fldLetterDate " +
                " 			, tblLetter.fldKeywords, tblLetter.fldLetterTypeID,tblSecurityType.fldType AS fldSecurityType " +
                    " 		, tblImmediacy.fldName AS fldImmediacyName " +
                        " 		,case when charindex('',fldSubject)<>0 then '/'+' '+substring(fldSubject,1,charindex('',fldSubject)) " +
                    " 			when  empsend.EmployeeList1 is not null  then empsend.EmployeeList1 else empsend2.EmployeeList  end as fldSenderName " +
                    " 		, tblLetter.fldDesc " +
                    " 		,case when emp1.EmployeeList1 is null  then emp2.EmployeeList when emp2.EmployeeList  is null  then emp1.EmployeeList1 else emp1.EmployeeList1+'/'+emp2.EmployeeList end as LetterRecievers " +
                    " 		, tblInternalAssignmentReceiver.fldReceiverComisionID AS ComisionId " +
                    " 		, tblInternalAssignmentReceiver.fldAssignmentID, tblLetter.fldCreatedDate " +
                     "      FROM            tblInternalAssignmentReceiver INNER JOIN " +
                      "                               tblAssignment ON tblInternalAssignmentReceiver.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                        "                             tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID left JOIN " +
                        " 							tblDateDim as d on d.fldDate=cast(tblLetter.fldLetterDate as date) inner join	 " +
                        " 						    tblDateDim as d2 on d2.fldDate=cast(tblLetter.fldCreatedDate as date) inner join	 " +
                         "                            tblCommision ON tblInternalAssignmentReceiver.fldReceiverComisionID = tblCommision.fldID INNER JOIN " +
                         "                            tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN " +
                         "                            tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                         "                            tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                         "                            tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID " +
                        " 	outer apply (select stuff((SELECT '/' + tblExternalPartner.fldName " +
                        " 	FROM         dbo.tblExternalLetterReceiver INNER JOIN " +
                        " 						  dbo.tblExternalPartner ON dbo.tblExternalLetterReceiver.fldExternalPartnerID = dbo.tblExternalPartner.fldId " +
                        " 	WHERE     (dbo.tblExternalLetterReceiver.fldLetterId  =tblLetter.fldID)  for xml path('')),1,1,'') as EmployeeList1 ) emp1 " +
" outer apply (select stuff((SELECT +'/' +tblStaff.fldName+' '+ tblStaff.fldfamily+'-'+ tblOrganicRole.fldName " +
                        " 	FROM         tblLetter as l INNER JOIN " +
                                    " 			  tblInternalLetterReceiver ON l.fldId = tblInternalLetterReceiver.[fldLetterId] INNER JOIN " +
                                    " 			  tblStaff INNER JOIN " +
                                    " 			  tblCommision ON tblStaff.fldId = tblCommision.fldStaffId INNER JOIN " +
                                    " 			  tblOrganicRole ON tblCommision.fldOrganicRoleId = tblOrganicRole.fldId ON tblInternalLetterReceiver.fldReceiverComisionID = tblCommision.fldId " +
                        " 	WHERE     l.fldId  =tblLetter.fldID for xml path('')),1,1,'') as EmployeeList)emp2 " +
                        " 	outer apply (select stuff((SELECT '/' + tblExternalPartner.fldName " +
                        " 	FROM         dbo.tblExternalLetterSender INNER JOIN " +
                        " 						  dbo.tblExternalPartner ON dbo.tblExternalLetterSender.fldExternalPartnerID = dbo.tblExternalPartner.fldId " +
                        " 	WHERE     (dbo.tblExternalLetterSender.fldLetterId  =tblLetter.fldID)  for xml path('')),1,1,'') as EmployeeList1 ) empsend " +
" 	outer apply (select stuff((SELECT +'/'  " +
                                        " 			+tblStaff.fldName+' '+ tblStaff.fldfamily+'-'+ tblOrganicRole.fldname " +
                                        " 			FROM         tblLetter as l INNER JOIN " +
                                " 	 tblStaff INNER JOIN " +
                                " 	tblCommision ON tblStaff.fldId = tblCommision.fldStaffId INNER JOIN " +
                                " 	tblOrganicRole ON tblCommision.fldOrganicRoleId = tblOrganicRole.fldId ON tblLetter.fldComisionId = tblCommision.fldId " +
                                " 					WHERE     l.fldId  =tblLetter.fldID for xml path('')),1,1,'') as EmployeeList)empsend2 " +
                    " 	cross apply    (SELECT top 1       c.fldID " +
                                              "             FROM            tblCommision AS c INNER JOIN " +
                                              "                                       tblUser AS u ON c.fldStaffID = u.fldStaffID " +
                                              "             WHERE        (u.fldID = " + Session["UserId"].ToString() + ")  " +
                                                " 		  AND (tblInternalAssignmentReceiver.fldReceiverComisionID = c.fldID))t " +
                       Where(LetterSearch) +
                         "  UNION ALL " +
                       "    SELECT        tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber " +
                            " 			,d.fldTarikh  AS fldLetterDate " +
                            " 			, tblLetter.fldKeywords, tblLetter.fldLetterTypeID,  " +
                           "              tblSecurityType.fldType AS fldSecurityType, tblImmediacy.fldName AS fldImmediacyName " +
                        " 				,case when charindex('',fldSubject)<>0 then '/'+' '+substring(fldSubject,1,charindex('',fldSubject)) " +
                        " 		when  empsend.EmployeeList1 is not null  then empsend.EmployeeList1 else empsend2.EmployeeList  end as fldSenderName " +
                            " 			, tblLetter.fldDesc " +
                            " 			,case when emp1.EmployeeList1 is null  then emp2.EmployeeList when emp2.EmployeeList  is null  then emp1.EmployeeList1 else emp1.EmployeeList1+'/'+emp2.EmployeeList end as LetterRecievers " +
                            " 			, tblInternalAssignmentSender.fldSenderComisionID AS ComisionId " +
                            " 			, tblInternalAssignmentSender.fldAssignmentID, tblLetter.fldCreatedDate " +
                       "    FROM            tblInternalAssignmentSender INNER JOIN " +
                             "                       tblAssignment ON tblInternalAssignmentSender.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                              "                      tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID left JOIN " +
                            " 					   tblDateDim as d on d.fldDate=cast(tblLetter.fldLetterDate as date) inner join		 " +
                            " 					   tblDateDim as d2 on d2.fldDate=cast(tblLetter.fldCreatedDate as date) inner join	 " +
                             "                       tblCommision ON tblInternalAssignmentSender.fldSenderComisionID = tblCommision.fldID INNER JOIN " +
                             "                       tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN " +
                             "                       tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                             "                       tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                             "                       tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID " +
                            " 					  outer apply (select stuff((SELECT '/' + tblExternalPartner.fldName " +
                            " FROM         dbo.tblExternalLetterReceiver INNER JOIN " +
                            " 					  dbo.tblExternalPartner ON dbo.tblExternalLetterReceiver.fldExternalPartnerID = dbo.tblExternalPartner.fldId " +
                        " 	WHERE     (dbo.tblExternalLetterReceiver.fldLetterId  =tblLetter.fldID)  for xml path('')),1,1,'') as EmployeeList1 ) emp1 " +
" outer apply (select stuff((SELECT +'/' +tblStaff.fldName+' '+ tblStaff.fldfamily+'-'+ tblOrganicRole.fldName " +
                            " FROM         tblLetter as l INNER JOIN " +
                            " 					  tblInternalLetterReceiver ON l.fldId = tblInternalLetterReceiver.[fldLetterId] INNER JOIN " +
                            " 					  tblStaff INNER JOIN " +
                            " 					  tblCommision ON tblStaff.fldId = tblCommision.fldStaffId INNER JOIN " +
                            " 					  tblOrganicRole ON tblCommision.fldOrganicRoleId = tblOrganicRole.fldId ON tblInternalLetterReceiver.fldReceiverComisionID = tblCommision.fldId " +
                            " WHERE     l.fldId  =tblLetter.fldID for xml path('')),1,1,'') as EmployeeList)emp2 " +
                        " 	outer apply (select stuff((SELECT '/' + tblExternalPartner.fldName " +
                        " 	FROM         dbo.tblExternalLetterSender INNER JOIN " +
                        " 						  dbo.tblExternalPartner ON dbo.tblExternalLetterSender.fldExternalPartnerID = dbo.tblExternalPartner.fldId " +
                        " 	WHERE     (dbo.tblExternalLetterSender.fldLetterId  =tblLetter.fldID)  for xml path('')),1,1,'') as EmployeeList1 ) empsend " +
" 	outer apply (select stuff((SELECT +'/'  " +
                                    " 				+tblStaff.fldName+' '+ tblStaff.fldfamily+'-'+ tblOrganicRole.fldname " +
                                    " 				FROM         tblLetter as l INNER JOIN " +
                                " 	 tblStaff INNER JOIN " +
                                " 	tblCommision ON tblStaff.fldId = tblCommision.fldStaffId INNER JOIN " +
                                " 	tblOrganicRole ON tblCommision.fldOrganicRoleId = tblOrganicRole.fldId ON tblLetter.fldComisionId = tblCommision.fldId " +
                                    " 				WHERE     l.fldId  =tblLetter.fldID for xml path('')),1,1,'') as EmployeeList)empsend2 " +
                        " 	cross apply (select top 1 c.fldid from tblCommision c inner join tblUser u    " +
              "   on c.fldStaffID=u.fldStaffID     " +
              "   where u.fldID=" + Session["UserId"].ToString() + "   and tblInternalAssignmentSender.fldSenderComisionID =c.fldid)	t					 " +
                      Where(LetterSearch) +
                                                " 		) AS t " +
WhereSenderReciever(LetterSearch) +
      "  order by fldLetterDate desc";
            
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
                if (!DBNull.Value.Equals(m[5]))
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
                if (!DBNull.Value.Equals(m[11]))
                    l.ReciverName = (string)m[11];
                l.ComisionId =(Int32)m[12];
                l.fldAssignmentID = (Int64)m[13];
                p.Add(l);
            }
            return Json(p.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateReadFlag(int AssignmentID, int ComisionId)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                var Assignment = p.sp_tblAssignmentSelect("fldId", AssignmentID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Assignment != null)
                {
                    var AssStatus = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", Assignment.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(h => h.fldReceiverComisionID == ComisionId).FirstOrDefault();
                    if (AssStatus != null)
                        if (AssStatus.fldAssignmentStatusID == 1)
                        {
                            p.sp_tblInternalAssignmentReceiverStatusUpdate(AssStatus.fldID, 2, Convert.ToInt32(Session["UserId"]));
                        }
                }
                return Json(new
                {
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
    
}
