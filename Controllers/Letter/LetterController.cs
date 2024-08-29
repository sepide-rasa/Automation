using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.IO;
using Automation.Controllers.Users;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using Automation.Models;
using System.Collections;
using Aspose.Words;
using Aspose.Cells;
using System.Drawing;


namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class LetterController : Controller
    {
        //
        // GET: /Letter/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 70))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult IndexTab(string HistoryLetter_Id, string ComisionID)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 70))
            {
                ViewBag.fldHistoryLetter_Id = HistoryLetter_Id;
                ViewBag.fldComisionID = ComisionID;
                return View();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult GenerateLetterPDF(int? id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var Content = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 0, 0, "").ToList();
                var path = Server.MapPath(@"~\Uploaded\Letter");
                byte[] pdf=null;
                foreach (var Item in Content)
                {
                    if (Item.fldExt == ".docx")
                    {
                        System.IO.File.WriteAllBytes(path + ".docx", Item.fldLetterText);
                        Document doc = new Document(path+".docx");
                        doc.Save(path+ "." + "pdf");
                        pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                        System.IO.File.Delete(path + ".docx");
                        System.IO.File.Delete(path + ".pdf");
                    }
                }
                if (Content != null)
                    return File(pdf, "application/pdf");
                    
                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GenerateAttachPDF(int? id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                var letterAttachment = p.sp_tblLetterAttachmentSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var Content = p.sp_tblContentFileSelect("fldID", letterAttachment.fldContentFileID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                string Ex="";
                var path = Server.MapPath(@"~\Uploaded\" + Content.fldName);
                Ex = Path.GetExtension(path);
                if (Content.fldExt == ".docx")
                    Ex = ".docx";
                var pathName = Server.MapPath(@"~\Uploaded\" + Path.GetFileNameWithoutExtension(path));

                byte[] pdf = null;

                if (Ex == ".docx"||Ex == ".doc")
                    {
                        System.IO.File.WriteAllBytes(path + ".docx", Content.fldLetterText);
                        Document doc = new Document(path + ".docx");
                        doc.Save(path + "." + "pdf");
                        pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                        System.IO.File.Delete(path + ".docx");
                        System.IO.File.Delete(path + ".pdf");
                    }
                else if (Ex == ".xlsx"||Ex == ".xls")
                {
                    System.IO.File.WriteAllBytes(path + ".xls", Content.fldLetterText);
                    Workbook workbook = new Workbook(path + ".xls");
                    workbook.Save(path + "." + "pdf");
                    pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                    System.IO.File.Delete(path + ".xls");
                    System.IO.File.Delete(path + ".pdf");
                }
                else if (Ex == ".pdf")
                {
                    pdf = Content.fldLetterText;
                }
                else
                {
                    Aspose.Pdf.Generator.Pdf pdf1 = new Aspose.Pdf.Generator.Pdf();
                    System.IO.File.WriteAllBytes(path, Content.fldLetterText);

                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    byte[] tmpBytes = new byte[fs.Length];
                    fs.Read(tmpBytes, 0, Convert.ToInt32(fs.Length));

                    MemoryStream mystream = new MemoryStream(tmpBytes);


                    Bitmap b = new Bitmap(mystream);
                    //Create a new section in the Pdf document
                    Aspose.Pdf.Generator.Section sec1 = new Aspose.Pdf.Generator.Section(pdf1);

                    // Set margins so image will fit, etc.
                    sec1.PageInfo.Margin.Top = 5;
                    sec1.PageInfo.Margin.Bottom = 5;
                    sec1.PageInfo.Margin.Left = 5;
                    sec1.PageInfo.Margin.Right = 5;

                    sec1.PageInfo.PageWidth = (b.Width / b.HorizontalResolution) * 72;
                    sec1.PageInfo.PageHeight = (b.Height / b.VerticalResolution) * 72;

                    //Add the section in the sections collection of the Pdf document
                    pdf1.Sections.Add(sec1);

                    //Create an image object
                    Aspose.Pdf.Generator.Image image1 = new Aspose.Pdf.Generator.Image(sec1);

                    //Add the image into paragraphs collection of the section
                    sec1.Paragraphs.Add(image1);
                    image1.ImageInfo.ImageFileType = Aspose.Pdf.Generator.ImageFileType.Tiff;

                    // set IsBlackWhite property to true for performance improvement
                    image1.ImageInfo.IsBlackWhite = false;
                    //Set the ImageStream to a MemoryStream object
                    image1.ImageInfo.ImageStream = mystream;
                    //Set desired image scale
                    image1.ImageScale = 0.95F;

                    //Save the Pdf
                    pdf1.Save(pathName + ".pdf");
                    fs.Close();
                    pdf = System.IO.File.ReadAllBytes(pathName + ".pdf");
                    System.IO.File.Delete(path);
                    System.IO.File.Delete(pathName + ".pdf");
                }
                if (Content != null)
                    return File(pdf, "application/pdf");

                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PreviewLetterImageBox()
        {
            return PartialView();
        }

        public ActionResult GetRoonevesht(int LetterId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var ronevesht = p.sp_tblLetterRoneveshtSelect(LetterId).ToList();
            string RoneveshtId = "", RoneveshtName = "";
            foreach (var item in ronevesht)
            {
                RoneveshtId += item.fldComID + "|" + item.fldType + ";";
                RoneveshtName += item.fldNameStaff + ";";
            }
            return Json(new
            {
                rooneveshtName = RoneveshtName,
                rooneveshtID = RoneveshtId
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadAttach(HttpPostedFileBase files)
        {
            if (files != null)
            {
                // Some browsers send file names with full path.
                // We are only interested in the file name.
                var fileName = Path.GetFileName(files.FileName);
                string savePath = Server.MapPath(@"~\Uploaded\" + fileName);
                Session["savePath"] = savePath;
                // The files are not actually saved in this demo
                files.SaveAs(savePath);

            }
            return Content("");
        }
        public ActionResult RemoveAttach(string fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
            {
                string physicalPath = Server.MapPath(@"~\Uploaded\" + fileNames);
                if (System.IO.File.Exists(physicalPath))
                {
                    // The files are not actually removed in this demo
                    System.IO.File.Delete(physicalPath);
                }
                Session.Remove("savePath");
            }
            return Content("");
        }

        public ActionResult GetImmediacyTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblImmediacySelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetComission()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var date = p.sp_GetDate().FirstOrDefault();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblCommisionSelect("fldStaffID", user.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
            if (Session["ComId"] != null)
            {
                q = p.sp_tblCommisionSelect("fldId", Session["ComId"].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
                Session.Remove("ComId");
            }
            return Json(q, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSecurityTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblSecurityTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult FillSavabegh([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblHistoryLetterSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
        //    return Json(q);
        //}

        public ActionResult ReloadSavabegh(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldCurrentLetter_Id", "fldLetterNumber", "fldSubject" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();//.Where(k => k.fldCurrentLetter_Id == value)
            var q = m.sp_tblHistoryLetterSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReloadStatusFinish(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldLetterId", "fldLetterNumber", "fldSubject" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterFollowSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }



        string Where(string field, string value)
        {
            ArrayList ar = new ArrayList();
            string s = "";
            if (field=="0")
            {
                ar.Add("(tblLetter.fldOrderId like N'%" + value + "%')");
            }
            if (field == "1")
            {
                ar.Add("(tblLetter.fldLetterNumber like N'%" + value + "%')");
            }
            if (field == "2")
            {
                ar.Add("(dbo.MiladiTOShamsi(tblLetter.fldLetterDate) like N'%" + value + "%')");
            }
            if (field == "3")
            {
                ar.Add("(dbo.MiladiTOShamsi(tblLetter.fldCreatedDate) like N'%" + value + "%')");
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
        public ActionResult ReloadLetter(string field, string value, int top, int searchtype)
        {//جستجو
            //string[] _fiald = new string[] { "fldLetterNumber", "fldSubject" };
            //string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            //string searchtext = string.Format(searchType[searchtype], value);
            //Models.AutomationEntities mm = new Models.AutomationEntities();
            //var q = mm.sp_tblLetterSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            //return Json(q, JsonRequestBehavior.AllowGet);
            //}

        //public ActionResult Search(LetterSearch LetterSearch)
        //{
            string sqlString = "SELECT     tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject, tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate, dbo.MiladiTOShamsi(tblLetter.fldCreatedDate) AS fldCreatedDate, tblLetter.fldKeywords, tblLetter.fldLetterTypeID, " +
                      "tblSecurityType.fldType AS fldSecurityType, tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName,tblLetter.fldDesc,dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers" +
                        " FROM         tblInternalAssignmentReceiver INNER JOIN " +
                      "tblAssignment ON tblInternalAssignmentReceiver.fldAssignmentID = tblAssignment.fldID INNER JOIN " +
                      "tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER JOIN " +
                      "tblCommision ON tblInternalAssignmentReceiver.fldReceiverComisionID = tblCommision.fldID INNER JOIN " +
                      "tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN " +
                      "tblImmediacy ON tblLetter.fldImmediacyID = tblImmediacy.fldID INNER JOIN " +
                      "tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID INNER JOIN " +
                      "tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID where tblInternalAssignmentReceiver.fldReceiverComisionID " +
                      "in(SELECT fldid FROM dbo.tblCommision WHERE fldStaffID =(SELECT fldStaffID FROM dbo.tblUser WHERE fldid=" + Session["UserId"].ToString() + ")) " + Where(field, value);
                      //+ " union SELECT     tblLetter.fldID, tblLetter.fldOrderId, tblLetter.fldSubject," +
                      //" tblLetter.fldLetterNumber, dbo.MiladiTOShamsi(tblLetter.fldLetterDate) AS fldLetterDate," +
                      //" tblLetter.fldKeywords, tblLetter.fldLetterTypeID, tblSecurityType.fldType AS fldSecurityType," +
                      //" tblImmediacy.fldName AS fldImmediacyName, dbo.GetLetterSender(tblLetter.fldID) AS fldSenderName," +
                      //" tblLetter.fldDesc, dbo.GetLetterReciever(tblLetter.fldID) AS LetterRecievers FROM " +
                      //"tblInternalAssignmentSender INNER JOIN tblAssignment ON tblInternalAssignmentSender.fldAssignmentID" +
                      //" = tblAssignment.fldID INNER JOIN tblLetter ON tblAssignment.fldLetterID = tblLetter.fldID INNER " +
                      //"JOIN tblCommision ON dbo.tblInternalAssignmentSender.fldsenderComisionID = tblCommision.fldID INNER " +
                      //"JOIN tblStaff ON tblCommision.fldStaffID = tblStaff.fldID INNER JOIN tblImmediacy ON tblLetter.fldImmediacyID " +
                      //"= tblImmediacy.fldID INNER JOIN tblSecurityType ON tblLetter.fldSecurityTypeID = tblSecurityType.fldID" +
                      //" INNER JOIN tblOrganicRole ON tblCommision.fldOrganicRoleID = tblOrganicRole.fldID WHERE     " +
                      //"(tblInternalAssignmentSender.fldSenderComisionID IN (SELECT     fldid FROM          tblCommision WHERE     " +
                      //" (fldStaffID = (SELECT     fldStaffID FROM          tblUser WHERE      (fldid = " + Session["UserId"].ToString() + "))))) " + Where(field,value);
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
                string keyWord = "", LetterNumber = "", LetterDate = "", CreatedDate = "";
                if (m[6] != null)
                    keyWord = m[6].ToString();
                if (m[3] != null)
                    LetterNumber = m[3].ToString();
                if (m[4] != null)
                    LetterDate = m[4].ToString();
                if (m[5] != null)
                    CreatedDate = m[5].ToString();
                l.fldId = (long)m[0];
                l.fldOrderId = (long)m[1];
                l.fldSubject = (string)m[2];
                l.fldLetterNumber = LetterNumber;
                l.fldLetterDate = LetterDate;
                l.fldCreatedDate = CreatedDate;
                l.fldKeywords = keyWord;
                l.fldSecurityType = (string)m[8];
                l.fldImmediacyName = (string)m[9];
                l.fldSenderName = (string)m[10];
                l.Desc = (string)m[11];
                l.ReciverName = (string)m[12];
                p.Add(l);
            }
            return Json(p.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReloadLetterAttach(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldLetterID_Attach" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterAttachmentSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult SaveStatusFinish(Models.LetterFollow LetterFollow)
        {//ذخیره آخرین پیگیری نامه
            try
            {
                if (LetterFollow.fldLetterID == 0)
                    return Json(new { data = "لطفا قبل از تعریف آخرین پیگیری، نامه را ذخیره کنید.", state = 1 });
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (LetterFollow.fldDesc == null)
                    LetterFollow.fldDesc = "";
                //if (LetterFollow.fldID == 0)
                //{//ثبت رکورد جدید

                p.sp_tblLetterFollowInsert(LetterFollow.fldLetterText, LetterFollow.fldLetterID, Convert.ToInt32(Session["UserId"]), LetterFollow.fldDesc, Session["UserPass"].ToString());

                return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0, LetterID = LetterFollow.fldLetterID });

                //}
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Khateme(/*Models.LetterFollow LetterFollow*/ int LetterID, int? InternalAssignmentReceiverID)
        {//ذخیره آخرین پیگیری نامه


            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 87))
                {
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    //if (LetterFollow.fldDesc == null)
                    //    LetterFollow.fldDesc = "";
                    //if (LetterFollow.fldID == 0)
                    //{//ثبت رکورد جدید

                    //p.sp_tblLetterFollowInsert(LetterFollow.fldLetterText, LetterFollow.fldLetterID, Convert.ToInt32(Session["UserId"]), LetterFollow.fldDesc, Session["UserPass"].ToString());
                    
                    var Msg = "خاتمه نامه امکان پذیر نمی باشد.";
                    var state = 1;
                    if (InternalAssignmentReceiverID != 0 && InternalAssignmentReceiverID != null)
                    {
                        p.sp_UpdateInternalAssignmentReceiver(InternalAssignmentReceiverID, 3);
                        Msg = "خاتمه نامه با موفقیت انجام شد.";
                        state = 0;
                    }
                    return Json(new { data = Msg, state = state, LetterID = LetterID/* LetterFollow.fldLetterID*/ }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
                //}
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1, LetterID = LetterID });
            }
        }

        public ActionResult SaveHistory(Models.sp_tblHistoryLetterSelect HistoryLetter)
        {//ذخیره آخرین پیگیری نامه


            try
            {
                //if (HistoryLetter.fldId == 0)
                //    return Json(new { data = "لطفا قبل از تعریف آخرین پیگیری، نامه را ذخیره کنید.", state = 1 });
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (HistoryLetter.fldDesc == null)
                    HistoryLetter.fldDesc = "";
                if (HistoryLetter.fldId == 0)
                {//ثبت رکورد جدید

                    p.sp_tblHistoryLetterInsert(HistoryLetter.fldCurrentLetter_Id, HistoryLetter.fldHistoryType_Id, HistoryLetter.fldHistoryLetter_Id, Convert.ToInt32(Session["UserId"]), HistoryLetter.fldDesc, Session["UserPass"].ToString());

                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0, LetterID = HistoryLetter.fldCurrentLetter_Id });

                }
                else
                {
                    p.sp_tblHistoryLetterUpdate(HistoryLetter.fldId, HistoryLetter.fldCurrentLetter_Id, HistoryLetter.fldHistoryType_Id, HistoryLetter.fldHistoryLetter_Id, Convert.ToInt32(Session["UserId"]), HistoryLetter.fldDesc, Session["UserPass"].ToString());
                    return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0, LetterID = HistoryLetter.fldCurrentLetter_Id });
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Signer(Models.Signer Signer)
        {//ذخیره آخرین پیگیری نامه
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 85))
                {
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    if (Signer.fldDesc == null)
                        Signer.fldDesc = "";
                    var signer = Signer.fldSignerComisionID.Split(';');
                    string data = "";
                    int state = -1;
                    for (int i = 0; i < signer.Count() - 1; i++)
                    {
                        //if (Signer.CreatorComId == Convert.ToInt32(signer[i]))
                        //{
                        var IsSign = p.sp_tblSignerSelect("fldLetterID", Signer.fldLetterID.ToString(), 0).Where(h => h.fldSignerComisionID == Convert.ToInt32(signer[i])).FirstOrDefault();
                        if (IsSign != null)
                        {
                            if (IsSign.fldFirstSigner == null)
                            {
                                if (signer[i] == Signer.CreatorComId.ToString())
                                {
                                    p.sp_SignLetter(IsSign.fldID, Convert.ToInt32(signer[i]), 1);
                                    p.sp_tblLetterStatusIdUpdate(Signer.fldLetterID, 2);
                                    if (state == -1)
                                    {
                                        data = "نامه با موفقیت امضا گردید.";
                                        state = 0;
                                    }
                                }
                                else
                                {
                                    var Substitute = p.sp_tblSubstituteSelect("fldReceiverComisionID", Signer.CreatorComId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                    if (Substitute != null)
                                    {
                                        if (Substitute.fldIsSigner)
                                        {
                                            p.sp_SignLetter(IsSign.fldID, Signer.CreatorComId, 1);
                                            p.sp_tblLetterStatusIdUpdate(Signer.fldLetterID, 2);
                                            if (state == -1)
                                            {
                                                data = "نامه با موفقیت امضا گردید.";
                                                state = 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (state == -1)
                                        {
                                            data = "شما مجوز امضا نامه را ندارید.";
                                            state = 1;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (state == -1)
                                {
                                    data = "نامه قبلا امضا شده است.";
                                    state = 1;
                                }
                            }
                        }
                        else
                        {
                            if (state == -1)
                            {
                                data = "شما مجوز امضا نامه را ندارید.";
                                state = 1;
                            }
                        }
                        //}
                        //else
                        //{
                        //    if (state == -1)
                        //    {
                        //        data = "شما مجوز امضا نامه را ندارید.";
                        //        state = 1;
                        //    }
                        //}
                    }
                    return Json(new { data = data, state = state });
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public FileContentResult FileExport(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var att = p.sp_tblLetterAttachmentSelect("fldId", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblContentFileSelect("fldId", att.fldContentFileID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            return File(q.fldLetterText, MimeType.Get(q.fldName.Split('.').Last()), q.fldName);
        }

        public ActionResult SaveAttach(Models.LetterAssign LetterAttachment)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (LetterAttachment.fldDesc == null)
                    LetterAttachment.fldDesc = "";
                if (LetterAttachment.fldID == 0)
                {//ثبت رکورد جدید                    
                    byte[] _file = null;
                    if (Session["savePath"] != null)
                    {
                        MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                        //if (stream.Length < 5242880)
                        //{
                        string filename = Path.GetFileName(Session["savePath"].ToString());
                            //if (LetterAttachment.fldName==null)
                            //    LetterAttachment.fldName = Path.GetFileNameWithoutExtension(Session["savePath"].ToString());
                            System.IO.File.Delete(Session["savePath"].ToString());
                            _file = stream.ToArray();
                            System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            p.sp_tblContentFileInsert(_id, /*LetterAttachment.fldName*/ filename, _file, null, LetterAttachment.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                            p.sp_tblLetterAttachmentInsert(LetterAttachment.fldLetterID, LetterAttachment.fldName , Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), LetterAttachment.fldDesc, Session["UserPass"].ToString());
                            Session.Remove("savePath");
                            return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                        //}
                        //else
                        //{
                        //    System.IO.File.Delete(Session["savePath"].ToString());
                        //    Session.Remove("savePath");
                        //    return Json(new { data = "حجم فایل پیوست بیشتر از 5 مگا بایت می باشد.", state = 1 });
                        //}
                    }
                    else
                        return Json(new { data = "لطفا فایل را وارد کنید.", state = 1 });

                }
                else
                {//ویرایش رکورد ارسالی
                    byte[] report_file = null;
                    if (Session["savePath"] != null)
                    {
                        //var q = p.sp_tblReportsSelect("fldOlgoLetterId", LetterAttachment.fldID.ToString(), 1).FirstOrDefault();
                        //MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                        //System.IO.File.Delete(Session["savePath"].ToString());
                        //Session.Remove("savePath");
                        //report_file = stream.ToArray();
                        //p.sp_tblOlgoGharardadUpdate(LetterAttachment.fldID, LetterAttachment.fldName, 1, LetterAttachment.fldDesc);
                        //p.sp_tblReportsUpdate(q.fldId, report_file, null, LetterAttachment.fldID, 1, LetterAttachment.fldDesc);
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                    }
                    else
                        return Json(new { data = "لطفا فایل گزارش را وارد کنید.", state = 1 });
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult FillAttach([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var m = q.sp_tblLetterAttachmentSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(m);

        }
        public ActionResult Save(Models.Letter Letter)
        {
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 84))
                {
                    DateTime? fldLetterDate = null;
                    if (Letter.fldLetterDate != null)
                        fldLetterDate = MyLib.Shamsi.Shamsi2miladiDateTime(Letter.fldLetterDate);

                    Models.AutomationEntities p = new Models.AutomationEntities();
                    if (Letter.fldDesc == null)
                        Letter.fldDesc = "";
                    if (Letter.fldID == 0)
                    {//ثبت رکورد جدید
                        System.Data.Objects.ObjectParameter _Letterid = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        System.Data.Objects.ObjectParameter _LetterOrderid = new System.Data.Objects.ObjectParameter("fldOrderId", typeof(int));
                        System.Data.Objects.ObjectParameter _IDLetterBox = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        if (Letter.fldExternalLetterReceiverExternalPartnerID == null)
                            return Json(new { data = "لطفا گیرنده نامه را مشخص کنید.", state = 1 });

                        if (Letter.fldSignerComisionID == null)
                            return Json(new { data = "لطفا امضا کننده نامه را مشخص کنید.", state = 1 });

                        var g = Letter.fldExternalLetterReceiverExternalPartnerID.Split(';');
                        var signer = Letter.fldSignerComisionID.Split(';');
                        string[] roonevesht = null;
                        string[] rooneveshtAssTypeId = null;
                        string[] rooneveshtAssDesc = null;
                        if (Letter.fldRooneveshtID != null)
                        {
                            roonevesht = Letter.fldRooneveshtID.Split(';');
                            rooneveshtAssTypeId = Letter.fldRooneveshtAssTypeID.Split(';');
                            rooneveshtAssDesc = Letter.fldRooneveshtAssDesc.Split(';');
                        }

                        p.sp_tblLetterInsert(_Letterid, Convert.ToInt32(Session["Year"]), _LetterOrderid, Letter.fldSubject, null, null, Letter.fldKeywords, 1, Letter.fldComisionID, Letter.fldImmediacyID, Letter.fldSecurityTypeID, 1, Letter.fldSignType, Convert.ToInt32(Session["UserId"]), Letter.fldDesc, Session["UserPass"].ToString());
                        for (int i = 0; i < g.Count() - 1; i++)
                        {
                            var recivers = g[i].Split('|');
                            if (recivers[1] == "2")
                                p.sp_tblExternalLetterReceiverInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(recivers[0]), Convert.ToInt32(Session["UserId"]), "");
                            else
                                p.sp_tblInternalLetterReceiverInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(recivers[0]), null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        }
                        var BoxID = p.sp_tblBoxSelect("fldComisionID", Letter.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 3).FirstOrDefault();
                        p.sp_tblLetterBoxInsert(_IDLetterBox, Convert.ToInt64(_Letterid.Value), BoxID.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        for (int j = 0; j < signer.Length - 1; j++)
                        {
                            p.sp_tblSignerInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(signer[j]), j + 1, null, Convert.ToInt32(Session["UserId"]), "");
                        }
                        if (roonevesht != null)
                        {
                            for (int k = 0; k < roonevesht.Length - 1; k++)
                            {
                                var roneveshts = roonevesht[k].Split('|');
                                if (roneveshts[1] == "2")
                                    p.sp_tblRoneveshtInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(roneveshts[0]), null, Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
                                else
                                    p.sp_tblRoneveshtInsert(Convert.ToInt64(_Letterid.Value), null, Convert.ToInt32(roneveshts[0]), Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
                            }
                        }
                        var q = p.sp_tblLetterSelect("fldId", _Letterid.Value.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        return Json(new
                        {
                            data = "ذخیره با موفقیت انجام شد.",
                            state = 0,
                            LetterID = _Letterid.Value,
                            LetterOrderId = _LetterOrderid.Value,
                            LetterCreateDate = q.fldCreatedDate,
                            CreatorId = q.fldComisionID
                        });
                    }
                    else
                    {//ویرایش رکورد ارسالی
                        var IsSign = p.sp_tblSignerSelect("fldLetterID", Letter.fldID.ToString(), 0).Where(h => h.fldFirstSigner != null).FirstOrDefault();
                        if (IsSign == null)
                        {
                            System.Data.Objects.ObjectParameter _Letterid = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            System.Data.Objects.ObjectParameter _LetterOrderid = new System.Data.Objects.ObjectParameter("fldOrderId", typeof(int));
                            System.Data.Objects.ObjectParameter _IDLetterBox = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            if (Letter.fldExternalLetterReceiverExternalPartnerID == null)
                                return Json(new { data = "لطفا گیرنده نامه را مشخص کنید.", state = 1 });

                            if (Letter.fldSignerComisionID == null)
                                return Json(new { data = "لطفا امضا کننده نامه را مشخص کنید.", state = 1 });

                            var g = Letter.fldExternalLetterReceiverExternalPartnerID.Split(';');
                            var signer = Letter.fldSignerComisionID.Split(';');
                            string[] roonevesht = null;
                            string[] rooneveshtAssTypeId = null;
                            string[] rooneveshtAssDesc = null;
                            if (Letter.fldRooneveshtID != null)
                            {
                                roonevesht = Letter.fldRooneveshtID.Split(';');
                                rooneveshtAssTypeId = Letter.fldRooneveshtAssTypeID.Split(';');
                                rooneveshtAssDesc = Letter.fldRooneveshtAssDesc.Split(';');
                            }

                            p.sp_tblLetterUpdate(Letter.fldID, Convert.ToInt32(Session["Year"]), 0, Letter.fldSubject, Letter.fldLetterNumber, fldLetterDate, Letter.fldKeywords, 1, Letter.fldComisionID, Letter.fldImmediacyID, Letter.fldSecurityTypeID, 1, Letter.fldSignType, Convert.ToInt32(Session["UserId"]), Letter.fldDesc, Session["UserPass"].ToString());
                            p.sp_tblExternalLetterReceiverDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]));
                            p.sp_tblInternalLetterReceiverDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                            for (int i = 0; i < g.Count() - 1; i++)
                            {
                                var recivers = g[i].Split('|');
                                if (recivers[1] == "2")
                                    p.sp_tblExternalLetterReceiverInsert(Convert.ToInt64(Letter.fldID), Convert.ToInt32(recivers[0]), Convert.ToInt32(Session["UserId"]), "");
                                else
                                    p.sp_tblInternalLetterReceiverInsert(Convert.ToInt64(Letter.fldID), Convert.ToInt32(recivers[0]), null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                            }
                            p.sp_tblSignerDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]));
                            for (int j = 0; j < signer.Length - 1; j++)
                            {
                                p.sp_tblSignerInsert(Convert.ToInt64(Letter.fldID), Convert.ToInt32(signer[j]), j + 1, null, Convert.ToInt32(Session["UserId"]), "");
                            }

                            p.sp_tblRoneveshtDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]));
                            if (roonevesht != null)
                            {
                                for (int k = 0; k < roonevesht.Length - 1; k++)
                                {
                                    var roneveshts = roonevesht[k].Split('|');
                                    if (roneveshts[1] == "2")
                                        p.sp_tblRoneveshtInsert(Convert.ToInt64(Letter.fldID), Convert.ToInt32(roneveshts[0]), null, Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
                                    else
                                        p.sp_tblRoneveshtInsert(Convert.ToInt64(Letter.fldID), null, Convert.ToInt32(roneveshts[0]), Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
                                }
                            }
                            var q = p.sp_tblLetterSelect("fldId", Letter.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            return Json(new
                            {
                                data = "ویرایش با موفقیت انجام شد.",
                                state = 0,
                                LetterID = Letter.fldID,
                                LetterOrderId = q.fldOrderId,
                                LetterCreateDate = q.fldCreatedDate,
                                CreatorId = q.fldComisionID
                            });
                        }
                        else
                            return Json(new { data = "نامه امضا شده و قابل تغییر نمی باشد.", state = 1 });
                    }
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        //public ActionResult Reload(string field, string value, int top, int searchtype)
        //{//جستجو
        //    string[] _fiald = new string[] { "fldSubject" };
        //    string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
        //    string searchtext = string.Format(searchType[searchtype], value);
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblLetterSelect(_fiald[Convert.ToInt32(field)], searchtext, top,1,"").ToList();
        //    return Json(q, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblHistoryLetterDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                    return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });
                }
                else
                {
                    return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
                }

            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                Int64 fldID = 0;
                Int64 fldOrderId = 0;
                int fldYear = 0;
                string fldDesc = "";
                string fldLetterDate = "";
                string fldCreatedDate = "";
                string fldLetterNumber = "";
                string fldSubject = "";
                string fldKeywords = "";
                int? fldLetterStatusID = 0;
                int fldComisionID = 0;
                string fldImmediacyID = "";
                int fldSecurityTypeID = 0;
                int fldLetterTypeID = 0;

                //ویرایش جدول LetterBox
                Int64 fldIDLetterBox = 0;
                int fldBoxIDLetterBox = 0;
                string fldDescLetterBox = "";

                //ویرایش جدول ارجاعات
                Int64 fldIDAssignment = 0;
                Int64 fldLetterIDAssignment = 0;
                string fldAssignmentDateAssignment = "";
                string fldAnswerDateAssignment = "";
                Int64 fldSourceAssIdAssignment = 0;
                string fldDescAssignment = "";

                //ویرایش ارجاعات  داخلی فرستنده
                Int64 fldIDInternalAssignmentSender = 0;
                Int64 fldAssignmentIDInternalAssignmentSender = 0;
                int fldSenderComisionIDInternalAssignmentSender = 0;
                int fldBoxIDInternalAssignmentSender = 0;
                string fldDescInternalAssignmentSender = "";

                //ویرایش ارجاعات داخلی گیرنده
                Int64 fldIDInternalAssignmentReceiver = 0;
                Int64 fldAssignmentIDInternalAssignmentReceiver = 0;
                int fldReceiverComisionIDInternalAssignmentReceiver = 0;
                int fldAssignmentStatusIDInternalAssignmentReceiver = 0;
                int fldAssignmentTypeIDInternalAssignmentReceiver = 0;
                int fldBoxIDInternalAssignmentReceiver = 0;
                string fldLetterReadDateInternalAssignmentReceiver = "";
                Boolean fldShowTypeT_FInternalAssignmentReceiver = false;
                string fldDescInternalAssignmentReceiver = "";

                //ویرایش نامه های داخلی فرستنده
                Int64 fldIDInternalLetterReceiver = 0;
                Int64 fldLetterIDInternalLetterReceiver = 0;
                int fldReceiverComisionIDInternalLetterReceiver = 0;
                int fldAssignmentStatusIDInternalLetterReceiver = 0;
                string fldDescInternalLetterReceiver = "";

                //ویرایش پیگیری نامه
                int fldIDLetterFollow = 0;
                Int64 fldLetterIDLetterFollow = 0;
                string fldLetterTextLetterFollow = "";
                string fldDescLetterFollow = "";

                //ویرایش سوابق نامه
                Int64 fldIdHistoryLetter = 0;
                Int64 fldCurrentLetter_IdHistoryLetter = 0;
                int fldHistoryType_IdHistoryLetter = 0;
                Int64 fldHistoryLetter_IdHistoryLetter = 0;
                string fldDescHistoryLetter = "";

                //ویرایش محتوای نامه
                Int64? fldIDContentFile = 0;
                string fldNameContentFile = "";
                byte fldLetterTextContentFile = 0;
                int? fldLetterPatternIDContentFile = 0;
                string fldDescContentFile = "";
                Int64? fldLetterIDContentFile = 0;
                //var HistoryLetter = p.sp_tblHistoryLetterSelect("fldId", id.ToString(), 1, 1, "").FirstOrDefault();
                var Letter = p.sp_tblLetterSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                string GirandeId = "";
                string GirandeName = "";
                string SignerId = "";
                string SignerName = "";
                string RoneveshtId = "";
                string RoneveshtName = "";
                string RoneveshtText = "";
                string RoneveshtAssTypeId = "";
                byte fldSignType = 0;
                if (Letter != null)
                {
                    fldID = Letter.fldID;
                    fldOrderId = Letter.fldOrderId;
                    fldYear = Letter.fldYear;
                    fldDesc = Letter.fldDesc;
                    fldLetterDate = Letter.fldLetterDate;
                    fldCreatedDate = Letter.fldCreatedDate;
                    fldLetterNumber = Letter.fldLetterNumber;
                    fldSubject = Letter.fldSubject;
                    fldKeywords = Letter.fldKeywords;
                    fldLetterStatusID = Letter.fldLetterStatusID;
                    fldComisionID = Letter.fldComisionID;
                    Session["ComId"] = fldComisionID;
                    fldImmediacyID = Letter.fldImmediacyID;
                    fldSecurityTypeID = Letter.fldSecurityTypeID;
                    fldLetterTypeID = Letter.fldLetterTypeID;
                    fldSignType = Letter.fldSignType;
                    var girande = p.sp_tblLetterReciversSelect(fldID).ToList();
                    foreach (var item in girande)
                    {
                        GirandeId += item.fldReceiverComisionID + "|" + item.fldtype + ";";
                        GirandeName += item.fldReceiverComisionName + ";";
                    }
                    var signer = p.sp_LetterSignerSelect(fldID).ToList();
                    foreach (var item in signer)
                    {
                        SignerName += item.fldSignerName + ";";
                        SignerId += item.fldSignerComisionID + ";";
                    }
                    var ronevesht = p.sp_tblLetterRoneveshtSelect(fldID).ToList();

                    foreach (var item in ronevesht)
                    {
                        RoneveshtId += item.fldComID + "|" + item.fldType + ";";
                        RoneveshtName += item.fldNameStaff + ";";
                        RoneveshtAssTypeId += item.fldAssignmentTypeId + ";";
                        RoneveshtText += item.fldText + ";";
                    }
                }

                var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (LetterBox != null)
                {
                    fldIDLetterBox = LetterBox.fldID;
                    fldBoxIDLetterBox = LetterBox.fldBoxID;
                    fldDescLetterBox = LetterBox.fldDesc;
                }
                var Assignment = p.sp_tblAssignmentSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Assignment != null)
                {
                    fldIDAssignment = Assignment.fldID;
                    fldLetterIDAssignment = Assignment.fldLetterID;
                    fldAssignmentDateAssignment = Assignment.fldAssignmentDate;
                    fldAnswerDateAssignment = Assignment.fldAnswerDate;
                    fldSourceAssIdAssignment = Convert.ToInt64(Assignment.fldSourceAssId);
                    fldDescAssignment = Assignment.fldDesc;
                }
                if (Assignment != null)
                {
                    var InternalAssignmentSender = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", Assignment.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    if (InternalAssignmentSender != null)
                    {
                        fldIDInternalAssignmentSender = InternalAssignmentSender.fldID;
                        fldAssignmentIDInternalAssignmentSender = InternalAssignmentSender.fldAssignmentID;
                        fldSenderComisionIDInternalAssignmentSender = InternalAssignmentSender.fldSenderComisionID;
                        fldBoxIDInternalAssignmentSender = InternalAssignmentSender.fldBoxID;
                        fldDescInternalAssignmentSender = InternalAssignmentSender.fldDesc;
                    }

                    var InternalAssignmentReceiver = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", Assignment.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    if (InternalAssignmentReceiver != null)
                    {
                        fldIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldID;
                        fldAssignmentIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldAssignmentID;
                        fldReceiverComisionIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldReceiverComisionID;
                        fldAssignmentStatusIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldAssignmentStatusID;
                        fldAssignmentTypeIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldAssignmentTypeID;
                        fldBoxIDInternalAssignmentReceiver = InternalAssignmentReceiver.fldBoxID;
                        fldLetterReadDateInternalAssignmentReceiver = InternalAssignmentReceiver.fldLetterReadDate;
                        fldShowTypeT_FInternalAssignmentReceiver = InternalAssignmentReceiver.fldShowTypeT_F;
                        fldDescInternalAssignmentReceiver = InternalAssignmentReceiver.fldDesc;
                    }
                }
                var InternalLetterReceiver = p.sp_tblInternalLetterReceiverSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (InternalLetterReceiver != null)
                {
                    fldIDInternalLetterReceiver = InternalLetterReceiver.fldID;
                    fldLetterIDInternalLetterReceiver = InternalLetterReceiver.fldLetterID;
                    fldReceiverComisionIDInternalLetterReceiver = InternalLetterReceiver.fldReceiverComisionID;
                    fldAssignmentStatusIDInternalLetterReceiver = InternalLetterReceiver.fldAssignmentStatusID;
                    fldDescInternalLetterReceiver = InternalLetterReceiver.fldDesc;
                }
                var LetterFollow = p.sp_tblLetterFollowSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (LetterFollow != null)
                {
                    fldIDLetterFollow = LetterFollow.fldID;
                    fldLetterIDLetterFollow = Convert.ToInt64(LetterFollow.fldLetterID);
                    fldLetterTextLetterFollow = LetterFollow.fldLetterText;
                    fldDescLetterFollow = LetterFollow.fldDesc;
                }
                var HistoryLetter = p.sp_tblHistoryLetterSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (HistoryLetter != null)
                {
                    fldIdHistoryLetter = HistoryLetter.fldId;
                    fldCurrentLetter_IdHistoryLetter = HistoryLetter.fldCurrentLetter_Id;
                    fldHistoryType_IdHistoryLetter = HistoryLetter.fldHistoryType_Id;
                    fldHistoryLetter_IdHistoryLetter = HistoryLetter.fldHistoryLetter_Id;
                    fldDescHistoryLetter = HistoryLetter.fldDesc;
                }

                var ContentFile = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (ContentFile != null)
                {
                    fldIDContentFile = ContentFile.fldID;
                    fldNameContentFile = ContentFile.fldName;
                    //fldLetterTextContentFile = ContentFile.fldLetterText;
                    fldLetterPatternIDContentFile = ContentFile.fldLetterPatternID;
                    fldDescContentFile = ContentFile.fldDesc;
                    fldLetterIDContentFile = ContentFile.fldLetterID;
                }

                return Json(new
                {//ویرایش کل نامه
                    //ویرایش جدول نامه
                    fldID = fldID,
                    fldYear = fldYear,
                    fldOrderId = fldOrderId,
                    fldGirandeId=GirandeId,
                    fldGirandeName=GirandeName,
                    fldSignerName = SignerName,
                    fldSignerId = SignerId,
                    RoneveshtId = RoneveshtId,
                    RoneveshtName = RoneveshtName,
                    RoneveshtText = RoneveshtText,
                    RoneveshtAssTypeId = RoneveshtAssTypeId,
                    fldDesc = fldDesc,
                    fldSignType = fldSignType,
                    fldLetterDate = fldLetterDate,
                    fldCreatedDate = fldCreatedDate,
                    fldLetterNumber = fldLetterNumber,
                    fldSubject = fldSubject,
                    fldKeywords = fldKeywords,
                    fldLetterStatusID = fldLetterStatusID,
                    fldComisionID = fldComisionID,
                    fldImmediacyID = fldImmediacyID,
                    fldSecurityTypeID = fldSecurityTypeID,
                    fldLetterTypeID = fldLetterTypeID,

                    //ویرایش جدول LetterBox
                    fldIDLetterBox = fldIDLetterBox,
                    fldBoxIDLetterBox = fldBoxIDLetterBox,
                    fldDescLetterBox = fldDescLetterBox,

                    //ویرایش جدول ارجاعات
                    fldIDAssignment = fldIDAssignment,
                    fldLetterIDAssignment = fldLetterIDAssignment,
                    fldAssignmentDateAssignment = fldAssignmentDateAssignment,
                    fldAnswerDateAssignment = fldAnswerDateAssignment,
                    fldSourceAssIdAssignment = fldSourceAssIdAssignment,
                    fldDescAssignment = fldDescAssignment,

                    //ویرایش ارجاعات  داخلی فرستنده
                    fldIDInternalAssignmentSender = fldIDInternalAssignmentSender,
                    fldAssignmentIDInternalAssignmentSender = fldAssignmentIDInternalAssignmentSender,
                    fldSenderComisionIDInternalAssignmentSender = fldSenderComisionIDInternalAssignmentSender,
                    fldBoxIDInternalAssignmentSender = fldBoxIDInternalAssignmentSender,
                    fldDescInternalAssignmentSender = fldDescInternalAssignmentSender,

                    //ویرایش ارجاعات داخلی گیرنده
                    fldIDInternalAssignmentReceiver = fldIDInternalAssignmentReceiver,
                    fldAssignmentIDInternalAssignmentReceiver = fldAssignmentIDInternalAssignmentReceiver,
                    fldReceiverComisionIDInternalAssignmentReceiver = fldReceiverComisionIDInternalAssignmentReceiver,
                    fldAssignmentStatusIDInternalAssignmentReceiver = fldAssignmentStatusIDInternalAssignmentReceiver,
                    fldAssignmentTypeIDInternalAssignmentReceiver = fldAssignmentTypeIDInternalAssignmentReceiver,
                    fldBoxIDInternalAssignmentReceiver = fldBoxIDInternalAssignmentReceiver,
                    fldLetterReadDateInternalAssignmentReceiver = fldLetterReadDateInternalAssignmentReceiver,
                    fldShowTypeT_FInternalAssignmentReceiver = fldShowTypeT_FInternalAssignmentReceiver,
                    fldDescInternalAssignmentReceiver = fldDescInternalAssignmentReceiver,

                    //ویرایش نامه های داخلی فرستنده
                    fldIDInternalLetterReceiver = fldIDInternalLetterReceiver,
                    fldLetterIDInternalLetterReceiver = fldLetterIDInternalLetterReceiver,
                    fldReceiverComisionIDInternalLetterReceiver = fldReceiverComisionIDInternalLetterReceiver,
                    fldAssignmentStatusIDInternalLetterReceiver = fldAssignmentStatusIDInternalLetterReceiver,
                    fldDescInternalLetterReceiver = fldDescInternalLetterReceiver,

                    //ویرایش پیگیری نامه
                    fldIDLetterFollow = fldIDLetterFollow,
                    fldLetterIDLetterFollow = fldLetterIDLetterFollow,
                    fldLetterTextLetterFollow = fldLetterTextLetterFollow,
                    fldDescLetterFollow = fldDescLetterFollow,

                    //ویرایش سوابق نامه
                    fldIdHistoryLetter = fldIdHistoryLetter,
                    fldCurrentLetter_IdHistoryLetter = fldCurrentLetter_IdHistoryLetter,
                    fldHistoryType_IdHistoryLetter = fldHistoryType_IdHistoryLetter,
                    fldHistoryLetter_IdHistoryLetter = fldHistoryLetter_IdHistoryLetter,
                    fldDescHistoryLetter = fldDescHistoryLetter,

                    //ویرایش محتوای نامه
                    fldIDContentFile = fldIDContentFile,
                    fldNameContentFile = fldNameContentFile,
                    fldLetterTextContentFile = fldLetterTextContentFile,
                    fldLetterPatternIDContentFile = fldLetterPatternIDContentFile,
                    fldDescContentFile = fldDescContentFile,
                    fldLetterIDContentFile = fldLetterIDContentFile


                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public JsonResult Details1(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblLetterAttachmentSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldName = q.fldName,
                    fldDesc = q.fldDesc,
                    fldContentFileID=q.fldContentFileID

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult GetHistoryType()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblHistoryTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public FileContentResult Image(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();
            var letterAttachment = p.sp_tblLetterAttachmentSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var pic = p.sp_tblContentFileSelect("fldID", letterAttachment.fldContentFileID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldLetterText != null)
                {
                    return File((byte[])pic.fldLetterText, "jpg");
                }
            }
            return null;

        }

        public ActionResult Distribute(Models.ExternalLetterAssignment ExternalLetterAssignment)
        {//توزیع نامه
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 86))
                {
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    var date = p.sp_GetDate().FirstOrDefault();

                    DateTime AnswerDate;
                    if (ExternalLetterAssignment.fldAssignmentAnswerDate != null)
                        AnswerDate = MyLib.Shamsi.Shamsi2miladiDateTime(ExternalLetterAssignment.fldAssignmentAnswerDate);
                    else
                        AnswerDate = date.fldDateTime;

                    if (ExternalLetterAssignment.fldRoneveshId == null)
                        ExternalLetterAssignment.fldRoneveshId = "";
                    if (ExternalLetterAssignment.AssignmentTypeId == null)
                        ExternalLetterAssignment.AssignmentTypeId = "";
                    var AssignmentType = ExternalLetterAssignment.AssignmentTypeId.Split(';');
                    
                    var Recivers = ExternalLetterAssignment.fldReceiverComisionID.Split(';');
                    var Ronevesht = ExternalLetterAssignment.fldRoneveshId.Split(';');
                    //var g = ExternalLetterAssignment.fldInternalAssignmentSenderComision.Split(';');
                    if (ExternalLetterAssignment.fldDesc == null)
                        ExternalLetterAssignment.fldDesc = "";

                    //for (int i = 0; i < Recivers.Count() - 1; i++)
                    //{
                    //    var MailReciver = Recivers[i].Split('|');
                    //    Session["ServerPath"] = Server.MapPath(@"~\Uploaded\");
                    //    var c = p.sp_tblCommisionSelect("fldId", MailReciver[0].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    //    var email = p.sp_tblEmailSelect("fldStaffID",c.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    //    if (email != null)
                    //        if (email.fldSendTrue_False == true)
                    //            SendEmail.send(MailReciver[0].ToString(), ExternalLetterAssignment.fldLetterID);
                    //}

                    if (ExternalLetterAssignment.fldID == 0)
                    {//ثبت رکورد جدید 
                        var status = p.sp_tblLetterSelect("fldId", ExternalLetterAssignment.fldLetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (status.fldLetterStatusID != 4)//توزیع شده4
                        {
                            System.Data.Objects.ObjectParameter _idAssignment = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            byte[] _file = null;
                            var IsLetterID = p.sp_tblAssignmentSelect("fldLetterID", ExternalLetterAssignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            //sabte erja
                            if (IsLetterID != null)
                            {// در صورتیکه این نامه قبلا ارجاع داده شده است
                                var ParentAssignmentID = ExternalLetterAssignment.fldAssignmentID;
                                p.sp_tblAssignmentInsert(_idAssignment, ExternalLetterAssignment.fldLetterID, AnswerDate, ParentAssignmentID, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

                                var BoxSendID = p.sp_tblBoxSelect("fldComisionID", ExternalLetterAssignment.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                                //ذخیره نامه در پوشه ارسال شده
                                //var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", LetterAssignment.fldLetterID.ToString(), 1, 1, "").FirstOrDefault();
                                //p.sp_tblLetterBoxUpdate(LetterBox.fldID, LetterAssignment.fldLetterID, BoxSendID.fldID, 1, "", "");
                                p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), ExternalLetterAssignment.fldComisionID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldDesc, Session["UserPass"].ToString());
                            }
                            else
                            {// در صورتیکه برای اولین بار نامه ارجاع داده می شود
                                p.sp_tblAssignmentInsert(_idAssignment, ExternalLetterAssignment.fldLetterID, AnswerDate, null, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

                                //دریافت کد باکس جهت ذخیره در کارتابل ارسال شده
                                var BoxSendID = p.sp_tblBoxSelect("fldComisionID", ExternalLetterAssignment.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                                //ذخیره نامه در پوشه ارسال شده
                                var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", ExternalLetterAssignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                p.sp_tblLetterBoxUpdate(LetterBox.fldID, ExternalLetterAssignment.fldLetterID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), ExternalLetterAssignment.fldComisionID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldDesc, Session["UserPass"].ToString());
                            }
                            //sabte erja
                            //p.sp_tblAssignmentInsert(_idAssignment, ExternalLetterAssignment.fldLetterID,null, 1, ExternalLetterAssignment.fldAssignmentDesc, "");
                            p.sp_tblLetterStatusIdUpdate(ExternalLetterAssignment.fldLetterID, 4);
                            //دریافت کد باکس جهت ذخیره در کارتابل ارسال شده
                            //var BoxSenderID = p.sp_tblBoxSelect("fldComisionID", ExternalLetterAssignment.fldComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                            //p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), ExternalLetterAssignment.fldComisionID, BoxSenderID.fldID, 1, "", "");

                            //ذخیره نامه در پوشه ارسال شده
                            //p.sp_tblLetterBoxUpdate(BoxSenderID.fldID, ExternalLetterAssignment.fldLetterID, BoxSenderID.fldID, 1, "", "");
                            //                    
                            for (int i = 0; i < Recivers.Count() - 1; i++)
                            {
                                var R = Recivers[i].Split('|');
                                if (R[1] != "2")
                                {
                                    var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", R[0].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                    p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), Convert.ToInt32(R[0]), 1, 2, BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]), "اصل", Session["UserPass"].ToString());
                                    var subStatiut = p.sp_tblSubstituteSelect("fldSenderComisionID", R[0], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                                    foreach (var item in subStatiut)
                                    {
                                        BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                        p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), item.fldReceiverComisionID, 1, 1, BoxCurrentID.fldID, null, false, Convert.ToInt32(Session["UserId"]), "اصل-تفویض شده", Session["UserPass"].ToString());
                                    }
                                }
                            }
                            for (int i = 0; i < Ronevesht.Count() - 1; i++)
                            {
                                var R = Ronevesht[i].Split('|');
                                if (R[1] != "2")
                                {
                                    var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", R[0].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                    p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), Convert.ToInt32(R[0]), 1, Convert.ToInt32(AssignmentType[i]), BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]), "رونوشت", Session["UserPass"].ToString());
                                    var subStatiut = p.sp_tblSubstituteSelect("fldSenderComisionID", R[0], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                                    foreach (var item in subStatiut)
                                    {
                                        BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                        p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), item.fldReceiverComisionID, 1, 1, BoxCurrentID.fldID, null, false, Convert.ToInt32(Session["UserId"]), "رونوشت-تفویض شده", Session["UserPass"].ToString());
                                    }
                                }
                            }
                            //if (ExternalLetterAssignment.fldName != null)
                            //{
                            //    if (Session["savePath"] != null)
                            //    {
                            //        MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                            //        string filename = Path.GetFileName(Session["savePath"].ToString());
                            //        System.IO.File.Delete(Session["savePath"].ToString());
                            //        Session.Remove("savePath");
                            //        _file = stream.ToArray();
                            //        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            //        //p.
                            //        p.sp_tblAssignmentAttachmentInsert(Convert.ToInt64(_id.Value), _file, ExternalLetterAssignment.fldName, 1, ExternalLetterAssignment.fldDesc, "");
                            //        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                            //    }
                            //    else
                            //        return Json(new { data = "لطفا فایل را وارد کنید.", state = 1 });
                            //}
                            return Json(new { data = "توزیع نامه با موفقیت انجام شد.", state = 0 });
                        }
                        else
                            return Json(new { data = "نامه قبلا توزیع شده است.", state = 1 });
                    }
                    else
                    {//ویرایش رکورد ارسالی
                        byte[] report_file = null;
                        if (Session["savePath"] != null)
                        {
                            //var q = p.sp_tblReportsSelect("fldOlgoLetterId", LetterAttachment.fldID.ToString(), 1).FirstOrDefault();
                            //MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                            //System.IO.File.Delete(Session["savePath"].ToString());
                            //Session.Remove("savePath");
                            //report_file = stream.ToArray();
                            //p.sp_tblOlgoGharardadUpdate(LetterAttachment.fldID, LetterAttachment.fldName, 1, LetterAttachment.fldDesc);
                            //p.sp_tblReportsUpdate(q.fldId, report_file, null, LetterAttachment.fldID, 1, LetterAttachment.fldDesc);
                            return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                        }
                        else
                            return Json(new { data = "لطفا فایل گزارش را وارد کنید.", state = 1 });
                    }
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult CheckHaveAss_Sign(int LetterID)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var sign = p.sp_tblSignerSelect("fldLetterID", LetterID.ToString(), 0).Where(k => k.fldFirstSigner != null).FirstOrDefault();
            int issign = 0;
            if (sign != null)
                issign = 1;
            var Ass = p.sp_tblAssignmentSelect("fldLetterID", LetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            int isAss = 0;
            if (Ass != null)
                isAss = 1;
            return Json(new { isAss = isAss, issign = issign }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckInternalP(string fldReceiverComisionID, string fldRoneveshId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            if (fldRoneveshId == null)
                fldRoneveshId = "";
            var HaveIn=0;
            var Recivers = fldReceiverComisionID.Split(';');
            var Ronevesht = fldRoneveshId.Split(';');

            for (int i = 0; i < Recivers.Count() - 1; i++)
            {
                var R = Recivers[i].Split('|');
                if (R[1] != "2")
                    HaveIn = 1;
            }
            for (int i = 0; i < Ronevesht.Count() - 1; i++)
            {
                var Ro = Ronevesht[i].Split('|');
                if (Ro[1] != "2")
                    HaveIn = 1;
            }
                
                return Json(new { In = HaveIn}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SenderDetails(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();

            var t = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var q = p.sp_tblCommisionSelect("fldId", t.fldSenderComisionID.ToString().ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                return Json(new { fldGirandeId = q.fldID+"|1;", fldGirandeName = q.fldStaffName+"("+q.fldOrganicRoleName+");" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSigner()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var date = p.sp_GetDate().FirstOrDefault();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblCommisionSelect("fldStaffID", user.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).FirstOrDefault();

            return Json(new { SignID = q.fldID + ";", SignName = q.fldStaffName + "(" + q.fldOrganicRoleName + ");" }, JsonRequestBehavior.AllowGet);
        }
    }
}
