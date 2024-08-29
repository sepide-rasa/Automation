using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;

namespace Automation.Helper
{
    public static class winClass
    {
        public static MvcHtmlString windowHeader(string id, string title, int width, int height)
        {
            return new MvcHtmlString("<script type='text/javascript'>$(document).ready(function () " +
                "{});</script><center style='position: fixed;top: 0;left: 0;z-index: 10001;width: 100%;height: 100%;background-image:url(/content/images/dialog-bg.png)' id='" +
                id + "'><div class='frame' style='width:" + width +
                "px; height:" + height + "px; position: fixed;  top: 50%;  left: 50%;opacity:1;  margin-top: -" +
                (height / 2).ToString() + "px;  margin-left: -" +
                (width / 2).ToString() + "px;'> <div class='wintitle' style='width:" + (width - 10) +
                "px'>" + title + "</div><div id='btnClose' title='خروج'>.</div><div class='frame1' style='width:" +
                (width - 10) + "px;height:" + (height - 30) + "px'> ");
        }
        
        public static MvcHtmlString textbox(string id)
        {
            return new MvcHtmlString("<input id='" + id + "' class='text' style='direction:rtl;font-family:tornado tahoma;width: 164px;height: 23px;'/>");
        }

        public static MvcHtmlString windowFother()
        {
            return new MvcHtmlString("</div></div></center>");
        }

        public static MvcHtmlString Buttons(string id, string text,string ButtonType)
        {
            return new MvcHtmlString("<button id='" + id + "' style=\"font-size:11px;direction:rtl;font-family:tornado tahoma;width: 80px;text-align: left;background-image: url('/Content/images/" + ButtonType + ".png');background-repeat: no-repeat;background-position-x: 95%;\">" + text + "</button>");
        }

        public static MvcHtmlString textArea(string id,int row,int col)
        {
            return new MvcHtmlString("<textarea id='" + id + "' class='text' cols=" + col + " rows=" + row + " style='direction:rtl;font-family:tornado tahoma;' />");
        }
    }
}