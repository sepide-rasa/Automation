using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.Main
{
    [Authorize]
    public class MoveToFolderController : Controller
    {
        //
        // GET: /MoveToFolder

        public ActionResult Index(int BoxId,string SelectedLetterId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_GetBoxTypeId(BoxId).FirstOrDefault();
            ViewBag.BoxId = q.fldId;
            ViewBag.SelectedLetterId = SelectedLetterId;
            return PartialView();
        }
        public JsonResult _FolderTree(int? id,int? BoxId)
        {
            string url = Url.Content("~/Content/images/B");
            try
            {
                var p = new Models.AutomationEntities();


                if (id != null)
                {
                    var rols = (from k in p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())                                
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    image = url + k.fldBoxTypeID.ToString() + ".png",
                                    hasChildren = p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblBoxSelect("fldid", BoxId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())                                
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    image = url + k.fldBoxTypeID.ToString() + ".png",
                                    hasChildren = p.sp_tblBoxSelect("fldid", BoxId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public ActionResult Recovery(int type,int Letterid,int BoxId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            switch (type)
            {
                case 1:
                    {
                        var q = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", Letterid.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxID == BoxId).FirstOrDefault();
                        if (q != null)
                        {
                            var Sent = p.sp_tblBoxSelect("fldComisionID", q.fldSenderComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                            p.sp_tblInternalAssignmentSenderBoxUpdate(q.fldID, Sent.fldID, Convert.ToInt32(Session["UserId"]));
                            return Json(new { data = "نامه با موفقیت بازیابی گردید.", state = 0 });
                        }
                    }
                    break;
                case 2:
                    {
                        var q = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", Letterid.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxID == BoxId).FirstOrDefault();
                        if (q != null)
                        {
                            var recive = p.sp_tblBoxSelect("fldComisionID", q.fldReceiverComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                            p.sp_tblInternalAssignmentReceiverBoxUpdate(q.fldID, recive.fldID, Convert.ToInt32(Session["UserId"]));
                            return Json(new { data = "نامه با موفقیت بازیابی گردید.", state = 0 });
                        }
                    }
                    break;
                case 3:
                    {
                        var q = p.sp_tblLetterBoxSelect("fldLetterID", Letterid.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxID == BoxId).FirstOrDefault();
                        if (q != null)
                        {
                            var Box = p.sp_tblBoxSelect("fldid", BoxId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            var Draft = p.sp_tblBoxSelect("fldComisionID", Box.fldComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 3).FirstOrDefault();

                            p.sp_tblLetterBoxUpdate(q.fldID, q.fldLetterID, Draft.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                            return Json(new { data = "نامه با موفقیت بازیابی گردید.", state = 0 });
                        }
                    }
                    break;
            }
            return Json("");
        }
        public JsonResult Save(string SelectedLetterId, int SelectedFolderId)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (SelectedLetterId != "")
                {
                    string[] letterAssId = SelectedLetterId.Split(';');
                    var box = p.sp_tblBoxSelect("fldId", SelectedFolderId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    for (int i = 0; i < letterAssId.Count() - 1; i++)
                    {
                        var q = p.sp_GetBoxTypeId(SelectedFolderId).FirstOrDefault();
                        if (q != null)
                        {
                            if (q.fldBoxType == 1)
                            {
                                var Assignment = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldReceiverComisionID == box.fldComisionID).FirstOrDefault();
                                if (Assignment != null)
                                {
                                    p.sp_tblInternalAssignmentReceiverBoxUpdate(Assignment.fldID, SelectedFolderId, 1);
                                }
                            }
                            else if (q.fldBoxType == 2)
                            {
                                var Assignment = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldSenderComisionID == box.fldComisionID).FirstOrDefault();
                                if (Assignment != null)
                                {
                                    p.sp_tblInternalAssignmentSenderBoxUpdate(Assignment.fldID, SelectedFolderId, Convert.ToInt32(Session["UserId"]));
                                }
                            }
                            else if (q.fldBoxType == 3)
                            {
                                var D = p.sp_tblLetterBoxSelect("fldLetterID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                p.sp_tblLetterBoxUpdate(D.fldID, D.fldLetterID, SelectedFolderId, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                            }
                        }
                    }
                    return Json(new { data = "نامه(ها) با موفقیت منتقل گردید.", state = 0 });
                }
                return Json(new { data = "نامه(ها) با موفقیت منتقل نگردید.", state = 0 });
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 0 });
            }
        }

        public JsonResult SaveDeleted(int BoxId,string SelectedLetterId)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (SelectedLetterId != "")
                {
                    string[] letterAssId = SelectedLetterId.Split(';');
                    var Box = p.sp_tblBoxSelect("fldId", BoxId.ToString(), 0, 1, "").FirstOrDefault();
                    var TrashBox = p.sp_tblBoxSelect("fldComisionID", Box.fldComisionID.ToString(), 0, 1, "").Where(k => k.fldBoxTypeID == 4).FirstOrDefault();
                    for (int i = 0; i < letterAssId.Count() - 1; i++)
                    {
                        var q = p.sp_GetBoxTypeId(BoxId).FirstOrDefault();
                        if (q != null)
                        {
                            if (q.fldBoxType == 1)
                            {
                                var Assignment = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldReceiverComisionID == Box.fldComisionID).FirstOrDefault();
                                if (Assignment != null)
                                {
                                    p.sp_tblInternalAssignmentReceiverBoxUpdate(Assignment.fldID, TrashBox.fldID, 1);
                                }
                            }
                            else if (q.fldBoxType == 2)
                            {
                                var Assignment = p.sp_tblInternalAssignmentSenderSelect("fldAssignmentID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldSenderComisionID == Box.fldComisionID).FirstOrDefault();
                                if (Assignment != null)
                                {
                                    p.sp_tblInternalAssignmentSenderBoxUpdate(Assignment.fldID, TrashBox.fldID, Convert.ToInt32(Session["UserId"]));
                                }
                            }
                            else if (q.fldBoxType == 3)
                            {
                                var Assignment = p.sp_tblLetterSelect("fldID", letterAssId[i], 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                if (Assignment != null)
                                {
                                    var BoxSendID = p.sp_tblBoxSelect("fldComisionID", Assignment.fldComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 4).FirstOrDefault();
                                    var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", letterAssId[i], 3, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                    p.sp_tblLetterBoxUpdate(LetterBox.fldID, Convert.ToInt64(letterAssId[i]), BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                                }
                            }
                            else if (q.fldBoxType == 4)
                            {
                                var sign = p.sp_tblSignerSelect("fldLetterID", (letterAssId[i]).ToString(), 0).Where(k => k.fldFirstSigner != null).FirstOrDefault();
                                
                                var si = p.sp_tblLetterSelect("fldID", (letterAssId[i]).ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                var Ass = p.sp_tblAssignmentSelect("fldLetterID", (letterAssId[i]).ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                                if (sign != null)
                                    return Json(new { data = "نامه شماره " + si.fldOrderId.ToString() + "دارای امضا می باشد و قابل حذف شدن نیست.", state = 0 });
                                else if (Ass != null)
                                    return Json(new { data = "نامه شماره " + si.fldOrderId.ToString() + "ارجاع شده و قابل حذف شدن نیست.", state = 0 });
                                else
                                {
                                    p.sp_tblRoneveshtDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]));
                                    p.sp_tblLetterFollowLetterIDDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblLetterBoxLetterIDDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblLetterAttachmentLetterIDDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblContentFileDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblLetterArchiveLetterIDDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblInternalLetterReceiverDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblExternalLetterReceiverDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]));
                                    p.sp_tblExternalLetterSenderDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]));
                                    p.sp_tblSignerDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]));
                                    p.sp_tblAssignmentLetterIDDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                    p.sp_tblLetterDelete(Convert.ToInt32(letterAssId[i]), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                                }
                            }
                        }
                    }
                    return Json(new { data = "نامه(ها) با موفقیت حذف گردید.", state = 0 });
                }
                return Json(new { data = "نامه(ها) با موفقیت حذف نگردید.", state = 0 });
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 0 });
            }
        }
    }
}
