using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using Ext.Net.Utilities;
using System.Text;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers
{
    public class ChatController : Controller
    {
        //
        // GET: /Chat/
        public ActionResult Index()
        {//باز شدن تب جدید
            if (Session["flag"].ToString() == "1")
            {

                ViewBag.TabName = Session["TabName"].ToString();
                ViewBag.FriendId = Session["FriendId"].ToString();
                ViewBag.Group = Session["Group"].ToString();
                Session["flag"] = 0;
                return PartialView();
            }
            else return null;
        }
        public ActionResult AddFriends(int State, int? Gid)
        {//باز شدن پنجره
            
            ViewBag.State = State;
            ViewBag.Gid = Gid;
            return PartialView();
        }
        public ActionResult LoadNodeId(int ID, string Ch)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            Session["flag"] = 1;
            if (Ch == "F")
            {// دوست
                var f = m.sp_tblFriendsSelect("fldID", ID.ToString(), 0, Convert.ToInt32(Session["UserId"])).FirstOrDefault();
                var q=m.sp_tblUserSelect("fldID", f.fldFriendsUserId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                Session["TabName"] = q.fldStaffName;
                Session["FriendId"] = q.fldID;
                Session["Group"] = 0;
            }
            if (Ch == "R")
            {// دوست
                var f = m.sp_tblGroupFriendsSelect("fldID", ID.ToString(), 0).FirstOrDefault();
                var q = m.sp_tblUserSelect("fldID", f.fldUserID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                Session["TabName"] = q.fldStaffName;
                Session["FriendId"] = q.fldID;
                Session["Group"] =0;
            }
            else if (Ch == "G")
            {// گروه
                var q = m.sp_tblGroupsSelect("fldID", ID.ToString(), 0, Convert.ToInt32(Session["UserId"])).FirstOrDefault();
                Session["TabName"] = q.fldName;
                Session["FriendId"] = q.fldId;
                Session["Group"] = 1;
            }
            return Json(new { FriendId = Session["FriendId"], TabName = Session["TabName"] }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadChat(int FriendId,int UID,int h)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var Date = m.sp_GetDate().FirstOrDefault().fldDateTime;
            string ChatString="";
            var f = m.sp_tblChatGroupSelect(h, UID, FriendId).ToList();
            foreach (var Item in f)
                ChatString += "<li>" + Item.fldSenderName + ": " + Item.fldMatneMessage + "</li>";

            var ff = m.sp_tblChatSingleSelect("fldReceiverUserID", UID.ToString(), "", 0).Where(l => l.fldSenderUserID == FriendId && l.fldReadDate == null).ToList();
            foreach(var _Item in ff)
                m.sp_tblChatSingle_ReadDateUpdate(_Item.fldId, Date);

            return Json(new
            {
                ChatString = ChatString
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TabName(int userid)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblUserSelect("fldID", userid.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
           
            return Json(new
            {
                Name = q.fldStaffName
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Save(Models.sp_tblChatSingleSelect chat)
        {
            string Msg = "", MsgTitle = "";
            try
            {
                Models.AutomationEntities m = new Models.AutomationEntities();
                DateTime? fldReadDate = null;
                if (chat.fldDesc == null)
                    chat.fldDesc = "";
                if (chat.fldReadDate != null)
                    fldReadDate = MyLib.Shamsi.Shamsi2miladiDateTime(chat.fldReadDate);
                    //ذخیره
                m.sp_tblChatSingleInsert(Convert.ToInt32(Session["UserId"]), chat.fldReceiverUserID, chat.fldGroupReceiverId, fldReadDate, chat.fldMatneMessage, Convert.ToInt32(Session["UserId"]), chat.fldDesc);
            }
            catch (Exception x)
            {
                if (x.InnerException != null)
                    Msg = x.InnerException.Message;
                else
                    Msg = x.Message;

                MsgTitle = "خطا";
            }
            return Json(new
            {
                Msg = Msg,
                MsgTitle = MsgTitle
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblUserSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k=>k.fldActive_Deactive==true).ToList().ToDataSourceResult(request);
            return Json(q);
        }
        public ActionResult Read(StoreRequestParameters parameters)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var filterHeaders = new FilterHeaderConditions(this.Request.Params["filterheader"]);

            List<Models.sp_tblUserSelect> data = null;
            if (filterHeaders.Conditions.Count > 0)
            {
                string field = "";
                string searchtext = "";
                List<Models.sp_tblUserSelect> data1 = null;
                foreach (var item in filterHeaders.Conditions)
                {
                    var ConditionValue = (Newtonsoft.Json.Linq.JValue)item.ValueProperty.Value;

                    switch (item.FilterProperty.Name)
                    {
                        case "fldID":
                            searchtext = ConditionValue.Value.ToString();
                            field = "fldID";
                            break;
                        case "fldUserName":
                            searchtext = ConditionValue.Value.ToString();
                            field = "fldUserName";
                            break;

                    }
                    if (data != null)
                        data1 = p.sp_tblUserSelect(field, searchtext, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                    else
                        data = p.sp_tblUserSelect(field, searchtext, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                }
                if (data != null && data1 != null)
                    data.Intersect(data1);
            }
            else
            {
                data = p.sp_tblUserSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            }

            var fc = new FilterHeaderConditions(this.Request.Params["filterheader"]);

            //FilterConditions fc = parameters.GridFilters;

            //-- start filtering ------------------------------------------------------------
            if (fc != null)
            {
                foreach (var condition in fc.Conditions)
                {
                    string field = condition.FilterProperty.Name;
                    var value = (Newtonsoft.Json.Linq.JValue)condition.ValueProperty.Value;

                    data.RemoveAll(
                        item =>
                        {
                            object oValue = item.GetType().GetProperty(field).GetValue(item, null);
                            return !oValue.ToString().Contains(value.ToString());
                        }
                    );
                }
            }
            //-- end filtering ------------------------------------------------------------

            //-- start paging ------------------------------------------------------------
            int limit = parameters.Limit;

            if ((parameters.Start + parameters.Limit) > data.Count)
            {
                limit = data.Count - parameters.Start;
            }

            List<Models.sp_tblUserSelect> rangeData = (parameters.Start < 0 || limit < 0) ? data : data.GetRange(parameters.Start, limit);
            //-- end paging ------------------------------------------------------------

            return this.Store(rangeData, data.Count);
        }
        public ActionResult Submit(string selection)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var F = selection.Split(';');

            for(int i=0;i<F.Length-1;i++)
            {
                var q = m.sp_tblFriendsSelect("fldFriendsUserId", F[i].ToString(), 0, Convert.ToInt32(Session["UserId"])).FirstOrDefault();
                if (q == null)
                {
                    m.sp_tblFriendsInsert(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(F[i]), "");
                    m.sp_tblFriendsInsert(Convert.ToInt32(F[i]), Convert.ToInt32(Session["UserId"]), "");
                }
            }
            return Json(new
            {
                data = "درخواست با موفقیت انجام شد.",
                state = 0
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SubmitGroup(string selection, string GroupName, int fldId)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            string Msg = "", MsgTitle = "";
            if (fldId == 0)
            {
                System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                var Friend = m.sp_tblUserSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                SelectedRowCollection src = JSON.Deserialize<SelectedRowCollection>(selection);

                m.sp_tblGroupsInsert(_id, GroupName, Convert.ToInt32(Session["UserId"]), "");
                foreach (SelectedRow row in src)
                    m.sp_tblGroupFriendsInsert(Convert.ToInt32(_id.Value), Friend[row.RowIndex].fldID, "");

                Msg = "درخواست با موفقیت انجام شد.";
                MsgTitle = "ذخیره موفق";
            }
            else
            {
                var q = m.sp_tblGroupFriendsSelect("fldGroupId", fldId.ToString(), 0).ToList();
                foreach (var Item in q)
                    m.sp_tblGroupFriendsDelete(Item.fldId, Convert.ToInt32(Session["UserId"]));

                var Friend = m.sp_tblUserSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                SelectedRowCollection src = JSON.Deserialize<SelectedRowCollection>(selection);

                m.sp_tblGroupsUpdate(fldId, GroupName, Convert.ToInt32(Session["UserId"]), "");
                foreach (SelectedRow row in src)
                    m.sp_tblGroupFriendsInsert(fldId, Friend[row.RowIndex].fldID, "");

                Msg = "ویرایش با موفقیت انجام شد.";
                MsgTitle = "ویرایش موفق";
            }
            return Json(new
            {
                Msg = Msg,
                MsgTitle = MsgTitle
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DelFriends(int id, string Ch)
        {//حذف یک رکورد
            try
            {
                Models.AutomationEntities m = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    if (Ch == "F")//حذف دوست
                    {
                        var z = m.sp_tblFriendsSelect("fldId", id.ToString(), 1, 0).FirstOrDefault();

                        m.sp_tblFriendsDeleteFriendId(z.fldFriendsUserId, z.fldUserID);
                    }
                    else if (Ch == "R")//حذف دوست از گزوه
                        m.sp_tblGroupFriendsDelete(id, Convert.ToInt32(Session["UserId"]));
                    else if (Ch == "G")//حذف گروه
                    {
                        var q = m.sp_tblGroupFriendsSelect("fldGroupId", id.ToString(), 0).ToList();
                        foreach (var Item in q)
                            m.sp_tblGroupFriendsDelete(Item.fldId, Convert.ToInt32(Session["UserId"]));
                        m.sp_tblGroupsDelete(id, Convert.ToInt32(Session["UserId"]));
                    }
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
           
        public ActionResult Details(int Gid)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblGroupsSelect("fldId", Gid.ToString(), 0, Convert.ToInt32(Session["UserId"])).FirstOrDefault();
            var q1 = p.sp_tblGroupFriendsSelect("fldGroupId", Gid.ToString(), 0).ToList();
            int[] checkedNodes = new int[q1.Count];
            int[] ID = new int[q1.Count];
            for (int i = 0; i < q1.Count; i++)
            {
                checkedNodes[i] = Convert.ToInt32(q1[i].fldUserID);
            }
            return Json(new
            {
                fldId = q.fldId,
                fldName = q.fldName,
                checkedNodes = checkedNodes
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult HavePm(int UID)
        {
            
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_ChatSenderCount(UID).ToList();
                Boolean Have = false;
                string txt = "";
                string Le = "";
                int count = 0;
                int[] FId = new int[q.Count];
                if (q.Count() != 0)
                {
                    Have = true;
                    foreach (var Item in q)
                    {
                        txt = txt + "<li>" + Item.fldSenderName + " : " + Item.fldCount + " پیام " + "</li>";
                    }
                }
                if (Convert.ToInt32(Session["UserId"]) != 0)
                {
                    var staff = p.sp_tblUserSelect("fldId", UID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    var com = p.sp_tblCommisionSelect("fldStaffID", staff.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();

                    foreach (var Item in com)
                    {
                        var rols = (from k in p.sp_tblBoxSelect("", "", 0, 1, "")
                                    where k.fldComisionID == Item.fldID
                                    select k);
                        var ggg = rols.ToList();
                        for (int i = 0; i < com.Count(); i++)
                        {
                            if (ggg[i].fldBoxTypeID == 1)
                            {
                                var c = p.sp_LetterSelectInboxNotRead(ggg[i].fldID).ToList();
                                count = 0;
                                if (c.Count != 0)
                                {
                                    count = c.Count;
                                    Have = true;
                                }
                            }
                        }
                        Le = Le + "<li>کارتابل " + Item.fldOrganicRoleName + " : " + count + " نامه " + "</li>";
                    }
                }
                return Json(new
                {
                    Have = Have,
                    txt = txt,
                    Letter = Le
                }, JsonRequestBehavior.AllowGet);
            
        }
        public ActionResult Reload(string value)
        {//جستجو
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[0], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblUserSelect("fldStaffName", searchtext, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldActive_Deactive == true).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
    }
}
