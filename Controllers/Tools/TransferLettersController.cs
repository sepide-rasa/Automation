using Automation.Controllers.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.Tools
{
    public class TransferLettersController : Controller
    {
        //
        // GET: /TransferLetters/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 112))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult TransLetters(int S_StaffID, int D_StaffID, int Type)
        {//نمایش اطلاعات جهت رویت کاربر

            Models.AutomationEntities p = new Models.AutomationEntities();
            var S_Box = p.sp_tblBoxSelect("fldComisionID", S_StaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == Type).FirstOrDefault();
            var D_Box = p.sp_tblBoxSelect("fldComisionID", D_StaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(l => l.fldBoxTypeID == Type).FirstOrDefault();
            if (Type == 1)
            {
                var q = p.sp_tblInternalAssignmentReceiverSelect("fldBoxID", S_Box.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                foreach (var Item in q)
                    p.sp_tblInternalAssignmentReceiverInsert(Item.fldAssignmentID, D_StaffID, Item.fldAssignmentStatusID, Item.fldAssignmentTypeID, D_Box.fldID, MyLib.Shamsi.Shamsi2miladiDateTime(Item.fldLetterReadDate), Item.fldShowTypeT_F, Item.fldUserID, Item.fldDesc, Session["UserPass"].ToString());
            }
            else if (Type == 2)
            {
                var q = p.sp_tblInternalAssignmentSenderSelect("fldBoxID", S_Box.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                foreach (var Item in q)
                    p.sp_tblInternalAssignmentSenderInsert(Item.fldAssignmentID, D_StaffID, D_Box.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
            }
            return Json(new { data = "انتقال با موفقیت انجام شد." });

        }
        
    }
}
