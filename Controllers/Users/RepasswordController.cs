using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class RepasswordController : Controller
    {
        //
        // GET: /Repassword/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 95))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Save(Models.sp_tblUserSelect user)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (user.fldDesc == null)
                    user.fldDesc = "";
                if (user.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 95))
                    {
                        //p.sp_tblUserInsert(Convert.ToInt32(Session["UserId"]), user.fldUserName, user.fldPassword, Convert.ToBoolean(user.fldActive_Deactive), 1, "", "");
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
                else
                {//ویرایش رکورد ارسالی
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 95))
                    {
                        p.sp_UserPassUpdate(Convert.ToInt32(Session["UserId"]), user.fldPassword.GetHashCode().ToString());
                        return Json(new { data = "تغییر رمز با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
