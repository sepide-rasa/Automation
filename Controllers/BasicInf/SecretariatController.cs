using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class SecretariatController : Controller
    {
        //
        // GET: /Secretariat/

        public ActionResult Index(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 11))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblSecretariatSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                
                ViewBag.id = id;
                ViewBag.name = q.fldName;
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        

        public ActionResult Save(Models.sp_tblSecretariatFormatSelect Secretariat)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Secretariat.fldDesc == null)
                    Secretariat.fldDesc = "";
                if (Secretariat.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 12))
                    {

                        p.sp_tblSecretariatFormatInsert(Secretariat.fldYear, Secretariat.fldSecretariatId, Secretariat.fldNumeralFormat, Secretariat.fldStartNumber, Convert.ToInt32(Session["UserId"]), Secretariat.fldDesc);
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
                     if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]),13))
                    {
                        p.sp_tblSecretariatFormatUpdate(Secretariat.fldID, Secretariat.fldYear, Secretariat.fldSecretariatId, Secretariat.fldNumeralFormat, Secretariat.fldStartNumber, Convert.ToInt32(Session["UserId"]), Secretariat.fldDesc);
                    return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
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
        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                 if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 14))
                {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblSecretariatDelete(Convert.ToInt32(id), 1, "");
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

        public JsonResult Details(int id,string year)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblSecretariatFormatSelect("fldYear", year, 0).Where(h => h.fldSecretariatId == id).FirstOrDefault();
                return Json(new
                {
                    fldId = q.fldID, 
                    fldNumeralFormat=q.fldNumeralFormat,
                    fldStartNumber=q.fldStartNumber,
                    fldDesc=q.fldDesc
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }

    }
}
