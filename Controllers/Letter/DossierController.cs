using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
namespace Automation.Controllers
{
    [Authorize]
    public class DossierController : Controller
    {
        //
        // GET: /Dossier/

        public ActionResult Index()
        {
            //ViewBag.Staffid = idStaff;int idStaff
            return PartialView();
        }
        public ActionResult GetHistoryType()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblHistoryTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult FillDossier([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblAssignmentTypeSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
        //    return Json(q);
        //}

        //public ActionResult FillSearch([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblAssignmentTypeSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
        //    return Json(q);
        //}

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldLetterNumber", "fldSubject" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterSelect(_fiald[Convert.ToInt32(field)], searchtext, top, 1, "").ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblAssignmentTypeDelete(Convert.ToInt32(id), 1, "");
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
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblAssignmentTypeSelect("fldId", id.ToString(), 1, 1, "").FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldType = q.fldType,
                    fldDesc = q.fldDesc
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
