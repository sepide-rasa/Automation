using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using OpenPop.Pop3;
using OpenPop.Mime;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Net.Mail;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Collections;
using HtmlAgilityPack;

using Aspose.Words;
using Automation.Controllers.Users;

namespace Automation.Controllers.Tools
{
    class EmailList
    {
        public List<OpenPop.Mime.MessagePart> attachments;
        public string Subject;
        public OpenPop.Mime.Header.RfcMailAddress Sender;
        public OpenPop.Mime.Header.RfcMailAddress From;
        public string MessageId;
        public List<OpenPop.Mime.Header.Received> Received;
        public string Body;
    }
    public class TransferEmailController : Controller
    {
        //
        // GET: /TransferEmail/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 100))
            {
            return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_Email_TempSelect("", "", 30).Select(k => new { k.fldId, k.fldSubject, k.fldFrom }).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult Reload( int value)
        {
            byte[] Pdf_file = null;
            string attachmentName;
            string attachmentType;
            byte[] attachbody;
            string attType;



            Models.AutomationEntities p = new Models.AutomationEntities();

            //var A = p.sp_tblEmailAttachement_TempSelect("", "", 0).ToList();
            //foreach (var item in A)
            //    p.sp_tblEmailAttachement_TempDelete(item.fldId, Convert.ToInt32(Session["UserId"]));
            //var E = p.sp_Email_TempSelect("", "", 0).ToList();
            //foreach (var item2 in E)
            //    p.sp_Email_TempDelete(item2.fldId, Convert.ToInt32(Session["UserId"]));

            var q = p.sp_tblProgramSettingSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            string serverName = q.fldRecieveServer;
            string username = q.fldEmailAddress.ToString();
            string password = q.fldEmailPassword.ToString();
            bool SSl = q.fldSSL;
            int port =q.fldRecievePort;
            List<string> seenUids = new List<string>();

            Pop3Client client = new Pop3Client();
            //Connect to the server.
            client.Connect(serverName, port, SSl);
            client.Authenticate(username, password);

            List<string> uids = client.GetMessageUids();

            // Create a list we can return with all new messages
            List<OpenPop.Mime.Message> newMessages = new List<OpenPop.Mime.Message>();
            int j = uids.Count - value;
            int k = value;
            if (j < 0)
            {
                j = 0;
                k = uids.Count;
            }
            // All the new messages not seen by the POP3 client
            EmailList Emails;
            ArrayList array = new ArrayList();
            for (int i = 0; i < k; i++)
            {

                j++;
                string currentUidOnServer = uids[i];
                if (!seenUids.Contains(currentUidOnServer))
                {
                    OpenPop.Mime.Message unseenMessage = client.GetMessage(j);
                    Emails = new EmailList();

                    //MessagePart messagePart = unseenMessage.MessagePart.MessageParts[0];
                    Emails.attachments = unseenMessage.FindAllAttachments();
                    // Add the message to the new messages
                    newMessages.Add(unseenMessage);
                    Emails.Subject = unseenMessage.Headers.Subject;
                    Emails.Sender = unseenMessage.Headers.Sender;
                    Emails.From = unseenMessage.Headers.From;
                    Emails.MessageId = unseenMessage.Headers.MessageId;
                    Emails.Received = unseenMessage.Headers.Received;
                    //Body = unseenMessage.MessagePart.Body;
                    //Body = messagePart.BodyEncoding.Get String(messagePart.Body);

                    string Text = "";
                    string body = "";
                    OpenPop.Mime.MessagePart plainTextPart = unseenMessage.FindFirstHtmlVersion();
                    if (plainTextPart != null)
                    {
                        Emails.Body = plainTextPart.GetBodyAsText();
                    }
                    else
                    {
                        plainTextPart = unseenMessage.FindFirstPlainTextVersion();
                        if (plainTextPart != null)
                        {
                            Emails.Body = plainTextPart.GetBodyAsText();
                        }
                        //for (int z = 0; z < Text.Length; z++)
                        //{
                        //    if (z % 100 == 0)
                        //        body = body + "</br>";
                        //    body = body + Text[z].ToString();
                            
                        //}
                        //Emails.Body = body;
                    }

                    //OpenPop.Mime.MessagePart plainTextPart = unseenMessage.FindFirstHtmlVersion();
                    //Emails.Body = plainTextPart.GetBodyAsText();


                    array.Add(Emails);
                    //Aspose.Pdf.Generator.Pdf pdf = new Aspose.Pdf.Generator.Pdf();
                    //// add the section to PDF document sections collection
                    //Aspose.Pdf.Generator.Section section = pdf.Sections.Add();
                    //// Read the contents of HTML file into StreamReader object

                    //StreamReader r = System.IO.File.OpenText(HtmlFile);

                    ////Create text paragraphs containing HTML text
                    //Aspose.Pdf.Generator.Text text2 = new Aspose.Pdf.Generator.Text(section, r.ReadToEnd());
                    //// enable the property to display HTML contents within their own formatting
                    //text2.IsHtmlTagSupported = true;
                    ////Add the text paragraphs containing HTML text to the section
                    //section.Paragraphs.Add(text2);
                    //// Specify the URL which serves as images database
                    //pdf.HtmlInfo.ImgUrl = "D:/pdftest/MemoryStream/";
                    //string pdfFilename = Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf");
                    ////Save the pdf document
                    //pdf.Save(pdfFilename);

                    //PdfSharp.Pdf.PdfDocument pdf = new PdfSharp.Pdf.PdfDocument();
                    //PdfSharp.Pdf.PdfPage pdfPage = pdf.AddPage();
                    //XGraphics graph = XGraphics.FromPdfPage(pdfPage);
                    //XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
                    //graph.DrawString(Body, font, XBrushes.Black, new XRect(0, 0, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.Center);
                    ////string pdfFilename = "firstpage2.pdf";
                    //string pdfFilename = Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf");
                    //pdf.Save(pdfFilename);
                    //MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(pdfFilename));

                    // Add the uid to the seen uids, as it has now been seen
                    seenUids.Add(currentUidOnServer);
                }

            }
            foreach (var item in array)
            {
                EmailList email = (EmailList)item;
                HtmlDocument doc = new HtmlDocument();
                MemoryStream st = new MemoryStream();
                doc.LoadHtml(email.Body);
                System.IO.StringWriter sw = new System.IO.StringWriter();

                System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(sw);
                doc.Save(xw);

                string result = sw.ToString();

                System.IO.File.WriteAllText(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".htm"), "<div dir='rtl'>" + result + "</div>", Encoding.UTF8);
                Document doc1 = new Document(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".htm"), new LoadOptions(LoadFormat.Html, "", ""));
                Aspose.Words.Saving.PdfSaveOptions options = new Aspose.Words.Saving.PdfSaveOptions();
                options.PageIndex = 0;
                options.PageCount = doc1.PageCount;
                doc1.Save(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf"), options);
                //string pdfFilename = Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf");


                //var h = new StringReader(result);
                //using (Document document = new Document())
                //{
                //    PdfWriter writer = PdfWriter.GetInstance(document, st);
                //    document.Open();

                //    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, h);
                //}

                Pdf_file = System.IO.File.ReadAllBytes(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf")).ToArray();
                
                System.IO.File.Delete(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".pdf"));
                System.IO.File.Delete(Server.MapPath(@"~\Uploaded\" + j.ToString() + ".htm"));

                System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                var Temp = p.sp_Email_TempSelect("fldMessage_No", email.MessageId, 1).FirstOrDefault();
                
                if (Temp == null)
                {
                    p.sp_Email_TempInsert(_id, email.Subject, email.From.ToString(), Pdf_file, email.MessageId, Convert.ToInt32(Session["UserId"]), "");

                    foreach (OpenPop.Mime.MessagePart attachment in email.attachments)
                    {
                        if (attachment != null)
                        {
                            attachmentName = attachment.FileName.Split('.').First();
                            String ContentID = attachment.ContentId;
                            attachmentType = attachment.ContentType.MediaType;
                            attType=MimeType.GetMimeType(attachmentType);
                            attachbody = attachment.Body;
                            attachmentName = attachment.FileName.Split('.').First() + "." + attType;
                            p.sp_tblEmailAttachement_TempInsert(Convert.ToInt32(_id.Value), attachmentName, attachbody, Convert.ToInt32(Session["UserId"]), "");
                        }
                    }
                }
            }


            var q1 = p.sp_Email_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldFrom }).ToList();
            return Json(q1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GeneratePDF(int? id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var Content = p.sp_Email_TempSelect("fldId", id.ToString(), 0).FirstOrDefault();

                if (Content != null)
                    return File(Content.fldBody, "application/pdf");

                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(string id)
        {
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 101))
                {
                    Models.AutomationEntities m = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        var k = m.sp_tblEmailAttachement_TempSelect("fldEmail_TempId", id, 1).FirstOrDefault();
                        if (k != null)
                            m.sp_tblEmailAttachement_TempDelete(k.fldId, Convert.ToInt32(Session["UserId"]));
                        m.sp_Email_TempDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]));
                        return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });

                    }
                    else
                    {
                        return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
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

        public FileContentResult FileExport(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_Email_TempSelect("fldId", id.ToString(), 0).FirstOrDefault();
            return File(q.fldBody, MimeType.Get("pdf"), q.fldSubject+".pdf");
        }
        public FileContentResult AttachementExport(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var att = p.sp_Email_TempSelect("fldId", id.ToString(), 0).FirstOrDefault();
            var q = p.sp_tblEmailAttachement_TempSelect("fldEmail_TempId", att.fldId.ToString(), 0).ToList();
            foreach(var item in q)
                return File(item.fldAttachementBody, MimeType.Get(item.fldAttachementName.Split('.').Last()), item.fldAttachementName);

            return null;
        }

        public ActionResult Reload1(string field, string value, int top, int searchtype)
        {
            string[] _fiald = new string[] { "fldID"};
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_Email_TempSelect("", "", 30).Select(k1 => new { k1.fldId, k1.fldSubject, k1.fldFrom }).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public JsonResult HaveAttach(int id)
        {
            try
            {
                int Have = 0;
                Models.AutomationEntities p = new Models.AutomationEntities();
                var att = p.sp_Email_TempSelect("fldId", id.ToString(), 0).FirstOrDefault();
                var q = p.sp_tblEmailAttachement_TempSelect("fldEmail_TempId", att.fldId.ToString(), 0).ToList();
                if (q.Count != 0)
                    Have = 1;

                return Json(new
                {
                    Have = Have
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
