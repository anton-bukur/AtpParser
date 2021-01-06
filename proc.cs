using AtpParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtpParser
{
    class proc
    {
        public static string GetMatchStatistics(string url)
        {
            url = "https://www.tennisexplorer.com/match-detail/?id=1864310";
            //var proxyHand = new WebProxy
            //{
            //    Address = null,
            //    BypassProxyOnLocal = false,
            //    UseDefaultCredentials = false
            //};

            var cookieContainer = new CookieContainer();

            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            HttpClient client = new HttpClient(handler);
            var Result = client.GetAsync(url).Result;
            var RespText = Result.Content.ReadAsStringAsync().Result;
            match sMatch  = new match();
            sMatch.Atp = Regex.Match(RespText,"").Value;

            return RespText;
        }
    }
}
