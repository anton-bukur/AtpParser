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
            string tStr; int i = 0;
            url = "https://www.tennisexplorer.com/match-detail/?id=1864302";
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

            tStr = Regex.Match(RespText, "(?<=<div class=\"box boxBasic lGray\"><span class=\"upper\">)[\\w\\W]*?(?=<iframe)").Value.Replace(" ", "");
            match sMatch  = new match();
            //sMatch.Atp = Regex.Match(RespText,"").Value;
            string[] tStrArr = tStr.Split(',');
            sMatch.Date = tStrArr[0].Substring(0, 10);
            sMatch.Surface = tStrArr[4];
            sMatch.Round = tStrArr[3];
            MatchCollection players = Regex.Matches(RespText, "(?<=<th class=\"plName\" colspan=\"2\">)[\\w\\W]*?(?=</a)");
            sMatch.Winner = players[0].Value.Split('>')[1];
            sMatch.Loser = players[1].Value.Split('>')[1];
            tStr = Regex.Match(RespText, "(?<=<td class=\"gScore\">)[\\w\\W]*?(?=\\)</span></td>)").Value;
            tStrArr = tStr.Split('(');
            sMatch.Wsets = tStrArr[0].Split('<')[0].Split(':')[0].Replace(" ", "");
            sMatch.Lsets = tStrArr[0].Split('<')[0].Split(':')[1].Replace(" ", "");
            tStrArr = tStrArr[1].Split(',');
            sMatch.W = new string[5];
            sMatch.L = new string[5];
            for (i=0; i< tStrArr.Length; i++)
            {
                tStr = tStrArr[i].Split('-')[0].Replace(" ", "");
                sMatch.W[i] = tStrArr[i].Split('-')[0].Replace(" ","");
                sMatch.L[i] = tStrArr[i].Split('-')[1].Replace(" ", "");
            }
            return RespText;
        }
    }
}
