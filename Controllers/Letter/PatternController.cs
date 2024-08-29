using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class PatternController : Controller
    {
        //
        // GET: /Pattern/

        public ActionResult Index(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 37))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblLetterPatternSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (q != null)
                    ViewBag.HaveFile = 1;
                else
                    ViewBag.HaveFile = 0;
                ViewBag.id = id;
                ViewBag.SiteURL = "http://" + Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Doc(int id)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblLetterPatternSelect("fldPatternID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (q != null)
            {
                Response.Write("Get Stream Successfully!");
                Response.Write("EDA_STREAMBOUNDARY");
                Response.BinaryWrite(q.fldPatternFile);
                Response.Write("EDA_STREAMBOUNDARY");
            }
            return null;
        }

        public ActionResult Save(int id)
        {
            if (Request.Files.Count > 0)
            {
                string filename = Request.Files[0].FileName;
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "docs\\" + filename;
                Request.Files[0].SaveAs(filePath);
                MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(filePath));
                byte[] _File = stream.ToArray();
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblLetterPatternSelect("fldPatternID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (q == null)
                {
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 46))
                    {
                        p.sp_tblLetterPatternInsert(id, _File, Convert.ToInt32(Session["UserId"]),"", Session["UserPass"].ToString());
                        System.IO.File.Delete(filePath);
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
                else
                {
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 47))
                    {
                        p.sp_tblLetterPatternUpdate(q.fldID, id, _File, Convert.ToInt32(Session["UserId"]),"", Session["UserPass"].ToString());
                        System.IO.File.Delete(filePath);
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
            }
            return null;
        }
    }
}
