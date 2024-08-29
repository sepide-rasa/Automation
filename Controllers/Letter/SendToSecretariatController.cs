using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class SendToSecretariatController : Controller
    {
        //
        // GET: /SendToSecretariat/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 115))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public JsonResult _SecretariatTree(int? id,int? SecretariatId)
        {
            try
            {
                var p = new Models.AutomationEntities();

                var rols = (from k in p.sp_SendToSecretariat("fldId", SecretariatId.ToString(), 0)
                            select new
                            {
                                id = k.CommisionId,
                                Name = k.fldStaffName

                            });
                return Json(rols, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public ActionResult GetSecretariat()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblSecretariat_OrganizationUnitSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Send(string secretriant, long LetterId, string SourceId, int SenderComId, List<Models.SendTo> checkedNodes)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            if (checkedNodes != null)
            {
                System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                var date = p.sp_GetDate().FirstOrDefault();

                //دریافت کد باکس جهت ذخیره در کارتابل ارسال شده
                var BoxSendID = p.sp_tblBoxSelect("fldComisionID", SenderComId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 2).FirstOrDefault();
                if (Convert.ToInt32(SourceId) == 0)
                {
                    p.sp_tblAssignmentInsert(_id, LetterId, date.fldDateTime, null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    var LetterBox = p.sp_tblLetterBoxSelect("fldLetterID", LetterId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    p.sp_tblLetterBoxUpdate(LetterBox.fldID, LetterId, BoxSendID.fldID, 1, "", "");
                }
                else
                    p.sp_tblAssignmentInsert(_id, LetterId, date.fldDateTime, Convert.ToInt32(SourceId), Convert.ToInt32(Session["UserId"]),"", Session["UserPass"].ToString());
                p.sp_tblInternalAssignmentSenderInsert((long)_id.Value, SenderComId, BoxSendID.fldID, Convert.ToInt32(Session["UserId"]), "ارسال به دبیرخانه", Session["UserPass"].ToString());
                for (int i = 0; i < checkedNodes.Count(); i++)
                {
                    var BoxCurrentID = p.sp_tblBoxSelect("fldComisionID", checkedNodes[i].ReciverComId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
                    p.sp_tblInternalAssignmentReceiverInsert(Convert.ToInt64(_id.Value), checkedNodes[i].ReciverComId, 1, 3, BoxCurrentID.fldID, null, true, Convert.ToInt32(Session["UserId"]),"", Session["UserPass"].ToString());
                }
            }
            return Json(new { data = "ارسال با موفقیت انجام شد.", state = 0 });
        }
    }
}

