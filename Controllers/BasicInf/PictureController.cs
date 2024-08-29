using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class PictureController : Controller
    {
        //
        // GET: /Picture/

        public ActionResult Index(int fldStaffID)
        {
            ViewBag.fldStaffID = fldStaffID;          
            return PartialView();            
        }

        public JsonResult GetInf(int fldStaffID)
        {

            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblStaffSelect("fldID", fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            return Json(new { Name = q.fldName }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Save(Models.Picture Picture)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                byte[] image1 = null, image2 = null;

                if (Picture.fldStaffPicture != null)
                    image1 = Automation.Helper.ClsCommon.Base64ToImage(Picture.fldStaffPicture);

                if (Picture.fldSignPicture != null)
                    image2 = Automation.Helper.ClsCommon.Base64ToImage(Picture.fldSignPicture);

                if (Picture.fldID == 0)
                {//ثبت رکورد جدید

                    p.sp_tblPictureInsert(Picture.fldStaffID, image1, image2, Convert.ToInt32(Session["UserId"]), Picture.fldDesc, Session["UserPass"].ToString());

                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });

                }
                else
                {//ویرایش رکورد ارسالی                    
                    p.sp_tblPictureUpdate(Picture.fldID, Picture.fldStaffID, image1, image2, Convert.ToInt32(Session["UserId"]), Picture.fldDesc, Session["UserPass"].ToString());

                    return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });

                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldStaffName" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblPictureSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblPictureDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblPictureSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,                    
                    fldDesc = q.fldDesc,

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public FileContentResult Image(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();

            var pic = p.sp_tblPictureSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldSignPicture != null)
                {
                    return File((byte[])pic.fldSignPicture, "jpg");
                }
                if (pic.fldStaffPicture != null)
                {
                    return File((byte[])pic.fldStaffPicture, "jpg");
                }
                
            }
            return null;

        }
    }
}
