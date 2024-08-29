using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.BasicInf
{
    public class ApplicationPartController : Controller
    {
        //
        // GET: /ApplicationPart/

        public ActionResult Index()
        {
            return PartialView();
        }

        public JsonResult _RolsTree(int? id)
        {
            try
            {
                var p = new Models.AutomationEntities();

                if (id != null)
                {
                    var rols = (from k in p.sp_tblApplicationPartSelect("fldPID", id.ToString(), 0)
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldTitle,
                                    hasChildren = p.sp_tblApplicationPartSelect("fldPID", id.ToString(), 0).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblApplicationPartSelect("", "", 0)
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldTitle,
                                    hasChildren = p.sp_tblApplicationPartSelect("", "", 0).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public JsonResult Save(Models.sp_tblApplicationPartSelect AppPart)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (AppPart.fldDesc == null)
                    AppPart.fldDesc = "";
                if (AppPart.fldID == 0)
                {//ثبت رکورد جدید
                    p.sp_tblApplicationPartInsert(AppPart.fldTitle, AppPart.fldPID, 1, AppPart.fldDesc);
                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                }
                else
                {//ویرایش رکورد ارسالی
                    p.sp_tblApplicationPartUpdate(AppPart.fldID, AppPart.fldTitle, AppPart.fldPID, 1, AppPart.fldDesc);
                    return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
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
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblApplicationPartDelete(Convert.ToInt32(id), 1);
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
        public JsonResult Details(int id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblApplicationPartSelect("fldID", id.ToString(), 1).FirstOrDefault();
                return Json(new
                {
                    fldTitle = q.fldTitle,
                    fldID = q.fldID,
                    fldPId = q.fldPID
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }

    }
}
