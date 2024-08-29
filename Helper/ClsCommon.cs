using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
namespace Automation.Helper
{
    public static class ClsCommon
    {
        public static byte[] Base64ToImage(string Base64String)
        {
            try
            {
                MemoryStream stream;
                if (isImage())
                {
                    stream = new MemoryStream(Convert.FromBase64String(Base64String));
                    return stream.ToArray();
                }
                else
                    return null;
            }catch(Exception x)
            {
                return null;
            }
        }

        private static bool  isImage()
        {
            return true;
        }
    }
}