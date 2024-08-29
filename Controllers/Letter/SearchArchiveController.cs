using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class SearchArchiveController : Controller
    {
        //
        // GET: /SearchArchive/

        public ActionResult Index(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            ViewBag.Id = id;
            return PartialView();          
        }

        public JsonResult _ArchiveTree(int? id)
        {
            try
            {
                var p = new Models.AutomationEntities();

                if (id != null)
                {
                    var rols = (from k in p.sp_tblArchiveSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    Image = Url.Content("~/Content/images/") + "Archiv16.png",
                                    hasChildren = p.sp_tblArchiveSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblArchiveSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    Image = Url.Content("~/Content/images/") + "1376570130_folder_home2.png",
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

        public JsonResult LetterArchivePosition(int id)
        {
            Models.AutomationEntities Auto = new Models.AutomationEntities();
            var nodes = Auto.sp_tblArchiveSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            return Json(new { Position = nodes.fldName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInf(int idLetter)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterSelect("fldId", idLetter.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            long ArchiveId = 0;
            var archive = m.sp_tblLetterArchiveSelect("fldLetterID", idLetter.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (archive != null)
                ArchiveId = archive.fldID;
            return Json(new
            {
                Subject = q.fldSubject,
                ArchiveId = ArchiveId
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldArchiveName" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblLetterArchiveSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Save(Models.sp_tblLetterArchiveSelect Archive)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Archive.fldDesc == null)
                    Archive.fldDesc = "";
                if (Archive.fldID == 0)
                {//ثبت رکورد جدید
                    p.sp_tblLetterArchiveInsert(Convert.ToInt64(Archive.fldLetterID), Archive.fldArchiveID, Convert.ToInt32(Session["UserId"]), Archive.fldDesc, Session["UserPass"].ToString());
                    var letter = p.sp_tblLetterSelect("fldid", Archive.fldLetterID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    return Json(new { data = "نامه با شماره " + letter.fldOrderId + " در  " + Archive.fldArchiveName + " با موفقیت ذخیره شد.", state = 0 });
                }
                else
                {//ویرایش رکورد ارسالی
                    p.sp_tblLetterArchiveUpdate(Convert.ToInt64(Archive.fldID), Convert.ToInt64(Archive.fldLetterID), Archive.fldArchiveID, Convert.ToInt32(Session["UserId"]), Archive.fldDesc, Session["UserPass"].ToString());
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
                    Car.sp_tblLetterArchiveDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblArchiveSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldName = q.fldName,
                    fldId = q.fldID,
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
