using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using Automation.Models;
using System.Collections;
using Aspose.Words;
using Aspose.Words.Saving;
using System.IO;
using Aspose.Cells;
using System.Drawing;

namespace Automation.Controllers
{
    public class HistoryLetterController : Controller
    {
        //
        // GET: /HistoryLetter/

        public ActionResult Index(int Pid, int ChangeType, int LogID)
        {//بارگذاری صفحه اصلی 
            ViewBag.Pid = Pid;
            ViewBag.ChangeType = ChangeType;
            ViewBag.LogID = LogID;
            Session["Creator"] = 1;
            return PartialView();

        }
        public ActionResult GetSecurityTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblSecurityTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetComission()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var date = p.sp_GetDate().FirstOrDefault();

            var q = p.sp_tblCommisionSelect("fldId", Session["Creator"].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
               
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetImmediacyTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblImmediacySelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
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

        public ActionResult ReloadLetterAttach(string field, string StartDate, string EndDate, int LetterId, int CommisionId, int LogID)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var z = m.sp_tblLetterSelect("fldID", LetterId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["Creator"] = z.fldUserID;
            var q = m.sp_tblContentFileAnnex_LogSelect(LetterId, CommisionId).Where(k => k.fldLogID == LogID).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReloadSavabegh(string StartDate, string EndDate, int LetterId, int CommisionId, int LogID)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var z = m.sp_tblLetterSelect("fldID", LetterId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["Creator"] = z.fldUserID;
           var q = m.sp_tblHistoryLetter_logSelect(LetterId,CommisionId ).Where(k => k.fldLogID == LogID).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadStatusFinish(string StartDate, string EndDate, int LetterId, int CommisionId ,int LogID)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var z = m.sp_tblLetterSelect("fldID", LetterId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["Creator"] = z.fldUserID;
            var q = m.sp_tblLetterFollow_LogSelect(LetterId,CommisionId ).Where(k => k.fldLogID == LogID).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(int id, string Start, string End, int Sender, int AssId,int? PNode)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var z = m.sp_tblLetterSelect("fldID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["Creator"] = z.fldUserID;
            var UID = m.sp_tblCommisionSelect("fldID", Sender.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var cont = m.sp_tblContentFile_LogSelect("", id, Sender).FirstOrDefault();
            if (cont == null)
            {
                cont = m.sp_tblContentFile_LogSelect("", id, PNode).FirstOrDefault();
            }
            var Ass = m.sp_tblAssignment_LogSelect(AssId).FirstOrDefault();
            var Letter = m.sp_tblLetter_LogSelect("", id,Sender).Where(l => l.fldUserID == UID.StaffUserId).LastOrDefault();
            var R = m.sp_tblRonevesht_logSelect(id, Sender).Where(l => l.fldUserID == UID.StaffUserId).ToList();
            var S = m.sp_tblSigner_LogSelect("", id, Sender).Where(l => l.fldUserID == UID.StaffUserId).ToList();
            var A = m.sp_tblInternalLetterReceiver_LogSelect("", id, Sender).Where(l => l.fldUserID == UID.StaffUserId).ToList();
            var H = m.sp_tblExternalLetterReceiver_LogSelect("", id, Sender).Where(l => l.fldUserID == UID.StaffUserId).ToList();
            var Girande = "";
            var Ronevesht = "";
            var Signer = "";
            int i = S.Count - 1;
            int Ri = R.Count - 1;
            int Ai = A.Count - 1;
            int Hi = H.Count - 1;

            if (R.Count > 0)
                while (R[Ri].fldLogTypeID == 1 )
                {
                    Ronevesht = Ronevesht + R[Ri].گیرنده_رونوشت + ';';
                    Ri--;
                    if (Ri < 0)
                        break;
                }

            if (S.Count > 0)
                while (S[i].fldLogTypeID == 1)
                {
                    Signer = Signer + S[i].نام_امضا_کننده_نامه + ';';
                    i--;
                    if (i < 0)
                        break;
                }

            if (A.Count > 0)
                while (A[Ai].fldLogTypeID == 1)
                {
                    Girande = Girande + A[Ai].نام_دریافت_کننده + ';';
                    Ai--;
                    if (Ai < 0)
                        break;
                }

            if (H.Count > 0)
                while (H[Hi].fldLogTypeID == 1)
                {
                    Girande = Girande + H[Hi].نام_ارگان_خارجی + ';';
                    Hi--;
                    if (Hi < 0)
                        break;
                }

            return Json(new
            {
                Girande = Girande,
                Signer = Signer,
                Ronevesht = Ronevesht,
                SignType = Letter.fldLetterTypeID,
                Subject = Letter.عنوان_نامه,
                LetterDate = Ass.تاریخ_نامه,
                LetterNum = Ass.شماره_نامه,
                Date = Letter.تاریخ_ایجاد_نامه,
                Keywords = Letter.کلمه_کلیدی,
                ImmediacyType = Letter.fldImmediacyID,
                SecurityType = Letter.fldSecurityTypeID,
                LetterNumComp = Letter.شماره_ثبت_نامه

            }, JsonRequestBehavior.AllowGet);
        }
        //public FileContentResult prview(int id, string Start, string End, int Sender, int? PNode, int LogID)
        //{

        //    try
        //    {
        //        Models.AutomationEntities p = new Models.AutomationEntities();
        //        var q = p.sp_tblContentFile_LogSelect("", id, Sender).FirstOrDefault();
        //        if (q == null)
        //            q = p.sp_tblContentFile_LogSelect("", id, PNode).FirstOrDefault();

        //        string docPath = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + id.ToString() + ".docx";
        //        System.IO.File.WriteAllBytes(docPath.ToString(), q.متن_نامه);
        //        Document doc = new Document(docPath);

        //        ImageSaveOptions options = new ImageSaveOptions(SaveFormat.Jpeg);
        //        options.PageIndex = 0;
        //        options.PageCount = doc.PageCount;
        //        string picPath = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + id.ToString() + ".jpg";
        //        doc.Save(picPath, options);

        //        MemoryStream st = new MemoryStream(System.IO.File.ReadAllBytes(picPath));
        //        System.IO.File.Delete(picPath);
        //        System.IO.File.Delete(docPath);
        //        return File(st.ToArray(), "jpg");

        //    }
        //    catch
        //    {
        //        //doc.Close(Type.Missing, Type.Missing, Type.Missing);
        //        //app.Quit(Type.Missing, Type.Missing, Type.Missing);
        //        return null;
        //    }
        //}
        public ActionResult GenerateLetterPDF(int id, string Start, string End, int Sender, int? PNode, int LogID)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var Content = p.sp_tblContentFile_LogSelect("", id, Sender).Where(k => k.fldLogID == LogID).ToList();
                if (Content.Count == 0)
                    Content = p.sp_tblContentFile_LogSelect("", id, PNode).Where(k => k.fldLogID == LogID).ToList();

                var path = Server.MapPath(@"~\Uploaded\Letter");
                byte[] pdf = null;
                foreach (var Item in Content)
                {
                    if (Item.پسوند == ".docx")
                    {
                        System.IO.File.WriteAllBytes(path + ".docx", Item.متن_نامه);
                        Document doc = new Document(path + ".docx");
                        doc.Save(path + "." + "pdf");
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
        public ActionResult LetterDetails(int id, int Sender, int LogID, int AssId)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var z = m.sp_tblLetterSelect("fldID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["Creator"] = z.fldUserID;
            //var UID = m.sp_tblCommisionSelect("fldID", Sender.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            //var cont = m.sp_tblContentFile_LogSelect(Start, End, id, Sender).FirstOrDefault();
            //if (cont == null)
            //{
            //    cont = m.sp_tblContentFile_LogSelect(Start, End, id, PNode).FirstOrDefault();
            //}
            var Ass = m.sp_tblAssignment_LogSelect( AssId).FirstOrDefault();
            var Letter = m.sp_tblLetter_LogSelect("", id, Sender).Where(l => l.fldLogID == LogID).LastOrDefault();
            var R = m.sp_tblRonevesht_logSelect(id, Sender).ToList();
            var S = m.sp_tblSigner_LogSelect("", id, Sender).ToList();
            var A = m.sp_tblInternalLetterReceiver_LogSelect("", id, Sender).ToList();
            var H = m.sp_tblExternalLetterReceiver_LogSelect("", id, Sender).ToList();
            var Girande = "";
            var Ronevesht = "";
            var Signer = "";

            for (int i = S.Count; i > 0; i--)
            {
                if (S[i - 1].fldLogTypeID == 3)
                    break;
                else
                    Signer = Signer + S[i - 1].نام_امضا_کننده_نامه + ';';
                
            }
            for (int i = A.Count; i > 0; i--)
            {
                if (A[i - 1].fldLogTypeID == 3)
                    break;
                else
                    Girande = Girande + A[i - 1].نام_دریافت_کننده + ';';

            }
            for (int i = H.Count; i > 0; i--)
            {
                if (H[i - 1].fldLogTypeID == 3)
                    break;
                else
                    Girande = Girande + H[i - 1].نام_ارگان_خارجی + ';';

            }
            for (int i = R.Count; i > 0; i--)
            {
                if (R[i - 1].fldLogTypeID == 3)
                    break;
                else
                    Ronevesht = Ronevesht + R[i - 1].گیرنده_رونوشت + ';';

            }   

            //int i = S.Count - 1;
            //int Ri = R.Count - 1;
            //int Ai = A.Count - 1;
            //int Hi = H.Count - 1;

            //if (R.Count > 0)
            //    while (R[Ri].fldLogTypeID == 1)
            //    {
            //        Ronevesht = Ronevesht + R[Ri].گیرنده_رونوشت + ';';
            //        Ri--;
            //        if (Ri < 0)
            //            break;
            //    }

            //if (S.Count > 0)
            //    while (S[i].fldLogTypeID == 1)
            //    {
            //        Signer = Signer + S[i].نام_امضا_کننده_نامه + ';';
            //        i--;
            //        if (i < 0)
            //            break;
            //    }

            //if (A.Count > 0)
            //    while (A[Ai].fldLogTypeID == 1)
            //    {
            //        Girande = Girande + A[Ai].نام_دریافت_کننده + ';';
            //        Ai--;
            //        if (Ai < 0)
            //            break;
            //    }

            //if (H.Count > 0)
            //    while (H[Hi].fldLogTypeID == 1)
            //    {
            //        Girande = Girande + H[Hi].نام_ارگان_خارجی + ';';
            //        Hi--;
            //        if (Hi < 0)
            //            break;
            //    }

            return Json(new
            {
                Girande = Girande,
                Signer = Signer,
                Ronevesht = Ronevesht,
                SignType = Letter.fldSignType,
                Subject = Letter.عنوان_نامه,
                LetterDate = Ass.تاریخ_نامه,
                LetterNum = Ass.شماره_نامه,
                Date = Letter.تاریخ_ایجاد_نامه,
                Keywords = Letter.کلمه_کلیدی,
                ImmediacyType = Letter.fldImmediacyID,
                SecurityType = Letter.fldSecurityTypeID,
                LetterNumComp = Letter.شماره_ثبت_نامه

            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateAttachPDF(int id, int Sender, int LogID)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                var Content = p.sp_tblContentFileAnnex_LogSelect(id, Sender).Where(k => k.fldLogID == LogID).FirstOrDefault();

                string Ex = "";
                var path = Server.MapPath(@"~\Uploaded\" + Content.نام_فایل_پیوست);
                Ex = Path.GetExtension(path);
                if (Content.fldExt == ".docx")
                    Ex = ".docx";
                var pathName = Server.MapPath(@"~\Uploaded\" + Path.GetFileNameWithoutExtension(path));

                byte[] pdf = null;

                if (Ex == ".docx" || Ex == ".doc")
                {
                    System.IO.File.WriteAllBytes(path + ".docx", Content.متن_نامه);
                    Document doc = new Document(path + ".docx");
                    doc.Save(path + "." + "pdf");
                    pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                    System.IO.File.Delete(path + ".docx");
                    System.IO.File.Delete(path + ".pdf");
                }
                else if (Ex == ".xlsx" || Ex == ".xls")
                {
                    System.IO.File.WriteAllBytes(path + ".xls", Content.متن_نامه);
                    Workbook workbook = new Workbook(path + ".xls");
                    workbook.Save(path + "." + "pdf");
                    pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                    System.IO.File.Delete(path + ".xls");
                    System.IO.File.Delete(path + ".pdf");
                }
                else if (Ex == ".pdf")
                {
                    pdf = Content.متن_نامه;
                }
                else
                {
                    Aspose.Pdf.Generator.Pdf pdf1 = new Aspose.Pdf.Generator.Pdf();
                    System.IO.File.WriteAllBytes(path, Content.متن_نامه);

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
    }
}
