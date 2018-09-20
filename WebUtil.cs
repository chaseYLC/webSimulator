using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webSimulator
{
    public class WebUtil
    {
        public static string WebText(string url, out string err)
        {
            err = "";

            if (url == "")
            {
                return "";
            }

            string html = "";

            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.12) Gecko/20100824 Firefox/3.5.12x";
#if USE_GET_PHPSESSION
                    client.Headers[HttpRequestHeader.Cookie] = "PHPSESSID=" + _myPhpSessionID;
#endif
                    client.Encoding = Encoding.UTF8;

                    html = client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return (html == "" ? "" : html.Trim());
        }
    }
}
