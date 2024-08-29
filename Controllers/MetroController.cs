using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers
{
    public class MetroController : Controller
    {
        //
        // GET: /Metro/

        public ActionResult error()
        {
            return PartialView();
        }

        public ActionResult YesNomsg(string id,string url)
        {
            ViewBag.ID = id;
            ViewBag.URL = url;
            return PartialView();
        }
        public ActionResult YesNomsgWin(string id, string url)
        {
            ViewBag.ID = id;
            ViewBag.URL = url;
            return PartialView();
        }
        public ActionResult YesNomsgArchiveBox(string LetterId,string ArchivID, string url)
        {
            ViewBag.LetterId = LetterId;
            ViewBag.ArchivID = ArchivID;
            ViewBag.URL = url;
            return PartialView();
        }

        public ActionResult YesNomsgDeleteBox(string BoxId, string SelectedLetterId)
        {
            ViewBag.BoxId = BoxId;
            ViewBag.SelectedLetterId = SelectedLetterId;
            return PartialView();
        }
        public ActionResult YesNomsgFriendChat(string id,string ch,string url)
        {
            ViewBag.ID = id;
            ViewBag.ch = ch;
            ViewBag.URL = url;
            return PartialView();
        }
        public ActionResult Window()
        {
            return PartialView();
        }
    }
}
