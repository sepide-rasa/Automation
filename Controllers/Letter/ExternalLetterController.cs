using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;
using System.Drawing;
using Aspose.Words;

namespace Automation.Controllers.BasicInf.Operation
{
    
    public class ExternalLetterController : Controller
    {
        //
        // GET: /ExternalLetter/
         
        public ActionResult Index(int Id, int? Email_Fax_ECEId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 71))
            {
                ViewBag.SiteURL = "http://"+Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                ViewBag.State = Id;
                ViewBag.Email_Fax_ECEId = Email_Fax_ECEId;
                Session["State"] = Id;
                Session["Email_Fax_ECEId"] = Email_Fax_ECEId;
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult IndexTab(int Id, int? Email_Fax_ECEId, string HistoryLetter_Id, string ComisionID)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 71))
            {
                ViewBag.SiteURL = "http://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                ViewBag.State = Id;
                ViewBag.Email_Fax_ECEId = Email_Fax_ECEId;
                Session["State"] = Id;
                Session["Email_Fax_ECEId"] = Email_Fax_ECEId;
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
        public ActionResult PreviewLetterPDFBox()
        {
            return PartialView();
        }
        public ActionResult GeneratePDF(int? id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var letterContent = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (letterContent != null)
                    return File(letterContent.fldLetterText, "application/pdf");
                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
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
        [HttpPost]
        public ActionResult ScanedUpload(long id)
        {
            if (Request.Files.Count > 0)
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                string savePath = Server.MapPath(@"~\Uploaded\" + id + ".pdf");                
                Request.Files[0].SaveAs(savePath);
                MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(savePath));
                byte[] _File = stream.ToArray();
                var q = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 1, 1, "").FirstOrDefault();
                if (q == null)
                {
                    System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                    p.sp_tblContentFileInsert(_id, "", _File, null, id, 1,"", "","");
                    System.IO.File.Delete(savePath);
                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                }
                else
                {
                    System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                    p.sp_tblContentFileUpdate(q.fldID, "", _File, null, id, 1,"", "");
                    System.IO.File.Delete(savePath);
                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                }
            }
            return Json("",JsonRequestBehavior.AllowGet);
        }
        public ActionResult LetterContentSave(long id)
        {
            byte[] _File = null;
            string savePath="";

            if (id > 0 || Convert.ToInt32(Session["State"]) == 2 || Convert.ToInt32(Session["State"]) == 3)
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Convert.ToInt32(Session["State"]) == 2)
                {
                    var Content = p.sp_FaxTempSelect("fldId", Session["Email_Fax_ECEId"].ToString(), 0).FirstOrDefault();
                    _File = Content.fldFile;
                }
                else if (Convert.ToInt32(Session["State"]) == 3)
                {
                    var Content = p.sp_Email_TempSelect("fldId", Session["Email_Fax_ECEId"].ToString(), 0).FirstOrDefault();
                    _File = Content.fldBody;
                }
                else if (Convert.ToInt32(Session["State"]) == 4)
                {
                    var Content = p.sp_tblECE_TempSelect("fldId", Session["Email_Fax_ECEId"].ToString(), 0).FirstOrDefault();
                    _File = Content.fldContentFile;
                }
                else
                {
                    savePath = Session["savePath"].ToString();
                   
                    FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read);

                    _File = new byte[fs.Length];
                    fs.Read(_File, 0, Convert.ToInt32(fs.Length));
                    
                    fs.Close();

                }
                var q = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (q == null)
                {
                    System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                    p.sp_tblContentFileInsert(_id, "", _File, null, id, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                    if (Convert.ToInt32(Session["State"]) != 2 & Convert.ToInt32(Session["State"]) != 3 & Convert.ToInt32(Session["State"]) != 4)
                        System.IO.File.Delete(savePath);
                    Session.Remove("savePath");

                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                    p.sp_tblContentFileUpdate(q.fldID, "", _File, null, id, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    if (Convert.ToInt32(Session["State"]) != 2 & Convert.ToInt32(Session["State"]) != 3 & Convert.ToInt32(Session["State"]) != 4)
                    {
                        
                        System.IO.File.Delete(savePath);
                    }
                    Session.Remove("savePath");

                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 }, JsonRequestBehavior.AllowGet);
                }
            }
            else
                return Json(new { data = "ذخیره با موفقیت انجام نشد.", state = 1 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UploadLetterContent(HttpPostedFileBase UptLetterContent)
        {
            if (UptLetterContent != null)
            {
                // Some browsers send file names with full path.
                // We are only interested in the file name.
                var fileName = Path.GetFileName(UptLetterContent.FileName);
                
                string savePath = Server.MapPath(@"~\Uploaded\" + fileName);
                string savePath1 = Server.MapPath(@"~\Uploaded\" + fileName);
                // The files are not actually saved in this demo
                UptLetterContent.SaveAs(savePath);

                string[] Tfiles = System.IO.Directory.GetFiles(Server.MapPath(@"~\Uploaded\" ));
                Aspose.Pdf.Generator.Pdf pdf1 = new Aspose.Pdf.Generator.Pdf();
              
                    string pp = Path.GetExtension(savePath).Replace(".", "");

                    FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read);
                    
                    byte[] tmpBytes = new byte[fs.Length];
                    fs.Read(tmpBytes, 0, Convert.ToInt32(fs.Length));

                    MemoryStream mystream = new MemoryStream(tmpBytes);
                    fs.Close();
                    if (pp == "docx" || pp == "doc")
                    {
                        fileName = Path.GetFileNameWithoutExtension(savePath);
                        Document doc = new Document(savePath);
                        savePath = Server.MapPath(@"~\Uploaded\" + fileName + ".pdf");
                        doc.Save(savePath);
                       
                        if (System.IO.File.Exists(savePath1))
                            System.IO.File.Delete(savePath1);
                    }
                    else if (pp != "pdf")
                    {

                        fileName = Path.GetFileNameWithoutExtension(savePath);

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
                        savePath = Server.MapPath(@"~\Uploaded\" + fileName + ".pdf");
                        pdf1.Save(savePath);
                        if (System.IO.File.Exists(savePath1))
                            System.IO.File.Delete(savePath1);
                    }
                

                Session["savePath"] = savePath;

            }
            return Content("");
        }
        public ActionResult RemoveLetterContent(string fileNames)
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

        public ActionResult ReloadLetter(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldLetterNumber", "fldSubject" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReloadLetterAttach(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldLetterID_Attach" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterAttachmentSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l=>l.fldName!="").ToList();
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

        public ActionResult Khateme(Models.LetterFollow LetterFollow)
        {//ذخیره آخرین پیگیری نامه


            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 91))
                {
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    if (LetterFollow.fldDesc == null)
                        LetterFollow.fldDesc = "";
                    //if (LetterFollow.fldID == 0)
                    //{//ثبت رکورد جدید

                    p.sp_tblLetterFollowInsert(LetterFollow.fldLetterText, LetterFollow.fldLetterID, Convert.ToInt32(Session["UserId"]), LetterFollow.fldDesc, Session["UserPass"].ToString());

                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0, LetterID = LetterFollow.fldLetterID });
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
                return Json(new { data = x.InnerException.Message, state = 1, LetterID = LetterFollow.fldLetterID });
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
                    if (Session["savePath"] != null & Convert.ToInt32(Session["State"]) != 3 & Convert.ToInt32(Session["State"]) != 4)
                    {
                        MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                        //if (stream.Length < 5242880)
                        //{
                            string filename = Path.GetFileName(Session["savePath"].ToString());
                            System.IO.File.Delete(Session["savePath"].ToString());
                            _file = stream.ToArray();
                            System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            p.sp_tblContentFileInsert(_id, filename, _file, null, LetterAttachment.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                            p.sp_tblLetterAttachmentInsert(LetterAttachment.fldLetterID, LetterAttachment.fldName/*filename*/, Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), LetterAttachment.fldDesc, Session["UserPass"].ToString());
                            Session.Remove("savePath");
                            return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                        //}
                        //else
                        //{
                        //    System.IO.File.Delete(Session["savePath"].ToString());
                        //    Session.Remove("savePath");
                        //    return Json(new { data = "حجم فایل پیوست بیشتر از 5 مگابایت می باشد.", state = 1 });
                        //}
                    }

                    else if (Convert.ToInt32(Session["State"]) == 3)
                    {
                        var Content = p.sp_tblEmailAttachement_TempSelect("fldEmail_TempId", Session["Email_Fax_ECEId"].ToString(), 0).FirstOrDefault();
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        p.sp_tblContentFileInsert(_id, Content.fldAttachementName, Content.fldAttachementBody, null, LetterAttachment.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                        p.sp_tblLetterAttachmentInsert(LetterAttachment.fldLetterID, Content.fldAttachementName, Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), LetterAttachment.fldDesc, Session["UserPass"].ToString());
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }

                    else if (Convert.ToInt32(Session["State"]) == 4)
                    {
                        var Content = p.sp_tblECE_Attach_TempSelect("fldECE_TempId", Session["Email_Fax_ECEId"].ToString(), 0).FirstOrDefault();
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        p.sp_tblContentFileInsert(_id, Content.fldName, Content.fldFile, null, LetterAttachment.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                        p.sp_tblLetterAttachmentInsert(LetterAttachment.fldLetterID, Content.fldName, Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), LetterAttachment.fldDesc, Session["UserPass"].ToString());
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
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
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 89))
                {
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

                        if (Letter.fldLetterSenderId == null)
                            return Json(new { data = "لطفا فرستنده نامه را مشخص کنید.", state = 1 });

                        var g = Letter.fldExternalLetterReceiverExternalPartnerID.Split(';');
                        var sender = Letter.fldLetterSenderId.Split(';');
                        string[] roonevesht = null;
                        string[] rooneveshtAssTypeId = null;
                        string[] rooneveshtAssDesc = null;
                        if (Letter.fldRooneveshtID != null)
                        {
                            roonevesht = Letter.fldRooneveshtID.Split(';');
                            rooneveshtAssTypeId = Letter.fldRooneveshtAssTypeID.Split(';');
                            rooneveshtAssDesc = Letter.fldRooneveshtAssDesc.Split(';');
                        }

                        p.sp_tblLetterInsert(_Letterid, Convert.ToInt32(Session["Year"]), _LetterOrderid, Letter.fldSubject, Letter.fldLetterNumber, MyLib.Shamsi.Shamsi2miladiDateTime(Letter.fldLetterDate), Letter.fldKeywords, 1, Letter.fldComisionID, Letter.fldImmediacyID, Letter.fldSecurityTypeID, 2, 0, Convert.ToInt32(Session["UserId"]), Letter.fldDesc, Session["UserPass"].ToString());
                        for (int i = 0; i < g.Count() - 1; i++)
                        {
                            p.sp_tblInternalLetterReceiverInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(g[i]), null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        }
                        var BoxID = p.sp_tblBoxSelect("fldComisionID", Letter.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 3).FirstOrDefault();
                        p.sp_tblLetterBoxInsert(_IDLetterBox, Convert.ToInt64(_Letterid.Value), BoxID.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        for (int j = 0; j < sender.Length - 1; j++)
                        {
                            p.sp_tblExternalLetterSenderInsert(Convert.ToInt64(_Letterid.Value), Convert.ToInt32(sender[j]), Convert.ToInt32(Session["UserId"]), "");
                        }
                        if (roonevesht != null)
                        {
                            for (int k = 0; k < roonevesht.Length - 1; k++)
                            {
                                var R = roonevesht[k].Split('|');
                                p.sp_tblRoneveshtInsert(Convert.ToInt64(_Letterid.Value), null, Convert.ToInt32(R[0]), Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
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
                            CreatorId = q.fldComisionID,
                            LetterDate = q.fldLetterDate,
                            LetterNumber = q.fldLetterNumber
                        });
                    }
                    else
                    {//ویرایش رکورد ارسالی
                        var IsTozi = p.sp_tblLetterSelect("fldID", Letter.fldID.ToString(), 0, 1, "").FirstOrDefault();
                        if (IsTozi.fldLetterStatusID != 4)
                        {
                            System.Data.Objects.ObjectParameter _Letterid = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            System.Data.Objects.ObjectParameter _LetterOrderid = new System.Data.Objects.ObjectParameter("fldOrderId", typeof(int));
                            System.Data.Objects.ObjectParameter _IDLetterBox = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            if (Letter.fldExternalLetterReceiverExternalPartnerID == null)
                                return Json(new { data = "لطفا گیرنده نامه را مشخص کنید.", state = 1 });

                            if (Letter.fldLetterSenderId == null)
                                return Json(new { data = "لطفا فرستنده نامه را مشخص کنید.", state = 1 });

                            var g = Letter.fldExternalLetterReceiverExternalPartnerID.Split(';');
                            var sender = Letter.fldLetterSenderId.Split(';');
                            string[] roonevesht = null;
                            string[] rooneveshtAssTypeId = null;
                            string[] rooneveshtAssDesc = null;
                            if (Letter.fldRooneveshtID != null)
                            {
                                roonevesht = Letter.fldRooneveshtID.Split(';');
                                rooneveshtAssTypeId = Letter.fldRooneveshtAssTypeID.Split(';');
                                rooneveshtAssDesc = Letter.fldRooneveshtAssDesc.Split(';');
                            }

                            p.sp_tblLetterUpdate(Letter.fldID, Convert.ToInt32(Session["Year"]), 0, Letter.fldSubject, Letter.fldLetterNumber, MyLib.Shamsi.Shamsi2miladiDateTime(Letter.fldLetterDate), Letter.fldKeywords, 1, Letter.fldComisionID, Letter.fldImmediacyID, Letter.fldSecurityTypeID, 2, Letter.fldSignType, Convert.ToInt32(Session["UserId"]), Letter.fldDesc, Session["UserPass"].ToString());

                            p.sp_tblInternalLetterReceiverDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                            for (int i = 0; i < g.Count() - 1; i++)
                            {
                                p.sp_tblInternalLetterReceiverInsert(Letter.fldID, Convert.ToInt32(g[i]), null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                            }
                            p.sp_tblExternalLetterSenderDelete(Letter.fldID, Convert.ToInt32(Session["UserId"]));
                            for (int j = 0; j < sender.Length - 1; j++)
                            {
                                p.sp_tblExternalLetterSenderInsert(Letter.fldID, Convert.ToInt32(sender[j]), Convert.ToInt32(Session["UserId"]), "");
                            }
                            p.sp_tblRoneveshtDelete(Letter.fldID, 1);
                            if (roonevesht != null)
                            {
                                for (int k = 0; k < roonevesht.Length - 1; k++)
                                {
                                    var R = roonevesht[k].Split('|');
                                    p.sp_tblRoneveshtInsert(Convert.ToInt64(_Letterid.Value), null, Convert.ToInt32(R[0]), Convert.ToInt32(rooneveshtAssTypeId[k]), rooneveshtAssDesc[k], Convert.ToInt32(Session["UserId"]), "");
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
                            return Json(new { data = "نامه توزیع شده و قابل تغییر نمی باشد.", state = 1 });
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
                    Car.sp_tblLetterDelete(Convert.ToInt32(id), 1, "");
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
        public ActionResult DeleteSavabegh(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblHistoryLetterDelete(Convert.ToInt32(id), 1, "");
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
        public ActionResult DeleteAttach(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    var q=p.sp_tblLetterAttachmentSelect("fldId", id, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                    foreach (var item in q)
                    {
                        p.sp_tblLetterAttachmentDelete(item.fldID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                        p.sp_tblContentFileDelete(Convert.ToInt32(item.fldContentFileID), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                    }
                    
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
                string SenderId = "";
                string SenderName = "";
                string RoneveshtId = "";
                string RoneveshtName = "";
                string RoneveshtText = "";
                string RoneveshtAssTypeId = "";
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
                    var girande = p.sp_tblLetterReciversSelect(fldID).ToList();
                    foreach (var item in girande)
                    {
                        GirandeId += item.fldReceiverComisionID + ";";
                        GirandeName += item.fldReceiverComisionName + ";";
                    }
                    var sender = p.sp_tblExternalLetterSenderSelect("fldLetterID", fldID.ToString(), 0).ToList();
                    foreach (var item in sender)
                    {
                        SenderName += item.fldName + ";";
                        SenderId += item.fldExternalPartnerID + ";";
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
                    fldGirandeId = GirandeId,
                    fldGirandeName = GirandeName,
                    fldSenderName = SenderName,
                    fldSenderId = SenderId,
                    RoneveshtId = RoneveshtId,
                    RoneveshtName = RoneveshtName,
                    RoneveshtText = RoneveshtText,
                    RoneveshtAssTypeId = RoneveshtAssTypeId,
                    fldDesc = fldDesc,
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
                    fldContentFileID = q.fldContentFileID

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult Distribute(Models.ExternalLetterAssignment ExternalLetterAssignment)
        {//توزیع نامه
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 90))
                {
                    if (ExternalLetterAssignment.fldRoneveshId == null)
                        ExternalLetterAssignment.fldRoneveshId = "";
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    var Recivers = ExternalLetterAssignment.fldReceiverComisionID.Split(';');
                    var Ronevesht = ExternalLetterAssignment.fldRoneveshId.Split(';');
                    //var g = ExternalLetterAssignment.fldInternalAssignmentSenderComision.Split(';');
                    if (ExternalLetterAssignment.fldDesc == null)
                        ExternalLetterAssignment.fldDesc = "";
                    if (ExternalLetterAssignment.fldID == 0)
                    {//ثبت رکورد جدید 
                        var status = p.sp_tblLetterSelect("fldId", ExternalLetterAssignment.fldLetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (status.fldLetterStatusID != 4)//توزیع شده4
                        {
                            System.Data.Objects.ObjectParameter _idAssignment = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            byte[] _file = null;

                            var date = p.sp_GetDate().FirstOrDefault();
                            var IsLetterID = p.sp_tblAssignmentSelect("fldLetterID", ExternalLetterAssignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            //sabte erja
                            if (IsLetterID != null)
                            {// در صورتیکه این نامه قبلا ارجاع داده شده است
                                var ParentAssignmentID = ExternalLetterAssignment.fldAssignmentID;
                                p.sp_tblAssignmentInsert(_idAssignment, ExternalLetterAssignment.fldLetterID, date.fldDateTime, ParentAssignmentID, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

                                var BoxSendID = p.sp_tblBoxSelect("fldComisionID", ExternalLetterAssignment.fldComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                                //ذخیره نامه در پوشه ارسال شده
                                //var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", LetterAssignment.fldLetterID.ToString(), 1, 1, "").FirstOrDefault();
                                //p.sp_tblLetterBoxUpdate(LetterBox.fldID, LetterAssignment.fldLetterID, BoxSendID.fldID, 1, "", "");
                                p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), ExternalLetterAssignment.fldComisionID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldDesc, Session["UserPass"].ToString());
                            }
                            else
                            {// در صورتیکه برای اولین بار نامه ارجاع داده می شود
                                p.sp_tblAssignmentInsert(_idAssignment, ExternalLetterAssignment.fldLetterID, date.fldDateTime, null, Convert.ToInt32(Session["UserId"]), ExternalLetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

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
                            // var BoxSenderID = p.sp_tblBoxSelect("fldComisionID", ExternalLetterAssignment.fldComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                            //p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), ExternalLetterAssignment.fldComisionID, BoxSenderID.fldID, 1, "", "");

                            //ذخیره نامه در پوشه ارسال شده
                            //p.sp_tblLetterBoxUpdate(BoxSenderID.fldID, ExternalLetterAssignment.fldLetterID, BoxSenderID.fldID, 1, "", "");
                            //                    
                            for (int i = 0; i < Recivers.Count() - 1; i++)
                            {
                                var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", Recivers[i].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), Convert.ToInt32(Recivers[i]), 1, 2, BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                var subStatiut = p.sp_tblSubstituteSelect("fldSenderComisionID", Recivers[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                                foreach (var item in subStatiut)
                                {
                                    BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                    p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), item.fldReceiverComisionID, 1, 1, BoxCurrentID.fldID, null, false, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                }
                            }
                            for (int i = 0; i < Ronevesht.Count() - 1; i++)
                            {
                                var R = Ronevesht[i].Split('|');
                                var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", R[0].ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), Convert.ToInt32(R[0]), 1, 2, BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                var subStatiut = p.sp_tblSubstituteSelect("fldSenderComisionID", R[0], 0, 1, "").ToList();
                                foreach (var item in subStatiut)
                                {
                                    BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                                    p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), item.fldReceiverComisionID, 1, 1, BoxCurrentID.fldID, null, false, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
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
        public ActionResult CheckIsDistribute(int LetterID)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var sign = p.sp_tblLetterSelect("fldID", LetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            int IsDistribute = 0;
            if (sign.fldLetterStatusID == 4)
                IsDistribute = 1;

            return Json(new { IsDistribute = IsDistribute }, JsonRequestBehavior.AllowGet);
        }
    }
}
