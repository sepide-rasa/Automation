using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;

namespace Automation.Controllers.Tools
{
    [Authorize]
    public class ProfileController : Controller
    {
        // 
        // GET: /Profile/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 98))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var user = p.sp_tblUserSelect("fldid", Session["UserId"].ToString(), 0, 1, "").FirstOrDefault();
                ViewBag.StaffId = user.fldStaffID;
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }


        public ActionResult Save(Models.Staff Staff)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                byte[] image1 = null, image2 = null;

                

                if (Staff.fldDesc == null)
                    Staff.fldDesc = "";
                if (Staff.fldEmailAddress == null)
                    Staff.fldEmailAddress = "";
                if (Staff.fldMobile == null)
                    Staff.fldMobile = "";
                if (Staff.fldAddress == null)
                    Staff.fldAddress = "";
                if (Staff.fldId == 0)
                {//ثبت رکورد جدید
                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                }
                else
                {//ویرایش رکورد ارسالی 
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 109))
                    {
                        var q = p.sp_tblStaffSelect("fldId", Staff.fldId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (Staff.fldStaffPicture != null)
                            image1 = Automation.Helper.ClsCommon.Base64ToImage(Staff.fldStaffPicture);
                        var pic = p.sp_tblPictureSelect("fldID", Staff.fldId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (pic.fldSignPicture != null)
                            image2 = pic.fldSignPicture;
                        p.sp_tblStaffUpdate(Staff.fldId, Staff.fldName, Staff.fldFamily, Staff.fldMelliCode, Staff.fldNameFather, MyLib.Shamsi.Shamsi2miladiDateTime(Staff.fldBirthDate), Staff.fldEmailAddress, Staff.fldMobile, Staff.fldAddress, q.fldSign,q.fldNotify,Staff.fldLetterLoadNum, Convert.ToInt32(Session["UserId"]), Staff.fldDesc, Session["UserPass"].ToString());
                        var k = p.sp_tblPictureSelect("fldStaffID", Staff.fldId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        p.sp_tblPictureUpdate(k.fldID, Staff.fldId, image1, image2, Convert.ToInt32(Session["UserId"]), Staff.fldDesc, Session["UserPass"].ToString());
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
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
        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblStaffSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var k = p.sp_tblPictureSelect("fldStaffID", q.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                return Json(new
                {
                    fldID = q.fldID,
                    fldName = q.fldName,
                    fldFamily = q.fldFamily,
                    fldNameFather = q.fldNameFather,
                    fldMeliCode = q.fldMelliCode,
                    fldBirthDate = q.fldBirthDate,
                    fldEmailAddress = q.fldEmailAddress,
                    fldMobile = q.fldMobile,
                    fldAddress = q.fldAddress,
                    fldDesc = q.fldDesc,
                    fldNotify=q.fldNotify,
                    fldLetterLoadNum=q.fldLetterLoadNum,
                    fldpicId = k.fldID
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public FileContentResult StaffImage(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();

            var pic = p.sp_tblPictureSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldStaffPicture != null)
                {
                    return File((byte[])pic.fldStaffPicture, "jpg");
                }

            }
            return null;

        }
    }
}
