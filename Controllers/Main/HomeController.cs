using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OpenPop.Pop3;
using OpenPop.Mime;
using Automation.Controllers.Users;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Xml;
using System.IO;
using System.Text;
using Ext.Net.MVC;
using Ext.Net;
using Automation.Models; 

namespace Automation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            Models.AutomationEntities p = new Models.AutomationEntities();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            Session["StaffId"] = user.fldStaffID;
            if (OnlineUser.userObj.Where(item => item.sessionId == System.Web.HttpContext.Current.Request.Cookies["ASP.NET_SessionId"].Value.ToString()).Count() > 0)
                OnlineUser.userObj.Remove(OnlineUser.userObj.Where(item => item.sessionId == System.Web.HttpContext.Current.Request.Cookies["ASP.NET_SessionId"].Value.ToString()).FirstOrDefault());
            OnlineUser.AddOnlineUser("", Session["UserName"].ToString(), Session["UserId"].ToString(), System.Web.HttpContext.Current.Request.Cookies["ASP.NET_SessionId"].Value.ToString());
            
            return View();
        }

        public ActionResult SendEmail(int LetterID, string Reciver)
        {
            string Path = null;
            Models.AutomationEntities p = new Models.AutomationEntities();
            var E = p.sp_tblLetterSelect("fldId", LetterID.ToString(), 0, 0, "").FirstOrDefault();

            var Content = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
            var Att = p.sp_tblLetterAttachmentSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();

            foreach (var Item in Content)
            {
                if (Item.fldExt == ".docx")
                {
                    System.IO.File.WriteAllBytes(Server.MapPath(@"~\Uploaded\" + E.fldSubject + ".docx"), Item.fldLetterText);
                    Path = Path + @"~\Uploaded\" + E.fldSubject + ".docx" + ";";
                }
            }
            foreach (var _Item in Att)
            {
                var AttContent = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
                foreach (var I in AttContent)
                {
                    if (I.fldLetterPatternID == null)
                    {
                        System.IO.File.WriteAllBytes(Server.MapPath(@"~\Uploaded\" + _Item.fldName), I.fldLetterText);
                        Path = Path + @"~\Uploaded\" + _Item.fldName + ";";
                    }
                }
            }

            if (Path != null)
            {
                Path = Server.MapPath(Path);
                Path = Path.Substring(0, Path.Length - 1);
            }
            Reciver = Reciver.Substring(0, Reciver.Length - 1);
            p.sp_Email_SendEmail(Reciver, "نامه در پیوست می باشد... " + E.fldDesc, E.fldSubject, Path);

            if (Path != null)
            {
                var att = Path.Split(';');
                for (int i = 0; i < att.Length; i++)
                    System.IO.File.Delete(att[i]);
            }
            return Json("ایمیل با موفقیت ارسال شد.", JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailOrDownload(int LetterID)
        {
            ViewBag.LetterID = LetterID;
            return PartialView();
        }
        public string ImageToBase64(byte[] imageBytes)
        {
            // Convert FileStrea
            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;

        }
        public string SendECE(int LetterID, string Reciver, int ReceiveType)
        {
            string Type = "";
            switch (ReceiveType)
            {
                case 1:
                    Type = "Origin";
                    break;
                case 2:
                    Type = "Copy";
                    break;
            }
            Guid g;
            g = Guid.NewGuid();
            string Path = null;
            Models.AutomationEntities p = new Models.AutomationEntities();
            var E = p.sp_tblLetterSelect("fldId", LetterID.ToString(), 0, 0, "").FirstOrDefault();
            var S = p.sp_tblProgramSettingSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var Content = p.sp_tblContentFileSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
            var Att = p.sp_tblLetterAttachmentSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
            var R = p.sp_tblExternalLetterReceiverSelect("fldLetterID", LetterID.ToString(), 0).ToList();
            var Signer = p.sp_tblSignerSelect("fldLetterID", LetterID.ToString(), 0).FirstOrDefault();

            var Keyword = E.fldKeywords;
            var Subject = E.fldSubject;
            var UserName = S.fldName;
            var data = E.fldCreatedDate + "T" + E.fldDateTime;
            var LetterNumber = E.fldLetterNumber;
            var ImmediacyName = E.fldImmediacyName;
            string ImmediacyID = E.fldImmediacyID.ToString();
            string UserID = S.fldUserID.ToString();
            var SecurityTypeName = E.fldSecurityTypeName;
            string SecurityTypeID = E.fldSecurityTypeID.ToString();
            Path = Server.MapPath(@"~\docs\" + E.fldSubject + ".xml");
            if (Content.Count != 0)
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(Path))
                {

                    writer.WriteStartDocument();

                    foreach (var Item in Content)
                    {

                        string LetterText = ImageToBase64(Item.fldLetterText);

                        if (Item.fldExt == ".docx")
                        {
                            writer.WriteStartElement("ECE_Send");
                            writer.WriteStartElement("Header");
                            writer.WriteAttributeString("PacketID", g.ToString());   // <-- These are new
                            writer.WriteStartElement("Software");
                            writer.WriteAttributeString("Version", 1.ToString());
                            writer.WriteAttributeString("SoftwareDeveloper", UserName);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.WriteStartElement("Letter");
                            writer.WriteAttributeString("LetterNo", LetterNumber);
                            writer.WriteAttributeString("LetterDateTime", data);
                            writer.WriteAttributeString("ShowDateAs", "jalali");
                            writer.WriteAttributeString("Subject", Subject);
                            writer.WriteStartElement("Sender");
                            writer.WriteAttributeString("Code", UserID);
                            writer.WriteAttributeString("Name", Signer.fldStaffName);
                            writer.WriteAttributeString("Position", Signer.fldName);
                            writer.WriteAttributeString("Organization", UserName);
                            writer.WriteRaw(UserName);
                            writer.WriteEndElement();
                            writer.WriteStartElement("Signers");
                            writer.WriteStartElement("Signer");
                            writer.WriteAttributeString("Code", E.fldUserID.ToString());
                            writer.WriteAttributeString("Name", Signer.fldStaffName);
                            writer.WriteAttributeString("Position", Signer.fldName);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.WriteStartElement("Receivers");
                            foreach (var Recivers in R)
                            {
                                writer.WriteStartElement("Receiver");
                                writer.WriteAttributeString("Organization", Recivers.fldName);
                                writer.WriteAttributeString("Code", Recivers.fldExternalPartnerID.ToString());
                                writer.WriteAttributeString("ReceiveType", Type);
                                writer.WriteRaw(Recivers.fldName);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            writer.WriteStartElement("Priority");
                            writer.WriteAttributeString("Code", ImmediacyID);
                            writer.WriteAttributeString("Name", ImmediacyName);
                            writer.WriteEndElement();
                            writer.WriteStartElement("Classification");
                            writer.WriteAttributeString("Code", SecurityTypeID);
                            writer.WriteAttributeString("Name", SecurityTypeName);
                            writer.WriteEndElement();
                            writer.WriteStartElement("Keywords");
                            writer.WriteElementString("Keyword", Keyword);
                            writer.WriteEndElement();
                            writer.WriteStartElement("Contents");
                            writer.WriteStartElement("Content");
                            writer.WriteAttributeString("ContentType", "application / vnd.openxmlformats - officedocument.wordprocessingml.document");
                            writer.WriteRaw(LetterText);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.WriteStartElement("Attachments");
                            foreach (var ATT in Att)
                            {
                                var Extension = ATT.fldName.Split('.').Last();
                                var Description = ATT.fldName.Split('.').First();
                                var contentType = MimeType.Get(ATT.fldName.Split('.').Last());
                                var qq = p.sp_tblContentFileSelect("fldID", ATT.fldContentFileID.ToString(), 0, 0, "").FirstOrDefault();
                                string Attachment = Convert.ToBase64String(qq.fldLetterText);
                                writer.WriteStartElement("Attachment");
                                writer.WriteAttributeString("ContentType", contentType);
                                writer.WriteAttributeString("Description", Description);
                                writer.WriteAttributeString("Extension", Extension);
                                writer.WriteRaw(Attachment);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }

                    }
                    writer.WriteEndDocument();
                }
            }



            return Path;
        }
        public ActionResult SendECEByEmail(int LetterID, string Reciver, int ReceiveType)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var E = p.sp_tblLetterSelect("fldId", LetterID.ToString(), 0, 0, "").FirstOrDefault();
            Reciver = Reciver.Substring(0, Reciver.Length - 1);
            p.sp_Email_SendEmail(Reciver, "نامه در پیوست می باشد... " + E.fldDesc, E.fldSubject, SendECE(LetterID, Reciver, ReceiveType));

            if (SendECE(LetterID, Reciver, ReceiveType) != null)
            {
                var att = SendECE(LetterID, Reciver, ReceiveType).Split(';');
                for (int i = 0; i < att.Length; i++)
                    System.IO.File.Delete(att[i]);
            }
            return Json("ایمیل با موفقیت ارسال شد.", JsonRequestBehavior.AllowGet);
        }
        public FileContentResult Download(int LetterID, string Reciver, int ReceiveType)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var E = p.sp_tblLetterSelect("fldId", LetterID.ToString(), 0, 0, "").FirstOrDefault();
            MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(SendECE(LetterID, Reciver, ReceiveType)));
            return File(stream.ToArray(), MimeType.Get("xml"), E.fldSubject + ".xml");
        }

        public ActionResult NotReadFill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var Date = m.sp_GetDate().FirstOrDefault();
            var Com = m.sp_tblCommisionSelect("fldStaffID", Session["StaffId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = m.sp_tblBoxSelect("fldComisionID", Com.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
            var q2 = m.sp_LetterSelectInboxNotRead(q.fldID).ToList().Where(k => k.fldAnswerDate == Date.fldDateTime.Date).ToDataSourceResult(request);
            return Json(q2);
        }
        public ActionResult DelayNotReadFill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var Date = m.sp_GetDate().FirstOrDefault();
            var Com = m.sp_tblCommisionSelect("fldStaffID", Session["StaffId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = m.sp_tblBoxSelect("fldComisionID", Com.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
            var q2 = m.sp_LetterSelectInboxNotRead(q.fldID).ToList().Where(k => k.fldAnswerDate < Date.fldDateTime.Date).ToDataSourceResult(request);
            return Json(q2);
        }

        public ActionResult Home()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_GetDate().FirstOrDefault();
            var time = q.fldDateTime;
            ViewBag.time = time.Hour.ToString().PadLeft(2, '0') + ":" +
                time.Minute.ToString().PadLeft(2, '0') + ":" +
                time.Second.ToString().PadLeft(2, '0');

            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var staff = p.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            ViewBag.FromDate = MyLib.Shamsi.Miladi2ShamsiString(time.AddDays(-(staff.fldLetterLoadNum)));
            ViewBag.ToDate = MyLib.Shamsi.Miladi2ShamsiString(time);
            return PartialView();
        }

        public ActionResult BasicInf()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 1))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Facilities()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 41))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        

        public ActionResult Andicator(int id, int ComId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();

            var comision = p.sp_tblCommisionSelect("fldid", ComId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var organicRol = p.sp_tblOrganicRoleSelect("fldid", comision.fldOrganicRoleID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var Secretariat = p.sp_tblSecretariatSelect("fldOrgUnitId", organicRol.fldOrganizationUnitID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (Secretariat != null)
            {
                var Letter = p.sp_tblLetterSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Letter.fldLetterStatusID == 2)
                {
                    if (Letter.fldLetterDate != null || Letter.fldLetterNumber != null)
                        return Json("نامه قبلا ثبت اندیکاتور شده است.", JsonRequestBehavior.AllowGet);
                    var SecretariatFormat = p.sp_tblSecretariatFormatSelect("fldSecretariatId", Secretariat.fldID.ToString(), 1).FirstOrDefault();
                    if (SecretariatFormat != null)
                    {
                        string LetterNumber = "";
                        var Format = SecretariatFormat.fldNumeralFormat.Split('*');
                        for (int i = 0; i < Format.Count() - 1; i++)
                        {
                            switch (Format[i])
                            {
                                case "شماره ثبت":
                                    LetterNumber += Letter.fldOrderId;
                                    break;
                                case "شمارنده":
                                    System.Data.Objects.ObjectParameter _Number = new System.Data.Objects.ObjectParameter("fldNumber", typeof(int));
                                    p.sp_tblLetterNumberInsert(Letter.fldID, _Number, SecretariatFormat.fldStartNumber, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                    LetterNumber += _Number.Value.ToString();
                                    break;
                                case "سال دو رقمی":
                                    LetterNumber += Letter.fldYear.ToString().Substring(2, 2);
                                    break;
                                case "سال چهار رقمی":
                                    LetterNumber += Letter.fldYear.ToString();
                                    break;
                                case "شماره حکم":
                                    var commistion = p.sp_tblCommisionSelect("fldid", Letter.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                                    LetterNumber += commistion.fldOraganicNumber.ToString();
                                    break;
                                default:
                                    LetterNumber += Format[i];
                                    break;
                            }
                        }
                        var Date = p.sp_GetDate().FirstOrDefault();
                        p.sp_tblLetterUpdateNumDate(Letter.fldID, LetterNumber, Date.fldDateTime, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                        p.sp_tblLetterStatusIdUpdate(Letter.fldID, 3);
                        return Json("نامه باموفقیت ثبت اندیکاتور شد. شماره نامه: " + LetterNumber, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json("نامه امضا نشده و شما نمی توانید آن را ثبت اندیکاتور نمایید.", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("نامه امضا نشده و شما نمی توانید آن را ثبت اندیکاتور نمایید.", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("شما کاربر دبیرخانه نمی باشید و مجوز ثبت اندیکاتور ندارید.", JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetBoxType(int id, int? BoxtypeId, int? AtypeId)
        {
            var IsRoot = 1;
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_GetBoxTypeId(id).FirstOrDefault();
            var type = q.fldBoxType;
            if (BoxtypeId == 999)
                type = 2;
            Session["FldBoxTypeId"] = type;
            if (q.fldPID != null)
                IsRoot = 0;
            return Json(new { fldType = type, IsRoot = IsRoot }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Boxes(int? id, int cId)
        {
            var p = new Models.AutomationEntities();
            string url = Url.Content("~/Content/images/B");
            if (id != null)
            {
                var rols = (from k in p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                            where k.fldComisionID == cId
                            select new
                            {
                                id = k.fldID,
                                Name = k.fldName,
                                BoxtypeId = k.fldBoxTypeID,
                                AtypeId = k.fldDesc,
                                image = url + k.fldBoxTypeID.ToString() + ".png",
                                hasChildren = p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                .Where(h => h.fldComisionID == cId).Any()
                            });

                return Json(rols, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rols = (from k in p.sp_tblBoxSelect("", "", 0, 1, "")
                            where k.fldComisionID == cId
                            select k);
                //new
                //            {
                //                id = k.fldID,
                //                Name = k.fldName,
                //                BoxtypeId=k.fldBoxTypeID,
                //                image = url + k.fldBoxTypeID.ToString() + ".png",
                //                hasChildren = p.sp_tblBoxSelect("", "", 0, 1, "")
                //                .Where(h => h.fldComisionID == cId).Any()
                //            });
                var ggg = rols.ToList();
                for (int i = 0; i < ggg.Count(); i++)
                {
                    if (ggg[i].fldBoxTypeID == 1)
                    {
                        var q = p.sp_LetterSelectInboxNotRead(ggg[i].fldID).ToList();
                        int count = 0;
                        if (q != null)
                            count = q.Count;
                        ggg[i].fldName = ggg[i].fldName + "(" + count + ")";
                    }
                }
                var t = ggg.Select(k => new
                 {
                     id = k.fldID,
                     Name = k.fldName,
                     BoxtypeId = k.fldBoxTypeID,
                    AtypeId = k.fldDesc,
                     image = url + k.fldBoxTypeID.ToString() + ".png",
                     hasChildren = p.sp_tblBoxSelect("", "", 0, 1, "")
                     .Where(h => h.fldComisionID == cId).Any()
                 }).ToList();
                return Json(t, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Minder(int id)
        {
            ViewBag.MindId = id;
            return PartialView();
        }

        public FileContentResult Image()
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, 1, "").FirstOrDefault();
            if (user != null)
            {
                var pic = p.sp_tblPictureSelect("fldStaffID", user.fldStaffID.ToString(), 1, 1, "").FirstOrDefault();
                if (pic != null)
                {
                    if (pic.fldStaffPicture != null)
                    {
                        return File((byte[])pic.fldStaffPicture, "jpg");
                    }
                }
            }
            return null;
        }

        public JsonResult _RolsTree(int? id)
        {
            List<tree> nodes = new List<tree>();
            AutomationEntities hh = new AutomationEntities();
            var newUsers = OnlineUser.userObj.Where(item => item.newStatus == true).Select(item => item.userId).ToList();
            var ISOn = false;
            if (id == null)
            {
                tree t1 = new tree();
                t1.id = "0";
                t1.Name = "دوستان";
                t1.image = Url.Content("~/Content/images/folder.png");
                t1.hasChildren = true;
                nodes.Add(t1);
            }
            else
            {
                var Friend = hh.sp_tblFriendsSelect("fldUserID", Session["UserId"].ToString(), 0, Convert.ToInt32(Session["UserId"])).ToList();
                foreach (var fitem in Friend)
                {
                    var Unread = hh.sp_tblChatSingleSelect("fldSender_ReceiverUserID", fitem.fldFriendsUserId.ToString(), Session["UserId"].ToString(), 0).Where(k => k.fldReadDate == null).ToList();
                    
                    tree t = new tree();
                    t.id = "F" + fitem.fldId.ToString();

                    if (Unread.Count != 0)
                        t.Name = fitem.fldName + "(" + Unread.Count + ")";
                    else
                        t.Name = fitem.fldName;

                    foreach (var _I in newUsers)
                        if (fitem.fldFriendsUserId.ToString() == _I)
                            ISOn = true;
                    if (ISOn)
                        t.image = Url.Content("~/Content/images/onlineUser.png");
                    else
                        t.image = Url.Content("~/Content/images/offlineUser.png");
                    t.IsOnline = ISOn;
                    t.hasChildren = false;
                    nodes.Add(t);
                    ISOn = false;
                    //.ImageUrl(Url.Content("~/Content/images/onlineUser.png"));
                }
            }
            var rols = (from k in nodes select new
                                {
                                    id = k.id,
                                    Name = k.Name,
                                    image=k.image,
                                    IsOnline=k.IsOnline,
                                    hasChildren = k.hasChildren

                                });
            return Json(rols, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OnlineChat()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 114))
            {                
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public StoreResult GetChildren(string node)
        {
            NodeCollection nodes = new Ext.Net.NodeCollection();
            Models.AutomationEntities m = new Models.AutomationEntities();
            if (!string.IsNullOrEmpty(node))
            {
                Node asyncNode = new Node();
                asyncNode.Text = "دوستان";
                asyncNode.NodeID = "0";
                asyncNode.Expanded = true;

                //for (int i = 1; i < 6; i++)
                //{
                //    Node childNode = new Node();
                //    childNode.Text = node + i;
                //    childNode.NodeID = node + i;
                //    childNode.Leaf = true;
                //    childNode.Icon = Ext.Net.Icon.User;
                //    asyncNode.Children.Add(childNode);
                //}
                var Friend = m.sp_tblFriendsSelect("fldUserID", Session["UserId"].ToString(), 0, Convert.ToInt32(Session["UserId"])).ToList();
                foreach (var Item in Friend)
                {
                    Node childNode = new Node();
                    childNode.Text = Item.fldName;
                    childNode.NodeID = "F" + Item.fldId.ToString();
                    childNode.Leaf = true;
                    childNode.Icon = Ext.Net.Icon.User;
                    asyncNode.Children.Add(childNode);
                }
                if (Friend.Count == 0)
                {
                    asyncNode.Expanded = false;
                    asyncNode.Leaf = true;
                    asyncNode.Icon = Ext.Net.Icon.Folder;
                }
                nodes.Add(asyncNode);

                Node asyncNode2 = new Node();
                asyncNode2.Text = "گروه ها";
                asyncNode2.NodeID = "1";
                asyncNode2.Expanded = true;

                var group = m.sp_tblGroupsSelect("fldUserID", Session["UserId"].ToString(), 0, Convert.ToInt32(Session["UserId"])).ToList();
                //var group = m.sp_tblGroupFriendsSelect("fldUserID", Session["UserId"].ToString(), 0).ToList();
                foreach (var I in group)
                {
                    Node childNode1 = new Node();
                    childNode1.Text = I.fldName;
                    childNode1.NodeID = "G" + I.fldId.ToString();
                    childNode1.Icon = Ext.Net.Icon.Group;
                    childNode1.Expanded = true;
                    asyncNode2.Children.Add(childNode1);

                    var groupFriend = m.sp_tblGroupFriendsSelect("fldGroupId", I.fldId.ToString(), 0).ToList();
                    foreach (var _I in groupFriend)
                    {
                        Node childNode2 = new Node();
                        childNode2.Text = _I.fldName;
                        childNode2.NodeID = "R" + _I.fldId.ToString();
                        childNode2.Leaf = true;
                        childNode2.Icon = Ext.Net.Icon.User;
                        childNode1.Children.Add(childNode2);
                    }
                    if (groupFriend.Count == 0)
                    {
                        childNode1.Expanded = false;
                        childNode1.Leaf = true;
                        childNode1.Icon = Ext.Net.Icon.Folder;
                    }
                }
                if (group.Count == 0)
                {
                    asyncNode2.Expanded = false;
                    asyncNode2.Leaf = true;
                    asyncNode2.Icon = Ext.Net.Icon.Folder;
                }
                nodes.Add(asyncNode2);
            }

            return this.Store(nodes);

        }
        public JsonResult ArchiveTree(string id, string cId)
        {
            try
            {
                var p = new Models.AutomationEntities();

                if (id != null)
                {
                    var h=id.Split('|');
                    var rols = (from k in p.sp_tblArchiveSelect("fldPID", h[0].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID + "|" + cId,
                                    Name = k.fldName,
                                    Image = Url.Content("~/Content/images/") + "archive.png",
                                    hasChildren = p.sp_tblArchiveSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblArchiveSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID + "|" + cId,
                                    Name = k.fldName,
                                    Image = Url.Content("~/Content/images/") + "archives.png",
                                    hasChildren = p.sp_tblArchiveSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }
        public ActionResult Archive()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 116))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_GetDate().FirstOrDefault();
                var time = q.fldDateTime;
                ViewBag.time = time.Hour.ToString().PadLeft(2, '0') + ":" +
                    time.Minute.ToString().PadLeft(2, '0') + ":" +
                    time.Second.ToString().PadLeft(2, '0');

                var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var staff = p.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                ViewBag.FromDate = MyLib.Shamsi.Miladi2ShamsiString(time.AddDays(-(staff.fldLetterLoadNum)));
                ViewBag.ToDate = MyLib.Shamsi.Miladi2ShamsiString(time);
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult ReloadArchive(string ArchiveBoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var s = m.sp_tblUserSelect("fldId", (Session["UserId"]).ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var c = m.sp_tblCommisionSelect("fldStaffID", s.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            //var boxes = "";
            //foreach (var item in c)
            //{
            //    var b = m.sp_tblBoxSelect("fldComisionID", item.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 1).ToList();
            //    foreach (var item2 in b)
            //    {
            //        boxes = boxes + item2.fldID.ToString() + ",";
            //    }
            //}
            var h = ArchiveBoxId.Split('|');
            var b = m.sp_tblBoxSelect("fldComisionID", h[1], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 1).FirstOrDefault();
            var q = m.sp_LetterSelectInboxDate("DateDESC_ArchiveBox", MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End),b.fldID.ToString() , h[0]).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadSent(string ArchiveBoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var s = m.sp_tblUserSelect("fldId", (Session["UserId"]).ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var c = m.sp_tblCommisionSelect("fldStaffID", s.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            //var boxes = "";
            //foreach (var item in c)
            //{
            //    var b = m.sp_tblBoxSelect("fldComisionID", item.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 2).ToList();
            //    foreach (var item2 in b)
            //    {
            //        boxes = boxes + item2.fldID.ToString() + ",";
            //    }
            //}
            var h = ArchiveBoxId.Split('|');
            var b = m.sp_tblBoxSelect("fldComisionID", h[1], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 2).FirstOrDefault();
            var q = m.sp_LetterSelectSentDate("DateDESC_ArchiveBox", MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), b.fldID.ToString(), h[0]).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadDrafts(string ArchiveBoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var s = m.sp_tblUserSelect("fldId", (Session["UserId"]).ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var c = m.sp_tblCommisionSelect("fldStaffID", s.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            //var boxes = "";
            //foreach (var item in c)
            //{
            //    var b = m.sp_tblBoxSelect("fldComisionID", item.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 3).ToList();
            //    foreach (var item2 in b)
            //    {
            //        boxes = boxes + item2.fldID.ToString() + ",";
            //    }
            //}
            var h = ArchiveBoxId.Split('|');
            var b = m.sp_tblBoxSelect("fldComisionID", h[1], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == 3).FirstOrDefault();
            var q = m.sp_LetterSelectDraftDate("DateDESC_ArchiveBox", MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), b.fldID.ToString(), h[0]).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFromArchive(string LetterId, string ArchivID)
        {//حذف یک رکورد

            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 117))
                {
                    if (Convert.ToInt32(LetterId) != 0)
                    {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    var k = Car.sp_tblLetterArchiveSelect("fldLetterID", LetterId, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldArchiveID == Convert.ToInt32(ArchivID)).FirstOrDefault();
                    
                        Car.sp_tblLetterArchiveDelete(k.fldID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
    }
    public class tree
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int BoxtypeId { get; set; }
        public string image { get; set; }
        public bool hasChildren { get; set; }
        public bool IsOnline { get; set; }
        public int pid { get; set; }
    }

}
