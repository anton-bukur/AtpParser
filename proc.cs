﻿using AtpParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;


namespace AtpParser
{

    class proc
    {
        const string homeUrl = "https://www.tennisexplorer.com/";
        //private static object excelborderStyle;

        public static List<string> GetTournaments(string url)
        {
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
            var tMatches = Regex.Matches(RespText, "(?<=<th class=\"t-name\" rowspan=\"2\"><a href=\")[\\w\\W]*?(?=\")");
            //List<string> listTournaments = new List<string>();
            var listTournaments = new List<string>().Concat(tMatches.Cast<Match>().Select(x => x.Value).ToList()).ToList();
            return listTournaments;
        }
        public static List<string> GetMatches(string url)
        {
            //url = "https://www.tennisexplorer.com/doha/2020/atp-men/";
            var cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            HttpClient client = new HttpClient(handler);
            string RespText = "";
            try
            {
                var Result = client.GetAsync(url).Result;
                RespText = Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                File.AppendAllText("lost_Tournaments.txt", url + Environment.NewLine);
                Thread.Sleep(5000);
                return null;

            }
            var tMatches = Regex.Matches(RespText, "(?<=<td rowspan=\"2\"><a href=\")[\\w\\W]*?(?=\")");
            string Tournament = Regex.Match(RespText, "(?<=<h1 class=\"bg\">)[\\w\\W]*?(?=<)").Value;
            List<string> listMatches = new List<string>();
            listMatches = listMatches.Concat(tMatches.Cast<Match>().Select(x => x.Value).ToList()).ToList();
            //и ещё такие матчи честь, оказывается
            tMatches = Regex.Matches(RespText, "(?<=<td><a href=\")[\\w\\W]*?(?=\")");
            listMatches = listMatches.Concat(tMatches.Cast<Match>().Select(x => x.Value).ToList()).ToList();
            //последний член типа - имя турнира
            listMatches.Add(Tournament);
            return listMatches;
        }
        public static string GetMatchStatistics(string url, string Tournament)
        {
            //!!Временно, потом сюда передать надо!!!
            //url = "https://www.tennisexplorer.com/match-detail/?id=1862730";
            string tStr; int i = 0;

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
            string RespText = "";

            try
            {
                var Result = client.GetAsync(url).Result;
                RespText = Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception )
            {
                File.AppendAllText("lost_Matches.txt", url + Environment.NewLine);
                Thread.Sleep(3000);
                return null;
            }

            tStr = Regex.Match(RespText, "(?<=<div class=\"box boxBasic lGray\"><span class=\"upper\">)[\\w\\W]*?(?=<iframe)").Value.Replace(" ", "");
            match sMatch = new match();
            //sMatch.Atp = Regex.Match(RespText,"").Value;
            string[] tStrArr = tStr.Split(',');
            sMatch.Date = tStrArr[0].Substring(0, 10);
            //когда время матча нету, ой!
            var cnt = tStrArr.Length;
            sMatch.Surface = tStrArr[cnt - 1];
            sMatch.Round = tStrArr[cnt - 2];
            MatchCollection players = Regex.Matches(RespText, "(?<=<th class=\"plName\" colspan=\"2\">)[\\w\\W]*?(?=</a)");
            sMatch.Winner = players[0].Value.Split('>')[1];
            sMatch.Loser = players[1].Value.Split('>')[1];
            var tables = Regex.Matches(RespText, "(?<=<tbody>)[\\w\\W]*?(?=</tbody>)"); // полезная коллекция, Слава Богу, тут вся статистика лежит практически
            tStr = Regex.Match(tables[0].Value, "(?<=<td class=\"thumb)[\\w\\W]*?(?=<td class=\"thumb)").Value;
            var tRanks = Regex.Matches(tStr, "(?<=class=\"t)[\\w\\W]*?(?=\\.)");
            sMatch.WRank = tRanks[0].Value.Split('>')[1];
            sMatch.LRank = tRanks[1].Value.Split('>')[1];
            string res = $"{Tournament},{sMatch.Date},{sMatch.Surface},{sMatch.Round},{sMatch.Winner},{sMatch.Loser},{sMatch.WRank},{sMatch.LRank},";

            tStr = Regex.Match(RespText, "(?<=<td class=\"gScore\">)[\\w\\W]*?(?=\\)</span></td>)").Value;
            if (tStr.Length > 0)
            {
                tStrArr = tStr.Split('(');
                sMatch.Wsets = tStrArr[0].Split('<')[0].Split(':')[0].Replace(" ", "");
                sMatch.Lsets = tStrArr[0].Split('<')[0].Split(':')[1].Replace(" ", "");
                tStrArr = tStrArr[1].Split(',');
                sMatch.W = new string[5];
                sMatch.L = new string[5];
                //нехорошо для памяти, но пох
                sMatch.WT = new string[5];
                sMatch.LT = new string[5];
                for (i = 0; i < tStrArr.Length; i++)
                {
                    tStr = tStrArr[i].Split('-')[0].Replace(" ", "");
                    sMatch.W[i] = tStrArr[i].Split('-')[0].Replace(" ", "");
                    //Тай-брейк, однако!
                    if (sMatch.W[i].Contains("<sup>"))
                    {
                        sMatch.WT[i] = Regex.Match(sMatch.W[i], "(?<=<sup>)[\\w\\W]*?(?=</sup>)").Value;
                        if (Int32.Parse(sMatch.WT[i]) < 6)
                            sMatch.LT[i] = "7";
                        else sMatch.LT[i] = (Int32.Parse(sMatch.WT[i]) + 2).ToString();
                        sMatch.W[i] = sMatch.W[i].Split('<')[0];
                    }

                    sMatch.L[i] = tStrArr[i].Split('-')[1].Replace(" ", "");
                    //Loser's Tie break
                    if (sMatch.L[i].Contains("<sup>"))
                    {
                        sMatch.LT[i] = Regex.Match(sMatch.L[i], "(?<=<sup>)[\\w\\W]*?(?=</sup>)").Value;
                        if (Int32.Parse(sMatch.LT[i]) < 6)
                            sMatch.WT[i] = "7";
                        else sMatch.WT[i] = (Int32.Parse(sMatch.LT[i]) + 2).ToString();
                        sMatch.L[i] = sMatch.L[i].Split('<')[0];
                    }
                }
                for (i = 0; i < 5; i++)
                    res = res + $"{sMatch.W[i]},{sMatch.L[i]},";
                for (i = 0; i < 5; i++)
                    res = res + $"{sMatch.WT[i]},{sMatch.LT[i]},";
                res = res + $"{sMatch.Wsets},{sMatch.Lsets},";
            }

            // Betting Odds пошли - самое интересное
            tStr = Regex.Match(Regex.Match(RespText, "(?<=oddsMenu-')[\\w\\W]*?(?=</tbody>)").Value, "(?<=</tr>)[\\w\\W]*").Value;
            //Home/Away
            var bookMakers = Regex.Matches(tStr, "(?<=<span class=\"t\">)[\\w\\W]*?(?=<)");
            if (bookMakers.Count == 0)
            {
                File.AppendAllText("log.txt", " No Bookmakers found " + url + Environment.NewLine);
                return res + "No Bookmakers found";
            }

            var betWin = Regex.Matches(tStr, "(?<=<td class=\"k1)[\\w\\W]*?(?=</div></td>)");
            var betLos = Regex.Matches(tStr, "(?<=<td class=\"k2)[\\w\\W]*?(?=</div></td>)");
            string WopenDate = null, Wdelta = null;

            res = res + "Home/Away,";
            for (i = 0; i < bookMakers.Count; i++)
            {
                res = res + $"{bookMakers[i]},";
                string WfinDate = Regex.Match(betWin[i].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                if (WfinDate.Length > 0)
                {
                    WopenDate = Regex.Match(betWin[i].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                    MatchCollection wBets = Regex.Matches(betWin[i].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                    Wdelta = Regex.Matches(betWin[i].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];
                    res = res + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                }
                else // ставка не менялась или отменена!
                {
                    var wBet = Regex.Match(betWin[i].Value, "(?<=n\">)[\\w\\W]*").Value;
                    var lBet = Regex.Match(betLos[i].Value, "(?<=n\">)[\\w\\W]*").Value;
                    if (betWin[i].Value.Contains("deactivated"))
                    {
                        res = res + $"deactivated,{wBet},{lBet},,,,,,,,";
                        continue; //Больше не надо, и так всё ясно
                    }
                    else
                    {
                        res = res + $"unchanged,{wBet},,,,";
                    }

                }
                string LfinDate = Regex.Match(betLos[i].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                if (LfinDate.Length > 0)
                {
                    string LopenDate = Regex.Match(betLos[i].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                    MatchCollection lBets = Regex.Matches(betLos[i].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                    string Ldelta = Regex.Matches(betLos[i].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];
                    //Home/Away,Bookmaker,WopenDate,wOpenBet,wfinDate,WfinBet,Wdelta, LopenDate,lOpenBet,LfinDate,lfinBet,Ldelta
                    res = res + $"{ LopenDate},{lBets[1].Value},{LfinDate},{lBets[0].Value},{Ldelta},";
                }
                else //не менялась
                {
                    var lBet = Regex.Match(betLos[i].Value, "(?<=n\">)[\\w\\W]*").Value;
                    res = res + $"unchanged,{lBet},,,,";
                }
            }

            //пошли остальное собирать
            foreach (var t in tables.Cast<Match>().Select(f => f.Value))
            {
                if (t.Contains("Over/Under")) res = GetOverUnder(t);
                if (t.Contains("Asian Handicap")) res = GetAsianHandicap(t);
                if (t.Contains("Correct Score")) res = GetCorrectScore(t);
            }

            string GetOverUnder(string t)
            {
                var tBets = Regex.Matches(t, "(?<=<tr class=\"odds-type\">)[\\w\\W]*?(?=<tr class=\"average\">)");
                res = res + "OVER/UNDER,";
                foreach (var tb in tBets.Cast<Match>().Select(f => f.Value))
                {
                    var OverUnderType = Regex.Match(tb, "(?<=<td colspan=\"4\">)[\\w\\W]*?(?=<)").Value;
                    res = res + OverUnderType + ",";
                    string tRes = OverUnderType + Environment.NewLine;
                    bookMakers = Regex.Matches(tb, "(?<=<span class=\"t\">)[\\w\\W]*?(?=<)");
                    var betOver = Regex.Matches(tb, "(?<=td class=\"k1)[\\w\\W]*?(?=</div></td>)");
                    var betUnder = Regex.Matches(tb, "(?<=td class=\"k2)[\\w\\W]*?(?=</div></td>)");
                    //var betOver = Regex.Matches(tb, "(?<=<div class=\"odds-in odown\">)[\\w\\W]*?(?=></div></div></td>)");
                    //var betUnder = Regex.Matches(tb, "(?<=<div class=\"odds-in oup\">)[\\w\\W]*?(?=></div></div></td>)");
                    WopenDate = null; Wdelta = null;

                    for (i = 0; i < bookMakers.Count; i++)
                    {
                        res = res + $"{bookMakers[i]},";
                        tRes = tRes + $"{bookMakers[i]},";
                        string WfinDate = Regex.Match(betOver[i].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                        if (WfinDate.Length > 0)
                        {
                            WopenDate = Regex.Match(betOver[i].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                            MatchCollection wBets = Regex.Matches(betOver[i].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                            Wdelta = Regex.Matches(betOver[i].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];

                            res = res + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                            tRes = tRes + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                        }
                        else // ставка не менялась или отменена!
                        {
                            var wBet = Regex.Match(betOver[i].Value, "(?<=n\">)[\\w\\W]*").Value;
                            var lBet = Regex.Match(betUnder[i + 1].Value, "(?<=n\">)[\\w\\W]*").Value;
                            if (betOver[i].Value.Contains("deactivated"))
                            {
                                tRes = tRes + $"deactivated,{wBet},{lBet},,,,,,,," + Environment.NewLine;
                                res = res + $"deactivated,{wBet},{lBet},,,,,,,,";
                                continue; //Больше не надо, и так всё ясно
                            }
                            else
                            {
                                tRes = tRes + $"unchanged,{wBet},,,,";
                                res = res + $"unchanged,{wBet},,,,";
                            }

                        }
                        string LfinDate = Regex.Match(betUnder[i + 1].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                        if (LfinDate.Length > 0)
                        {
                            string LopenDate = Regex.Match(betUnder[i + 1].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                            MatchCollection lBets = Regex.Matches(betUnder[i + 1].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                            string Ldelta = Regex.Matches(betUnder[i + 1].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];
                            //Over/Under,Bookmaker,WopenDate,wOpenBet,wfinDate,WfinBet,Wdelta, LopenDate,lOpenBet,LfinDate,lfinBet,Ldelta
                            tRes = tRes + $"{ LopenDate},{lBets[1].Value},{LfinDate},{lBets[0].Value},{Ldelta}," + Environment.NewLine; ;
                            res = res + $"{ LopenDate},{lBets[1].Value},{LfinDate},{lBets[0].Value},{Ldelta},";
                        }
                        else //не менялась
                        {
                            var lBet = Regex.Match(betUnder[i + 1].Value, "(?<=n\">)[\\w\\W]*").Value;
                            tRes = tRes + $"unchanged,{lBet},,,," + Environment.NewLine;
                            res = res + $"unchanged,{lBet},,,,";
                        }
                    }
                    //File.AppendAllText("bet.txt", tRes + Environment.NewLine);
                }
                if (tBets.Count == 0)
                {
                    res = res + "Null,";
                }
                return res;
            }
            string GetAsianHandicap(string t)
            {
                var tBets = Regex.Matches(t, "(?<=<tr class=\"odds-type\">)[\\w\\W]*?(?=<tr class=\"average\">)");
                res = res + "ASIAN HANDICAP,";
                foreach (var tb in tBets.Cast<Match>().Select(f => f.Value))
                {
                    var OverUnderType = Regex.Match(tb, "(?<=<td colspan=\"4\">)[\\w\\W]*?(?=<)").Value;
                    res = res + OverUnderType + ",";
                    string tRes = OverUnderType + Environment.NewLine;
                    bookMakers = Regex.Matches(tb, "(?<=<span class=\"t\">)[\\w\\W]*?(?=<)");
                    var betOver = Regex.Matches(tb, "(?<=td class=\"k1)[\\w\\W]*?(?=</div></td>)");
                    var betUnder = Regex.Matches(tb, "(?<=td class=\"k2)[\\w\\W]*?(?=</div></td>)");
                    //var betOver = Regex.Matches(tb, "(?<=<div class=\"odds-in odown\">)[\\w\\W]*?(?=></div></div></td>)");
                    //var betUnder = Regex.Matches(tb, "(?<=<div class=\"odds-in oup\">)[\\w\\W]*?(?=></div></div></td>)");
                    WopenDate = null; Wdelta = null;

                    for (i = 0; i < bookMakers.Count; i++)
                    {
                        res = res + $"{bookMakers[i]},";
                        tRes = tRes + $"{bookMakers[i]},";
                        string WfinDate = Regex.Match(betOver[i].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                        if (WfinDate.Length > 0)
                        {
                            WopenDate = Regex.Match(betOver[i].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                            MatchCollection wBets = Regex.Matches(betOver[i].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                            Wdelta = Regex.Matches(betOver[i].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];

                            res = res + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                            tRes = tRes + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                        }
                        else // ставка не менялась или отменена!
                        {
                            var wBet = Regex.Match(betOver[i].Value, "(?<=n\">)[\\w\\W]*").Value;
                            var lBet = Regex.Match(betUnder[i + 1].Value, "(?<=n\">)[\\w\\W]*").Value;
                            if (betOver[i].Value.Contains("deactivated"))
                            {
                                tRes = tRes + $"deactivated,{wBet},{lBet},,,,,,,," + Environment.NewLine;
                                res = res + $"deactivated,{wBet},{lBet},,,,,,,,";
                                continue; //Больше не надо, и так всё ясно
                            }
                            else
                            {
                                tRes = tRes + $"unchanged,{wBet},,,,";
                                res = res + $"unchanged,{wBet},,,,";
                            }

                        }
                        string LfinDate = Regex.Match(betUnder[i + 1].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                        if (LfinDate.Length > 0)
                        {
                            string LopenDate = Regex.Match(betUnder[i + 1].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                            MatchCollection lBets = Regex.Matches(betUnder[i + 1].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                            string Ldelta = Regex.Matches(betUnder[i + 1].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];
                            //Over/Under,Bookmaker,WopenDate,wOpenBet,wfinDate,WfinBet,Wdelta, LopenDate,lOpenBet,LfinDate,lfinBet,Ldelta
                            tRes = tRes + $"{ LopenDate},{lBets[1].Value},{LfinDate},{lBets[0].Value},{Ldelta}," + Environment.NewLine; ;
                            res = res + $"{ LopenDate},{lBets[1].Value},{LfinDate},{lBets[0].Value},{Ldelta},";
                        }
                        else //не менялась
                        {
                            var lBet = Regex.Match(betUnder[i + 1].Value, "(?<=n\">)[\\w\\W]*").Value;
                            tRes = tRes + $"unchanged,{lBet},,,," + Environment.NewLine;
                            res = res + $"unchanged,{lBet},,,,";
                        }
                    }
                    //File.AppendAllText("bet.txt", tRes + Environment.NewLine);
                }
                if (tBets.Count == 0)
                {
                    res = res + "Null,";
                }
                return res;
            }
            string GetCorrectScore(string t)
            {
                var tBets = Regex.Matches(t, "(?<=<tr class=\"odds-type\">)[\\w\\W]*?(?=<tr class=\"average\">)");
                res = res + "COREECT SCORE,";
                foreach (var tb in tBets.Cast<Match>().Select(f => f.Value))
                {
                    var OverUnderType = Regex.Match(tb, "(?<=<td colspan=\"3\">)[\\w\\W]*?(?=<)").Value;
                    res = res + OverUnderType + ",";
                    string tRes = OverUnderType + Environment.NewLine;
                    bookMakers = Regex.Matches(tb, "(?<=<span class=\"t\">)[\\w\\W]*?(?=<)");
                    var betOver = Regex.Matches(tb, "(?<=td class=\"k1)[\\w\\W]*?(?=</div></td>)");
                    WopenDate = null; Wdelta = null;

                    for (i = 0; i < bookMakers.Count; i++)
                    {
                        res = res + $"{bookMakers[i]},";
                        tRes = tRes + $"{bookMakers[i]},";
                        string WfinDate = Regex.Match(betOver[i].Value, "(?<=<table cellspacing=\"0\"><tr><td>)[\\w\\W]*?(?=<)").Value;
                        if (WfinDate.Length > 0)
                        {
                            WopenDate = Regex.Match(betOver[i].Value, "(?<=Opening odds</td></tr><tr><td>)[\\w\\W]*?(?=<)").Value;
                            MatchCollection wBets = Regex.Matches(betOver[i].Value, "(?<=<td class=\"bold\">)[\\w\\W]*?(?=<)"); //0й - fin, 1й - open
                            Wdelta = Regex.Matches(betOver[i].Value, "(?<=><td class=\"diff-)[\\w\\W]*?(?=</td)")[0].Value.Split('>')[1];

                            res = res + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta},";
                            tRes = tRes + $"{WopenDate},{wBets[1].Value},{WfinDate},{wBets[0].Value},{Wdelta}," + Environment.NewLine;
                        }
                        else // ставка не менялась или отменена!
                        {
                            var wBet = Regex.Match(betOver[i].Value, "(?<=n\">)[\\w\\W]*").Value;

                            if (betOver[i].Value.Contains("deactivated"))
                            {
                                tRes = tRes + $"deactivated,{wBet},,,," + Environment.NewLine;
                                res = res + $"deactivated,{wBet},,,,";
                                continue; //Больше не надо, и так всё ясно
                            }
                            else
                            {
                                tRes = tRes + $"unchanged,{wBet},,,," + Environment.NewLine;
                                res = res + $"unchanged,{wBet},,,,";
                            }

                        }
                    }
                    //File.AppendAllText("bet.txt", tRes + Environment.NewLine);
                }
                if (tBets.Count == 0)
                {
                    res = res + "Null,";
                }
                return res;
            }
            return res;
        }

        public static void Excel(List<string> allStats)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\ReadyExcel"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ReadyExcel");
            string path = Directory.GetCurrentDirectory() + $"\\ReadyExcel\\{DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss")}_parse.xlsx";
            int i = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excelpackage = new ExcelPackage(new FileInfo(path));
            ExcelWorksheet worksheet = excelpackage.Workbook.Worksheets.Add("лист1");
            //worksheet.Cells.Style.Border.Top.Style = worksheet.Cells.Style.Border.Left.Style = worksheet.Cells.Style.Border.Right.Style = worksheet.Cells.Style.Border.Bottom.Style = excelborderStyle.thi;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //tournament,date,surface,round,winner,loser,wrank,lrank,w1,l1,w2,l2,w3,l3,w4,l4,w5,l5,wt1,lt1,wt2,lt2,wt3,lt3,wt4,lt4,wt5,lt5,wsets,lsets,bettype,bookmaker,wopendate,wbetopen,wfindate,wbetfin,wdelta,lopendate,lbet,lfindate,lbet,ldelta...et cetera...
            worksheet.Cells[1, 1].Value = "tournament,date,surface,round,winner,loser,wrank,lrank,w1,l1,w2,l2,w3,l3,w4,l4,w5,l5,wt1,lt1,wt2,lt2,wt3,lt3,wt4,lt4,wt5,lt5,wsets,lsets,bettype,bookmaker,wopendate,wbetopen,wfindate,wbetfin,wdelta,lopendate,lbet,lfindate,lbet,ldelta...et cetera...";
            for (i = 0; i < allStats.Count; i++)
                worksheet.Cells[i + 2, 1].Value = allStats[i];
            excelpackage.Save();
        }
    }
}
