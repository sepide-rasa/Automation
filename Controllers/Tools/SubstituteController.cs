using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;
namespace Automation.Controllers.Tools
{
    [Authorize]
    public class SubstituteController : Controller
    {
        //
        // GET: /Substitute/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 80))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }        

        public ActionResult GetComission()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var date = p.sp_GetDate().FirstOrDefault();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblCommisionSelect("fldStaffID", user.fldStaffID.ToString(), 0, 1, "").Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Models.Substitue Substitue)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Substitue.fldDesc == null)
                    Substitue.fldDesc = "";
                if (Substitue.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 81))
                    {
                        p.sp_tblSubstituteInsert(Substitue.fldSenderComisionID, Substitue.fldReceiverComisionID, MyLib.Shamsi.Shamsi2miladiDateTime(Substitue.fldStartDate), MyLib.Shamsi.Shamsi2miladiDateTime(Substitue.fldEndDate), Substitue.fldStartTime, Substitue.fldEndTime, Substitue.fldIsSigner, Substitue.fldShowReceiverName, Convert.ToInt32(Session["UserId"]), Substitue.fldDesc, "");

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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 82))
                    {
                        p.sp_tblSubstituteUpdate(Substitue.fldID, Substitue.fldSenderComisionID, Substitue.fldReceiverComisionID, MyLib.Shamsi.Shamsi2miladiDateTime(Substitue.fldStartDate), MyLib.Shamsi.Shamsi2miladiDateTime(Substitue.fldEndDate), Substitue.fldStartTime, Substitue.fldEndTime, Substitue.fldIsSigner,Substitue.fldShowReceiverName, Convert.ToInt32(Session["UserId"]), Substitue.fldDesc, "");
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

        //public JsonResult GetInf(int idStaff)
        //{

        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblStaffSelect("fldId", idStaff.ToString(), 1, 1, "").FirstOrDefault();
        //    return Json(new
        //    {
        //        StaffName = q.fldName + " " + q.fldFamily,
        //    }, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldSenderComision" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblSubstituteSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 83))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblSubstituteDelete(Convert.ToInt32(id), 1, "");
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
        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblSubstituteSelect("fldId", id.ToString(), 1, 1, "").FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldType = q.fldSenderComisionID,
                    fldReceiverComisionID=q.fldReceiverComisionID,
                    fldStartDate=q.fldStartDate,
                    fldEndDate=q.fldEndDate,
                    fldStartTimeH=q.fldStartTime.Hours,
                    fldStartTimeM = q.fldStartTime.Minutes,
                    fldEndTimeH=q.fldEndTime.Hours,
                    fldEndTimeM = q.fldEndTime.Minutes,
                    fldIsSigner = q.fldIsSigner,
                    fldShowReceiverName=q.fldShowReceiverName,
                    fldDesc = q.fldDesc,
                    fldStaffName=q.fldStaffName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
