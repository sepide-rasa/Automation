using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.IO;
using System.Drawing;
using Aspose.Words;
using Aspose.Words.Saving;
using System.Web.Configuration;

namespace Automation.Controllers
{
    [Authorize]
    public class LetterContentController : Controller
    {
        //
        // GET: /Content/

        public ActionResult Index()
        {
            //ViewBag.Staffid = idStaff;int idStaff
            return PartialView();
            
        }

        public FileContentResult prview(int id)
        {

            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldLetterPatternID != null).FirstOrDefault();
                string docPath = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + id.ToString() + ".docx";
                System.IO.File.WriteAllBytes(docPath.ToString(), q.fldLetterText);
                Document doc = new Document(docPath);

                ImageSaveOptions options = new ImageSaveOptions(SaveFormat.Jpeg);
                options.PageIndex = 0;
                options.PageCount = doc.PageCount;
                string picPath=AppDomain.CurrentDomain.BaseDirectory + "docs\\" + id.ToString() + ".jpg";
                doc.Save(picPath, options);

                MemoryStream st = new MemoryStream(System.IO.File.ReadAllBytes(picPath));
                System.IO.File.Delete(picPath);
                System.IO.File.Delete(docPath);
                return File(st.ToArray(), "jpg");

            }
            catch
            {
                //doc.Close(Type.Missing, Type.Missing, Type.Missing);
                //app.Quit(Type.Missing, Type.Missing, Type.Missing);
                return null;
            }
        }

        public ActionResult ViewDoc(int id, int state, String Ronevesht)
        {
            ViewBag.Id = id;
            ViewBag.State = state;
            ViewBag.SiteURL = "http://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
            ViewBag.Ronevesht = Ronevesht;
            
            

            Session["ViewState"] = state;
            return PartialView();
        }

        public ActionResult PatternDoc(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblLetterPatternSelect("fldPatternID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Response.Write("Get Stream Successfully!");
            Response.Write("EDA_STREAMBOUNDARY");
            Response.BinaryWrite(q.fldPatternFile);
            Response.Write("EDA_STREAMBOUNDARY");
            return null;
        }
        
        public ActionResult EditDoc(int id)
        {
            try
            {
                
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Convert.ToInt32(Session["ViewState"]) == 3)
                {
                    var q = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())/*.Where(l => l.fldLetterPatternID != null)*/.FirstOrDefault();
                    var Letter = p.sp_tblLetterSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    bool SignType = true;
                    if (Letter.fldSignType == 2)
                        SignType = false;

                    //Application oWordApplic = new Application();
                    //Microsoft.Office.Interop.Word.Document oDoc = new Document();
                   // Models.AutomationEntities p = new Models.AutomationEntities();
                    var letter = p.sp_tblLetterSelect("fldid", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                    string LetterDate = "";
                    string LetterNumber = "";
                    if (letter.fldLetterDate != null)
                    {
                        LetterDate = letter.fldLetterDate.Substring(8, 2) + "/" + letter.fldLetterDate.Substring(5, 2) + "/" + letter.fldLetterDate.Substring(0, 4);
                        LetterNumber = letter.fldLetterNumber;
                    }
                    var Attach = p.sp_tblLetterAttachmentSelect("fldLetterID", letter.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                    string _Attach = "ندارد";
                    if (Attach != null)
                        _Attach = "دارد";

                    bool HaveAndicator = false;
                    if (letter.fldLetterNumber != null)
                        HaveAndicator = true;

                    object fileName = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + id.ToString() + ".docx";
                    System.IO.File.WriteAllBytes(fileName.ToString(), q.fldLetterText);
                    Aspose.Words.Document Doc = new Aspose.Words.Document(fileName.ToString());

                    Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(Doc);
                    
                    var MergeField = Doc.MailMerge.GetFieldNames();
                    foreach (var item in MergeField)
                    {
                        if (HaveAndicator)
                        {
                            if (item == "#تاریخ#")
                            {
                                builder.MoveToMergeField("#تاریخ#");
                                builder.Write(LetterDate);
                            }
                            if (item == "#شماره#")
                            {
                                builder.MoveToMergeField("#شماره#");
                                builder.Write(LetterNumber);
                            }
                            if (item == "#پیوست#")
                            {
                                builder.MoveToMergeField("#پیوست#");
                                builder.Write(_Attach);
                            }
                        }
                        string MergeFieldName = item.Substring(6);
                        if (MergeFieldName == "امضا1")
                        {
                            
                            var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 1 && h.fldFirstSigner != null).FirstOrDefault();
                            if (signer != null)
                            {
                                builder.MoveToMergeField("امضا1");
                                string signpath = Sign(signer, id, SignType);
                                builder.InsertImage(System.IO.File.ReadAllBytes(signpath));
                                System.IO.File.Delete(signpath);
                            }
                        }
                        else if (MergeFieldName == "امضا2")
                        {
                            
                            var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 2 && h.fldFirstSigner != null).FirstOrDefault();
                            if (signer != null)
                            {
                                builder.MoveToMergeField("امضا2");
                                string signpath = Sign(signer, id, SignType);
                                builder.InsertImage(System.IO.File.ReadAllBytes(signpath));
                                System.IO.File.Delete(signpath);
                            }
                        }
                        if (MergeFieldName == "امضا3")
                        {
                            
                            var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 3 && h.fldFirstSigner != null).FirstOrDefault();
                            if (signer != null)
                            {
                                builder.MoveToMergeField("امضا3");
                                string signpath = Sign(signer, id, SignType);
                                builder.InsertImage(System.IO.File.ReadAllBytes(signpath));
                                System.IO.File.Delete(signpath);
                            }
                        }
                        if (MergeFieldName == "امضا4")
                        {
                            
                            var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 4 && h.fldFirstSigner != null).FirstOrDefault();
                            if (signer != null)
                            {
                                builder.MoveToMergeField("امضا4");
                                string signpath = Sign(signer, id, SignType);
                                builder.InsertImage(System.IO.File.ReadAllBytes(signpath));
                                System.IO.File.Delete(signpath);
                            }
                        }
                        if (MergeFieldName == "امضا5")
                        {
                            
                            var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 5 && h.fldFirstSigner != null).FirstOrDefault();
                            if (signer != null)
                            {
                                builder.MoveToMergeField("امضا5");
                                string signpath = Sign(signer, id, SignType);
                                builder.InsertImage(System.IO.File.ReadAllBytes(signpath));
                                System.IO.File.Delete(signpath);
                            }
                        }     
                    }

                    Doc.Save(fileName.ToString(), Aspose.Words.SaveFormat.Docx);
                    
                    //object readOnly = false;
                    //object isVisible = true;
                    //object missing = System.Reflection.Missing.Value;

                    //oDoc = oWordApplic.Documents.Open(ref fileName, ref missing, ref readOnly,
                    //ref missing, ref missing, ref missing, ref missing, ref missing, ref missing,
                    //ref missing, ref missing, ref isVisible, ref missing, ref missing, ref missing, ref missing);

                    //oDoc.Activate();
                    //foreach (Microsoft.Office.Interop.Word.Shape s in oDoc.Shapes)
                    //{
                    //    foreach (Field myMergeField in s.TextFrame.TextRange.Fields)
                    //    {
                    //        //iTotalFields++;
                    //        Range rngFieldCode = myMergeField.Code;
                    //        String fieldText = rngFieldCode.Text;


                    //        // GET only MAILMERGE fields
                    //        if (fieldText.StartsWith(" MERGEFIELD"))
                    //        {
                    //            Int32 endMerge = fieldText.IndexOf("\\");
                    //            Int32 fieldNameLength = fieldText.Length - endMerge;
                    //            String fieldName = fieldText.Substring(11, endMerge - 11);


                    //            fieldName = fieldName.Trim();
                    //            if (fieldName == "امضا1")
                    //            {
                    //                myMergeField.Select();
                    //                var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 1 && h.fldFirstSigner != null).FirstOrDefault();

                    //                if (signer != null)
                    //                {
                    //                    string signpath = Sign(signer, id, SignType);

                    //                    Microsoft.Office.Interop.Word.InlineShape inlineShape = oWordApplic.Selection.InlineShapes.AddPicture(signpath);
                    //                    System.IO.File.Delete(signpath);
                    //                    inlineShape.ScaleWidth = 100.0F;
                    //                    inlineShape.ScaleHeight = 100.0F;
                    //                }
                    //            }
                    //            else if (fieldName == "امضا2")
                    //            {
                    //                myMergeField.Select();
                    //                var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 2 && h.fldFirstSigner != null).FirstOrDefault();

                    //                if (signer != null)
                    //                {
                    //                    string signpath = Sign(signer, id, SignType);

                    //                    Microsoft.Office.Interop.Word.InlineShape inlineShape = oWordApplic.Selection.InlineShapes.AddPicture(signpath);
                    //                    System.IO.File.Delete(signpath);
                    //                    inlineShape.ScaleWidth = 100.0F;
                    //                    inlineShape.ScaleHeight = 100.0F;
                    //                }
                    //            }
                    //            else if (fieldName == "امضا3")
                    //            {
                    //                myMergeField.Select();
                    //                var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 3 && h.fldFirstSigner != null).FirstOrDefault();

                    //                if (signer != null)
                    //                {
                    //                    string signpath = Sign(signer, id, SignType);

                    //                    Microsoft.Office.Interop.Word.InlineShape inlineShape = oWordApplic.Selection.InlineShapes.AddPicture(signpath);
                    //                    System.IO.File.Delete(signpath);
                    //                    inlineShape.ScaleWidth = 100.0F;
                    //                    inlineShape.ScaleHeight = 100.0F;
                    //                }
                    //            }
                    //            else if (fieldName == "امضا4")
                    //            {
                    //                myMergeField.Select();
                    //                var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 4 && h.fldFirstSigner != null).FirstOrDefault();

                    //                if (signer != null)
                    //                {
                    //                    string signpath = Sign(signer, id, SignType);

                    //                    Microsoft.Office.Interop.Word.InlineShape inlineShape = oWordApplic.Selection.InlineShapes.AddPicture(signpath);
                    //                    System.IO.File.Delete(signpath);
                    //                    inlineShape.ScaleWidth = 100.0F;
                    //                    inlineShape.ScaleHeight = 100.0F;
                    //                }
                    //            }
                    //            else if (fieldName == "امضا5")
                    //            {
                    //                myMergeField.Select();
                    //                var signer = p.sp_tblSignerSelect("fldLetterID", id.ToString(), 0).Where(h => h.fldIndexerID == 5 && h.fldFirstSigner != null).FirstOrDefault();

                    //                if (signer != null)
                    //                {
                    //                    string signpath = Sign(signer, id, SignType);

                    //                    Microsoft.Office.Interop.Word.InlineShape inlineShape = oWordApplic.Selection.InlineShapes.AddPicture(signpath);
                    //                    System.IO.File.Delete(signpath);
                    //                    inlineShape.ScaleWidth = 100.0F;
                    //                    inlineShape.ScaleHeight = 100.0F;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //oDoc.SaveAs(ref fileName, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing,
                    //    ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
                    //oDoc.Close();
                    //oWordApplic.Application.Quit();
                    MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(fileName.ToString()));
                    byte[] _File = stream.ToArray();
                    p.sp_tblContentFileUpdate(q.fldID, "", _File, q.fldLetterPatternID, q.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    System.IO.File.Delete(fileName.ToString());
                    
                    Session.Remove("ViewState");

                }
                var m = p.sp_tblContentFileSelect("fldLetterID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldLetterPatternID != null).FirstOrDefault();
                Response.Write("Get Stream Successfully!");
                Response.Write("EDA_STREAMBOUNDARY");
                Response.BinaryWrite(m.fldLetterText);
                Response.Write("EDA_STREAMBOUNDARY");
                return null;
            }
            catch (Exception x)
            {
                System.IO.File.WriteAllText("d:\\a.txt", x.Message);
                return null;
            }
        }

        private string Sign(Models.sp_tblSignerSelect signer,int id,bool SignType)
        {
            string FontName = WebConfigurationManager.AppSettings["FontName"];
            int FontSize = Convert.ToInt32(WebConfigurationManager.AppSettings["FontSize"]);
            int NameY = Convert.ToInt32(WebConfigurationManager.AppSettings["NameY"]);
            int CommY = Convert.ToInt32(WebConfigurationManager.AppSettings["CommY"]);

            int Name_AzTaraf = Convert.ToInt32(WebConfigurationManager.AppSettings["NameAzTaraf"]);
            int NameY_AzTaraf = Convert.ToInt32(WebConfigurationManager.AppSettings["NameYAzTaraf"]);
            int CommY_AzTaraf = Convert.ToInt32(WebConfigurationManager.AppSettings["CommYAzTaraf"]);

            string signpath = "";
            int height = 200, width = 250;
            Models.AutomationEntities p = new Models.AutomationEntities();
            if (signer.fldSignerComisionID == Convert.ToInt32(signer.fldFirstSigner))
            {
                var comision = p.sp_tblCommisionSelect("fldId", signer.fldFirstSigner.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var staffimage = p.sp_tblPictureSelect("fldStaffID", comision.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                MemoryStream str = new MemoryStream(staffimage.fldSignPicture);
                Bitmap bitmap1 = new Bitmap(str);
                System.Drawing.Image img = System.Drawing.Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "content\\images\\Blank.jpg");

                Bitmap bitmap2 = new Bitmap(img, 250, 200);
                //bitmap2.SetResolution(200, 200);
                
                Graphics graphicsMasterImage = Graphics.FromImage(bitmap2);

                graphicsMasterImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphicsMasterImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsMasterImage.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                //Set the alignment based on the coordinates 
                StringFormat stringformatWriteTextFormat = new StringFormat();
                stringformatWriteTextFormat.Alignment = StringAlignment.Center;

                //Set the font color
                Color colorStringColor = System.Drawing.ColorTranslator.FromHtml("#000000");
                graphicsMasterImage.DrawString(comision.fldStaffName, new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new System.Drawing.SolidBrush(colorStringColor),
                new System.Drawing.Point(115, /*45*/NameY), stringformatWriteTextFormat);
                graphicsMasterImage.DrawString(comision.fldOrganicRoleName, new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new System.Drawing.SolidBrush(colorStringColor),
                new System.Drawing.Point(115,/*75*/ CommY), stringformatWriteTextFormat);
                //Save the new image in a physical location
                signpath = AppDomain.CurrentDomain.BaseDirectory + "docs\\sign" + id + "_" + comision.fldStaffID + ".png";

                int w = 0, h = 0, w1 = 0, h1 = 0, w2 = 0, h2 = 0;
                if (bitmap1.Width > 250)
                {
                    width = bitmap1.Width;
                    w = width / 2 - (250 / 2);
                }
                else
                {
                    w = 115 - (bitmap1.Width / 2);
                }
                if (bitmap1.Height > 250)
                {
                    height = bitmap1.Height;
                    h = height / 2 - (250 / 2);
                }
                else
                {
                    h = 115 - (bitmap1.Height / 2);
                }

                if (bitmap1.Width > 250 && bitmap1.Height < 250)
                {
                    w1 = 0;
                    h1 = h;
                    w2 = w;
                    h2 = 0;
                }
                else if (bitmap1.Width > 250 && bitmap1.Height > 250)
                {
                    w1 = 0;
                    h1 = 0;
                    w2 = w;
                    h2 = h;
                }

                else if (bitmap1.Width < 250 && bitmap1.Height > 250)
                {
                    w1 = w;
                    h1 = 0;
                    w2 = 0;
                    h2 = h;
                }
                else if (bitmap1.Width < 250 && bitmap1.Height < 250)
                {
                    w1 = w;
                    h1 = h;
                    w2 = 0;
                    h2 = 0;
                }
                bitmap1.MakeTransparent(Color.White);
                bitmap2.MakeTransparent(Color.White);
                Bitmap img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(Color.Transparent);
                if (SignType)
                    g.DrawImage(bitmap1, new System.Drawing.Point(w1, h1));
                g.DrawImage(bitmap2, new System.Drawing.Point(w2, h2));
                
                img3.Save(signpath, System.Drawing.Imaging.ImageFormat.Png);

            }
            else
            {
                var comision1 = p.sp_tblCommisionSelect("fldId", signer.fldSignerComisionID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var comision2 = p.sp_tblCommisionSelect("fldId", signer.fldFirstSigner.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var staffimage = p.sp_tblPictureSelect("fldStaffID", comision2.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                MemoryStream str = new MemoryStream(staffimage.fldSignPicture);
                Bitmap bitmap1 = new Bitmap(str);
                System.Drawing.Image img = System.Drawing.Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "content\\images\\Blank.jpg");

                Bitmap bitmap2 = new Bitmap(img, 200, 200);
                Graphics graphicsMasterImage = Graphics.FromImage(bitmap2);

                graphicsMasterImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphicsMasterImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsMasterImage.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                //Set the alignment based on the coordinates 
                StringFormat stringformatWriteTextFormat = new StringFormat();
                stringformatWriteTextFormat.Alignment = StringAlignment.Center;

                //Set the font color
                Color colorStringColor = System.Drawing.ColorTranslator.FromHtml("#000000");

                graphicsMasterImage.DrawString(comision1.fldStaffName, new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new System.Drawing.SolidBrush(colorStringColor),
                new System.Drawing.Point(100,/*25*/ NameY_AzTaraf), stringformatWriteTextFormat);

                graphicsMasterImage.DrawString(comision1.fldOrganicRoleName, new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new System.Drawing.SolidBrush(colorStringColor),
                new System.Drawing.Point(100,/*45*/ CommY_AzTaraf), stringformatWriteTextFormat);

                stringformatWriteTextFormat.Alignment = StringAlignment.Near;
                graphicsMasterImage.DrawString("از طرف", new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new SolidBrush(colorStringColor),
                new System.Drawing.Point(90, /*60*/Name_AzTaraf), stringformatWriteTextFormat);

                stringformatWriteTextFormat.Alignment = StringAlignment.Center;

                var q = p.sp_tblSubstituteSelect("fldReceiverComisionID", comision2.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldSenderComisionID == comision1.fldID).FirstOrDefault();
                var StaffName = comision2.fldStaffName;
                if (q.fldShowReceiverName)
                StaffName = comision2.fldStaffName + " " + comision2.fldOrganicRoleName;

                graphicsMasterImage.DrawString(StaffName, new System.Drawing.Font(FontName, FontSize,
                System.Drawing.FontStyle.Regular), new SolidBrush(colorStringColor),
                new System.Drawing.Point(70, /*75*/Name_AzTaraf+10), stringformatWriteTextFormat);
                //Save the new image in a physical location
                signpath = AppDomain.CurrentDomain.BaseDirectory + "docs\\sign" + id + "_" + comision1.fldStaffID + ".png";


                int w = 0, h = 0, w1 = 0, h1 = 0, w2 = 0, h2 = 0;
                if (bitmap1.Width > 200)
                {
                    width = bitmap1.Width;
                    w = width / 2 - (200 / 2);
                }
                else
                {
                    w = 70 - (bitmap1.Width / 2);
                }
                if (bitmap1.Height > 200)
                {
                    height = bitmap1.Height;
                    h = height / 2 - (200 / 2);
                }
                else
                {
                    h = 70 - (bitmap1.Height / 2);
                }

                if (bitmap1.Width > 200 && bitmap1.Height < 200)
                {
                    w1 = 0;
                    h1 = h;
                    w2 = w;
                    h2 = 0;
                }
                else if (bitmap1.Width > 200 && bitmap1.Height > 200)
                {
                    w1 = 0;
                    h1 = 0;
                    w2 = w;
                    h2 = h;
                }

                else if (bitmap1.Width < 200 && bitmap1.Height > 200)
                {
                    w1 = w;
                    h1 = 0;
                    w2 = 0;
                    h2 = h;
                }
                else if (bitmap1.Width < 200 && bitmap1.Height < 200)
                {
                    w1 = w;
                    h1 = h;
                    w2 = 0;
                    h2 = 0;
                }
                bitmap1.MakeTransparent(Color.White);
                bitmap2.MakeTransparent(Color.White);
                Bitmap img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(Color.Transparent);
                if (SignType)
                    g.DrawImage(bitmap1, new System.Drawing.Point(w1, h1));
                g.DrawImage(bitmap2, new System.Drawing.Point(w2, h2));
                
                img3.Save(signpath, System.Drawing.Imaging.ImageFormat.Png);
            }
            return signpath;
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterPatternSelect("", "", 30, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult CheckHaveFile(int LetterID)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())/*.Where(l=>l.fldExt!="")*/.FirstOrDefault();
            if (q != null)
            {
                var sign = p.sp_tblSignerSelect("fldLetterID", LetterID.ToString(), 0).Where(k => k.fldFirstSigner != null).FirstOrDefault();
                int issign = 0;
                if (sign != null)
                    issign = 1;
                return Json(new { have = 1, issign = issign }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { have = 0 ,issign =0}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetLetterPattern()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_UserId_PatternSelect((Session["UserId"]).ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Models.sp_tblContentFileSelect LetterPattern)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (LetterPattern.fldDesc == null)
                    LetterPattern.fldDesc = "";
                //ثبت رکورد جدید
                if (Request.Files.Count > 0)
                {
                    string filename = Request.Files[0].FileName;
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + filename;
                    Request.Files[0].SaveAs(filePath);
                    MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(filePath));
                    byte[] _File = stream.ToArray();
                    var q = p.sp_tblContentFileSelect("fldLetterID", LetterPattern.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    if (q == null)
                    {
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                        p.sp_tblContentFileInsert(_id, "", _File, LetterPattern.fldLetterPatternID, LetterPattern.fldLetterID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), ".docx");
                        System.IO.File.Delete(filePath);
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });                        
                    }
                    else
                    {
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                        p.sp_tblContentFileUpdate(q.fldID, "", _File, LetterPattern.fldLetterPatternID, LetterPattern.fldLetterID, Convert.ToInt32(Session["UserId"]),"", Session["UserPass"].ToString());
                        System.IO.File.Delete(filePath);
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }
                }
                return Json(new { data = "ذخیره با موفقیت انجام نشد.", state = 1 });
            }
            catch (Exception x)
            {
                string savePath = Server.MapPath(@"~\Uploaded\Err.txt");
                System.IO.File.WriteAllText(savePath, x.InnerException.Message);
                        
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldType" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterPatternSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblContentFileDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblLetterPatternSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldDesc = q.fldDesc
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

    }
}
