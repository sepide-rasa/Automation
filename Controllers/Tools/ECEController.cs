using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Automation.Controllers.Users;
using System.IO;
using System.Text;
using System.Drawing;
using Aspose.Words;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Automation.Controllers.Tools
{          
    public class ECEController : Controller
    {
        //
        // GET: /ECE/


        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 102))
            {
            return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult UploadContent(HttpPostedFileBase UpContent)
        {
            string PacketID = "";
            string InstanceID = "";
            string LetterNo = "";
            string LetterDateTime = "";
            string Subject = "";
            string Sender = "";
            string Receivers = "";
            string ReceiveType = "";
            byte[] Contents = null;
            string ContentType = "";
            string Immediacy="";
            string Security="";
            string Keywords = "";
            string Attachments = "";
            string AttType = "";
            string AttDescription = "";
            DateTime LetterDate;

            Models.AutomationEntities f = new Models.AutomationEntities();
         

            if (UpContent != null)
            {
                // Some browsers send file names with full path.
                // We are only interested in the file name.
                var fileName = Path.GetFileName(UpContent.FileName);
                string savePath = Server.MapPath(@"~\Uploaded\" + fileName);
                Session["savePath"] = savePath;
                // The files are not actually saved in this demo
                UpContent.SaveAs(savePath);

                ViewBag.FileName = fileName;


                XmlTextReader reader = new XmlTextReader(savePath);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            switch (reader.Name)
                            {
                                case "Header":
                                    PacketID = reader.GetAttribute("PacketID");
                                    break;

                                case "Software":
                                    InstanceID = reader.GetAttribute("InstanceID");
                                    break;

                                case "Letter":
                                    LetterNo = reader.GetAttribute("LetterNo");
                                    LetterDateTime = reader.GetAttribute("LetterDateTime");
                                    Subject = reader.GetAttribute("Subject");
                                    break;

                                case "Sender":
                                    if (reader.Read())
                                        Sender = reader.Value;
                                    break;

                                case "Priority":
                                    Immediacy = reader.GetAttribute("Name");
                                    break;

                                case "Classification":
                                    Security = reader.GetAttribute("Name");
                                    break;

                                case "Keywords":
                                    while (reader.Read())
                                    {
                                        if (reader.Name == "Keyword")
                                        {
                                            if (reader.Read())
                                                if ((reader.Value).Substring(0, 1) != "" & (reader.Value).Substring(0, 1) != "\r" & (reader.Value).Substring(0, 1) != "\n")
                                                    Keywords = Keywords + reader.Value + ";";
                                        }
                                        else if (reader.Name == "Keywords")
                                            break;
                                    }
                                    break;

                                case "Receivers":
                                    while (reader.Read())
                                    {
                                        if (reader.Name == "Receiver")
                                        {
                                            if (reader.GetAttribute("ReceiveType") != null)
                                                ReceiveType = ReceiveType + reader.GetAttribute("ReceiveType") + ";";

                                            if (reader.Read())
                                                if ((reader.Value).Substring(0, 1) != "" & (reader.Value).Substring(0, 1) != "\r" & (reader.Value).Substring(0, 1) != "\n")
                                                    Receivers = Receivers + reader.Value + ";";

                                        }
                                        else if (reader.Name == "Receivers")
                                            break;
                                    }
                                    break;

                                case "Contents":
                                    while (reader.Read())
                                    {
                                        if (reader.Name == "Content")
                                        {
                                            if (reader.GetAttribute("ContentType") != null)
                                                ContentType = reader.GetAttribute("ContentType");

                                            if (reader.Read())
                                                if ((reader.Value).Substring(0, 1) != "" & (reader.Value).Substring(0, 1) != "\r" & (reader.Value).Substring(0, 1) != "\n")
                                                    Contents = PDF(MimeType.GetMimeType(ContentType), Convert.FromBase64String(reader.Value));

                                        }
                                        else if (reader.Name == "Contents")
                                            break;

                                    }
                                    break;

                                case "Attachments":
                                    while (reader.Read())
                                    {
                                        if (reader.Name == "Attachment")
                                        {
                                            if (reader.GetAttribute("Description") != null)
                                                AttDescription = AttDescription + reader.GetAttribute("Description") + ";";

                                            if (reader.GetAttribute("Extension") != null)
                                                AttType = AttType + reader.GetAttribute("Extension") + ";";
                                            
                                            if (reader.Read())
                                                if ((reader.Value).Substring(0, 1) != "" & (reader.Value).Substring(0, 1) != "\r" & (reader.Value).Substring(0, 1) != "\n")
                                                    Attachments = Attachments + reader.Value + ";";

                                        }
                                        else if (reader.Name == "Attachments")
                                            break;
                                    }
                                    break;


                            }
                            break;

                        case XmlNodeType.Text: //Display the text in each element.
                            //array.Add(reader.Value);
                            break;
                    }
                }
                reader.Close();


                Session["LetterNo"] = LetterNo;
                Session["Sender"] = Sender;
                Session["Subject"] = Subject;
                LetterDate = Convert.ToDateTime(LetterDateTime.Substring(0, 10));
                Session["LetterDateTime"] = LetterDate;

                var q = f.sp_tblExternalPartnerSelect("fldName", Sender, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (q == null)
                {
                    f.sp_tblExternalPartnerInsert(Sender, "", "", "", "", "", Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    q = f.sp_tblExternalPartnerSelect("fldName", Sender, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                }
                var fldSenderId = q.fldID;

                
                System.Data.Objects.ObjectParameter _Id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                f.sp_tblECE_TempInsert(_Id, Receivers, Sender, fldSenderId, Subject, LetterNo, LetterDate, Immediacy, Security, null, "", "", Contents, Keywords, Convert.ToInt32(Session["UserId"]), "");
            
                var Att=Attachments.Split(';');
                var Type=AttType.Split(';');
                var Description=AttDescription.Split(';');
                for (int j = 0; j < Att.Length - 1; j++)
                    f.sp_tblECE_Attach_TempInsert(Convert.ToInt32(_Id.Value), Description[j] + "." + Type[j], Convert.FromBase64String(Att[j]), Convert.ToInt32(Session["UserId"]), "");

            }
            var p = f.sp_tblECE_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldLetterNumber, k1.fldRecieverComision, k1.fldExternalPartner, k1.fldSenderComisionID, k1.fldImmediacy, k1.fldLetterDate, k1.fldSecurityType }).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }



        public ActionResult ShowECE()
        {
            Models.AutomationEntities f = new Models.AutomationEntities();
            var p = f.sp_tblECE_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldLetterNumber, k1.fldRecieverComision, k1.fldExternalPartner, k1.fldSenderComisionID, k1.fldImmediacy, k1.fldLetterDate, k1.fldSecurityType }).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GeneratePDF(int? id)
        {
            try
            {
                //byte[] report_file = null;

                Models.AutomationEntities p = new Models.AutomationEntities();
                var Content = p.sp_tblECE_TempSelect("fldId", id.ToString(), 0).FirstOrDefault();

                if (Content != null)
                    return File(Content.fldContentFile, "application/pdf");

                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
            
        }

        public byte[] PDF(string Type, byte[] Content)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var path = Server.MapPath(@"~\Uploaded\Letter");
            byte[] pdf = null;

            if (Type == "docx" || Type == "doc")
            {
                System.IO.File.WriteAllBytes(path + ".docx", Content);
                Document doc = new Document(path + ".docx");
                doc.Save(path + "." + "pdf");
                pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                System.IO.File.Delete(path + ".docx");
                System.IO.File.Delete(path + ".pdf");
            }
            else if (Type != "docx" & Type != "doc" & Type != "pdf")
            {
                Aspose.Pdf.Generator.Pdf pdf1 = new Aspose.Pdf.Generator.Pdf();

                MemoryStream mystream = new MemoryStream(Content);
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
                pdf1.Save(path+ ".pdf");
                pdf = System.IO.File.ReadAllBytes(path + ".pdf");
                System.IO.File.Delete(path + ".pdf");
            }

            return pdf;

        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var p = m.sp_tblECE_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldLetterNumber, k1.fldRecieverComision, k1.fldExternalPartner, k1.fldSenderComisionID, k1.fldImmediacy, k1.fldLetterDate, k1.fldSecurityType }).ToList();
            return Json(p);
        }
        public ActionResult Reload()
        {//جستجو
            Models.AutomationEntities m = new Models.AutomationEntities();
            var p = m.sp_tblECE_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldLetterNumber, k1.fldRecieverComision, k1.fldExternalPartner, k1.fldSenderComisionID, k1.fldImmediacy, k1.fldLetterDate, k1.fldSecurityType }).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        var k = Car.sp_tblECE_Attach_TempSelect("fldECE_TempId", id, 0).ToList();
                        foreach(var Item in k)
                            Car.sp_tblECE_Attach_TempDelete(Item.fldId, Convert.ToInt32(Session["UserId"]));
                        Car.sp_tblECE_TempDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]));
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
       
    }
}
