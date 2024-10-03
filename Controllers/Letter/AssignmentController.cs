using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
namespace Automation.Controllers.BasicInf
{
    [Authorize] 
    public class AssignmentController : Controller
    {
        //
        // GET: /Assignment/

        public ActionResult Index(long? id, int? CreatorComId)
        {
            ViewBag.AssSourceId = id;
            Session["AssId"] = id;
            Session["ComId"] = CreatorComId;
            Models.AutomationEntities p = new Models.AutomationEntities();
            if (id != null)
            {
                var Assiment = p.sp_tblAssignmentSelect("fldId", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var letter = p.sp_tblLetterSelect("fldId", Assiment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                ViewBag.fldLetterType = letter.fldLetterTypeID;
            }
            return PartialView();
        }
        
        public ActionResult GetComission()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblCommisionSelect("fldStaffID", user.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
            if (Session["AssId"] != null)
            {
                var t = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", Session["AssId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                q = p.sp_tblCommisionSelect("fldId", t.fldSenderComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
                Session.Remove("AssId");
            }
            else if (Session["ComId"] != null)
            {
                q = p.sp_tblCommisionSelect("fldId", Session["ComId"].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
                Session.Remove("ComId");
            }
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAssignmentType()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblAssignmentTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Upload()
        {
            var file = Request.Files["Filedata"];
            string savePath = Server.MapPath(@"~\Uploaded\" + file.FileName);
            file.SaveAs(savePath);
            Session["savePath"] = savePath;
            return Content(Url.Content(@"~\Uploaded\" + file.FileName));
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var m = q.sp_tblLetterAttachmentSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(m);
        }
        public ActionResult Save(Models.LetterAssign LetterAssignment)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var AssignmentReciver = LetterAssignment.fldInternalAssignmentReciverComisionID.Split(';');
                var AssignmentType = LetterAssignment.fldAssignmentTypeID.Split(';');
                if (LetterAssignment.fldDesc == null)
                    LetterAssignment.fldDesc = "";

                //for (int i = 0; i < AssignmentReciver.Count() - 1; i++)
                //{
                //    var email = p.sp_tblEmailSelect("fldStaffID", AssignmentReciver[i].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                //    if (email != null)
                //        if (email.fldSendTrue_False == true)
                //            SendEmail.send(AssignmentReciver[i].ToString(), LetterAssignment.fldLetterID);
                //}

                if (LetterAssignment.fldID == 0)
                {//ثبت رکورد جدید            
                    //var Substitute=p.sp_tblSubstituteSelect("fldReceiverComisionID", LetterAssignment.fldComisionID.ToString(), 1, 1, "");
                    System.Data.Objects.ObjectParameter _idAssignment = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                    byte[] _file = null;
                    var IsLetterID = p.sp_tblAssignmentSelect("fldLetterID", LetterAssignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault(); 
                    //sabte erja
                    if (IsLetterID != null)
                    {// در صورتیکه این نامه قبلا ارجاع داده شده است
                        int? ParentAssignmentID =LetterAssignment.fldAssignID;
                        if (ParentAssignmentID == 0)
                            ParentAssignmentID = null;
                        p.sp_tblAssignmentInsert(_idAssignment, LetterAssignment.fldLetterID, MyLib.Shamsi.Shamsi2miladiDateTime(LetterAssignment.fldAssignmentAnswerDate), ParentAssignmentID, Convert.ToInt32(Session["UserId"]), LetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

                        var BoxSendID = p.sp_tblBoxSelect("fldComisionID", LetterAssignment.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                        //ذخیره نامه در پوشه ارسال شده
                        //var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", LetterAssignment.fldLetterID.ToString(), 1, 1, "").FirstOrDefault();
                        //p.sp_tblLetterBoxUpdate(LetterBox.fldID, LetterAssignment.fldLetterID, BoxSendID.fldID, 1, "", "");
                        p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), LetterAssignment.fldComisionID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), LetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());
                    }
                    else
                    {// در صورتیکه برای اولین بار نامه ارجاع داده می شود
                        p.sp_tblAssignmentInsert(_idAssignment, LetterAssignment.fldLetterID, MyLib.Shamsi.Shamsi2miladiDateTime(LetterAssignment.fldAssignmentAnswerDate), null, Convert.ToInt32(Session["UserId"]), LetterAssignment.fldAssignmentDesc, Session["UserPass"].ToString());

                        //دریافت کد باکس جهت ذخیره در کارتابل ارسال شده
                        var BoxSendID = p.sp_tblBoxSelect("fldComisionID", LetterAssignment.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                        //ذخیره نامه در پوشه ارسال شده
                        var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", LetterAssignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        p.sp_tblLetterBoxUpdate(LetterBox.fldID, LetterAssignment.fldLetterID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        p.sp_tblInternalAssignmentSenderInsert(Convert.ToInt64(_idAssignment.Value), LetterAssignment.fldComisionID, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), LetterAssignment.fldDesc, Session["UserPass"].ToString());
                    }
                    //بررسی فیلد  تفویض جهت ارجاع نامه

                    
                    //
                    var Date = p.sp_GetDate().FirstOrDefault();
                    for (int i = 0; i < AssignmentReciver.Count() - 1; i++)
                    {
                        var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", AssignmentReciver[i].ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();

                        p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), Convert.ToInt32(AssignmentReciver[i]), 1, Convert.ToInt32(AssignmentType[i]), BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        var subStatiut = p.sp_tblSubstituteSelect("fldSenderComisionID", AssignmentReciver[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                        foreach (var item in subStatiut)
                        {
                            BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                            p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_idAssignment.Value), item.fldReceiverComisionID, 1, 1, BoxCurrentID.fldID, null, false, Convert.ToInt32(Session["UserId"]), "تفویض شده", Session["UserPass"].ToString());
                        }
                        
                        //p.sp_tblInternalLetterReceiverInsert(LetterAssignment.fldLetterID, Convert.ToInt32(s[i]), 1, 1, "", "");
                    }
                    
                    if (LetterAssignment.fldName != null)
                    {
                        if (Session["savePath"] != null)
                        {
                            MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                            string filename = Path.GetFileName(Session["savePath"].ToString());
                            System.IO.File.Delete(Session["savePath"].ToString());
                            Session.Remove("savePath");
                            _file = stream.ToArray();
                            System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                            //p.
                            p.sp_tblAssignmentAttachmentInsert(Convert.ToInt64(_id.Value), _file, LetterAssignment.fldName, Convert.ToInt32(Session["UserId"]), LetterAssignment.fldDesc,Session["UserPass"].ToString());
                            return Json(new { data = "ارسال با موفقیت انجام شد.", state = 0 }, JsonRequestBehavior.AllowGet);
                        }
                        else
                            return Json(new { data = "لطفا فایل را وارد کنید.", state = 1 }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { data = "ارسال با موفقیت انجام شد.", state = 0 }, JsonRequestBehavior.AllowGet);
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
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { data = "لطفا فایل گزارش را وارد کنید.", state = 1 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.Message, state = 1 },JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                //ویرایش جدول ارجاعات
                Int64 fldIDAssignment = 0;
                Int64 fldLetterIDAssignment = 0;
                string fldAssignmentDateAssignment = "";
                string fldAnswerDateAssignment = "";
                Int64 fldSourceAssIdAssignment = 0;
                string fldDescAssignment = "";

                Int64 fldID = 0;
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

                //Int64 fldIDExternalLetterReceiver = 0;
                //Int64 fldLetterIDExternalLetterReceiver = 0;
                string fldExternalPartnerIDExternalLetterReceiver = "";
                //string fldDescExternalLetterReceiver = "";



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
                byte fldSignType = 0;
                //ویرایش محتوای نامه
                //Int64 fldIDContentFile = 0;
                //string fldNameContentFile = "";
                //byte fldLetterTextContentFile = 0;
                //int fldLetterPatternIDContentFile = 0;
                //string fldDescContentFile = "";
                //Int64 fldLetterIDContentFile = 0;
                //var HistoryLetter = p.sp_tblHistoryLetterSelect("fldId", id.ToString(), 1, 1, "").FirstOrDefault();

                var Assignment = p.sp_tblAssignmentSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Assignment != null)
                {
                    fldIDAssignment = Assignment.fldID;
                    fldLetterIDAssignment = Assignment.fldLetterID;
                    fldAssignmentDateAssignment = Assignment.fldAssignmentDate;
                    fldAnswerDateAssignment = Assignment.fldAnswerDate;
                    fldSourceAssIdAssignment = Convert.ToInt64(Assignment.fldSourceAssId);
                    fldDescAssignment = Assignment.fldDesc;
                }

                var Letter = p.sp_tblLetterSelect("fldID", fldLetterIDAssignment.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Letter != null)
                {
                    fldID = Letter.fldID;
                    fldDesc = Letter.fldDesc;
                    fldLetterDate = Letter.fldLetterDate;
                    fldCreatedDate = Letter.fldCreatedDate;
                    fldLetterNumber = Letter.fldLetterNumber;
                    fldSubject = Letter.fldSubject;
                    fldKeywords = Letter.fldKeywords;
                    fldLetterStatusID = Letter.fldLetterStatusID;
                    fldComisionID = Letter.fldComisionID;
                    fldImmediacyID = Letter.fldImmediacyID;
                    fldSecurityTypeID = Letter.fldSecurityTypeID;
                    fldLetterTypeID = Letter.fldLetterTypeID;
                    fldSignType = Letter.fldSignType;
                }
                var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", fldLetterIDAssignment.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (LetterBox != null)
                {
                    fldIDLetterBox = LetterBox.fldID;
                    fldBoxIDLetterBox = LetterBox.fldBoxID;
                    fldDescLetterBox = LetterBox.fldDesc;
                }

                var InternalAssignmentSender = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (InternalAssignmentSender != null)
                {
                    fldIDInternalAssignmentSender = InternalAssignmentSender.fldID;
                    fldAssignmentIDInternalAssignmentSender = InternalAssignmentSender.fldAssignmentID;
                    fldSenderComisionIDInternalAssignmentSender = InternalAssignmentSender.fldSenderComisionID;
                    fldBoxIDInternalAssignmentSender = InternalAssignmentSender.fldBoxID;
                    fldDescInternalAssignmentSender = InternalAssignmentSender.fldDesc;
                }
                //var ExternalLetterReceiverCount = p.sp_tblExternalLetterReceiverSelect("fldLetterID", id.ToString(), 0).Count();
                //for (int i = 1; i <= ExternalLetterReceiverCount; i++)
                //{
                var ExternalLetterReceiver = p.sp_tblExternalLetterReceiverSelect("fldLetterID", fldLetterIDAssignment.ToString(), 0).FirstOrDefault();
                var ExternalParntnerName = p.sp_tblExternalPartnerSelect("fldId", ExternalLetterReceiver.fldExternalPartnerID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                fldExternalPartnerIDExternalLetterReceiver = fldExternalPartnerIDExternalLetterReceiver + ExternalParntnerName.fldName.ToString() + ";";
                //}

                var InternalAssignmentReceiver = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
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
                var InternalLetterReceiver = p.sp_tblInternalLetterReceiverSelect("fldLetterID", fldLetterIDAssignment.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (InternalLetterReceiver != null)
                {
                    fldIDInternalLetterReceiver = InternalLetterReceiver.fldID;
                    fldLetterIDInternalLetterReceiver = InternalLetterReceiver.fldLetterID;
                    fldReceiverComisionIDInternalLetterReceiver = InternalLetterReceiver.fldReceiverComisionID;
                    fldAssignmentStatusIDInternalLetterReceiver = InternalLetterReceiver.fldAssignmentStatusID;
                    fldDescInternalLetterReceiver = InternalLetterReceiver.fldDesc;
                }
                var LetterFollow = p.sp_tblLetterFollowSelect("fldLetterID", fldLetterIDAssignment.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (LetterFollow != null)
                {
                    fldIDLetterFollow = LetterFollow.fldID;
                    fldLetterIDLetterFollow = Convert.ToInt64(LetterFollow.fldLetterID);
                    fldLetterTextLetterFollow = LetterFollow.fldLetterText;
                    fldDescLetterFollow = LetterFollow.fldDesc;
                }
                var HistoryLetter = p.sp_tblHistoryLetterSelect("fldLetterID", fldLetterIDAssignment.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (HistoryLetter != null)
                {
                    fldIdHistoryLetter = HistoryLetter.fldId;
                    fldCurrentLetter_IdHistoryLetter = HistoryLetter.fldCurrentLetter_Id;
                    fldHistoryType_IdHistoryLetter = HistoryLetter.fldHistoryType_Id;
                    fldHistoryLetter_IdHistoryLetter = HistoryLetter.fldHistoryLetter_Id;
                    fldDescHistoryLetter = HistoryLetter.fldDesc;
                }

                //var ContentFile = p.sp_tblContentFileSelect("fldLetterID", fldLetterIDAssignment.ToString(), 1, 1, "").FirstOrDefault();
                //if (ContentFile != null)
                //{
                //    fldIDContentFile = ContentFile.fldID;
                //    fldNameContentFile = ContentFile.fldName;
                //    //fldLetterTextContentFile = ContentFile.fldLetterText;
                //    fldLetterPatternIDContentFile = ContentFile.fldLetterPatternID;
                //    fldDescContentFile = ContentFile.fldDesc;
                //    fldLetterIDContentFile = ContentFile.fldLetterID;
                //}

                return Json(new
                {//ویرایش کل نامه
                    //ویرایش جدول نامه
                    fldID = fldID,
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
                    fldSignType = fldSignType,
                    fldExternalPartnerIDExternalLetterReceiver = fldExternalPartnerIDExternalLetterReceiver,
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
                    //fldIDContentFile = fldIDContentFile,
                    //fldNameContentFile = fldNameContentFile,
                    //fldLetterTextContentFile = fldLetterTextContentFile,
                    //fldLetterPatternIDContentFile = fldLetterPatternIDContentFile,
                    //fldDescContentFile = fldDescContentFile,
                    //fldLetterIDContentFile = fldLetterIDContentFile


                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public JsonResult ChekHaveMail(string AssignmentId)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var Ass=AssignmentId.Split(';');
                string Name = "";
                string Email = "";
                for (int i = 0; i < Ass.Length - 1;i++ )
                {
                    var k = p.sp_tblCommisionSelect("fldId", Ass[i].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    var q = p.sp_tblEmailSelect("fldStaffID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    var q2 = p.sp_tblStaffSelect("fldID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                    if (q != null)
                        if (q.fldSendTrue_False)
                        {
                            Name = Name + k.fldStaffName + ";";
                            Email = Email + q2.fldEmailAddress + ";";
                        }
                }
                return Json(new
                {
                    Name = Name,
                    Email = Email
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
