using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.Tools
{
    public class MailContactController : Controller
    {
        //
        // GET: /MailContact/

        public ActionResult Index(int State, int LetterID, string Name, string Email,int? RoneveshtType)
        {
            ViewBag.State = State;
            ViewBag.LetterID = LetterID;
            string ReciverName = "";
            string EmailAddress = "";
            Models.AutomationEntities m = new Models.AutomationEntities();
            if (State != 2 & State != 3)
            {
                ViewBag.RoneveshtType = RoneveshtType;
                var Ex = m.sp_tblExternalLetterReceiverSelect("fldLetterID", LetterID.ToString(), 0).ToList();
                foreach (var Item in Ex)
                {
                    ReciverName = ReciverName + Item.fldName + ";";
                    if (Item.fldEmailAddress == null)
                        Item.fldEmailAddress = "";
                    EmailAddress = EmailAddress + Item.fldEmailAddress + ";";
                }

                var In = m.sp_tblInternalLetterReceiverSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
                foreach (var _Item in In)
                {
                    ReciverName = ReciverName + _Item.fldStaffName + ";";
                    if (_Item.fldEmailAddress == null)
                        _Item.fldEmailAddress = "";
                    EmailAddress = EmailAddress + _Item.fldEmailAddress + ";";
                }

                var Ru = m.sp_tblRoneveshtSelect("fldLetterID", LetterID.ToString(), 0).ToList();
                foreach (var item in Ru)
                {
                    ReciverName = ReciverName + item.fldGirande + ";";
                    if (item.fldEmailAddress == null)
                        item.fldEmailAddress = "";
                    EmailAddress = EmailAddress + item.fldEmailAddress + ";";
                }
            }
            else
            {
                ReciverName = Name;
                EmailAddress = Email;
            }

            ViewBag.ReciverName = ReciverName;
            ViewBag.EmailAddress = EmailAddress;

            return PartialView();
        }
        public ActionResult ReloadGride(string ReciverName, string EmailAddress)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();

            List<Models.EmailContact> groups = new List<Models.EmailContact>();
            var Reciver = ReciverName.Split(';');
            var Email = EmailAddress.Split(';');

            for (byte i = 0; i < Reciver.Length - 1; i++)
            {
                Models.EmailContact S = new Models.EmailContact();
                S.fldReciverName = Reciver[i];
                S.fldEmailAddress = Email[i];
                groups.Add(S);
            }
            return Json(groups, JsonRequestBehavior.AllowGet);
            //}
        }

        public JsonResult ChekHaveMail(int state, int LetterID)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                string ReciverName = "";
                string EmailAddress = "";
                Models.AutomationEntities m = new Models.AutomationEntities();
           
                var In = m.sp_tblInternalLetterReceiverSelect("fldLetterID", LetterID.ToString(), 0, 0, "").ToList();
                foreach (var _Item in In)
                {
                    var k = m.sp_tblCommisionSelect("fldId", _Item.fldReceiverComisionID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    var q = m.sp_tblEmailSelect("fldStaffID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    var q2 = m.sp_tblStaffSelect("fldID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                    if (q != null)
                        if (q.fldSendTrue_False)
                        {
                            ReciverName = ReciverName + _Item.fldStaffName + ";";
                            if (_Item.fldEmailAddress == null)
                                _Item.fldEmailAddress = "";
                            EmailAddress = EmailAddress + _Item.fldEmailAddress + ";";
                        }
                }

                var Ru = m.sp_tblRoneveshtSelect("fldLetterID", LetterID.ToString(), 0).ToList();
                foreach (var item in Ru)
                {
                    if (item.fldExternalPartnerId == null)
                    {
                        var k = m.sp_tblCommisionSelect("fldId", item.fldComID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        var q = m.sp_tblEmailSelect("fldStaffID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        var q2 = m.sp_tblStaffSelect("fldID", k.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                        if (q != null)
                            if (q.fldSendTrue_False)
                            {
                                ReciverName = ReciverName + item.fldGirande + ";";
                                if (item.fldEmailAddress == null)
                                    item.fldEmailAddress = "";
                                EmailAddress = EmailAddress + item.fldEmailAddress + ";";
                            }
                    }
                }
                return Json(new
                {
                    Name = ReciverName,
                    Email = EmailAddress
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult EmailOrEce(int state,int ID)
        {
            ViewBag.state = state;
            ViewBag.ID = ID;
            return PartialView();
        }
    }
}
