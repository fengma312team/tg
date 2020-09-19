using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// 爬数据逻辑
    /// </summary>
    public class Reptile
    {
        /// <summary>
        /// 网站地址
        /// </summary>
        private string baseurl = "";
        /// <summary>
        /// rest客户端
        /// </summary>
        private RestClient restclient;
        private HtmlWeb htmlWeb = new HtmlWeb();
        IList<RestResponseCookie> cookies;
        /// <summary>
        /// 初始化
        /// </summary>
        public Reptile(string baseurl = "https://w3.tg9000.net/")
        {
            this.baseurl = baseurl;
            this.restclient = new RestClient(baseurl);
        }

        /// <summary>
        /// 获取验证码图片  /ac_repic.php  注：获取随机值时，可能域名有变化，会有302跳转动作
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, byte[]> GetImage()
        {
            var baseurl = new RestRequest($"/", Method.POST);
            var baseresponse = restclient.Execute(baseurl);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(baseresponse.Content);
            HtmlNode navNode = htmlDoc.GetElementbyId("pic_rdcode");
            string randcode = navNode.Attributes["value"].Value;

            var request = new RestRequest($"ac_repic.php", Method.POST);
            request.AddParameter("url", "ac_repic.php");
            request.AddParameter("step", "Getpic");
            request.AddParameter("randcode", randcode);
            var response = restclient.Execute(request);
            var base64 = response.Content;
            byte[] images = Convert.FromBase64String(base64);
            return new KeyValuePair<string, byte[]>(randcode, images);
        }

        /// <summary>
        /// 获取验证码 ,注：此方法基本无用，要想提高此方法可用性，干扰线颜色相同，以此来去掉干扰线，再做ocr运行，基本上就可用了。
        /// </summary>
        /// <param name="b">验证码图片</param>
        /// <param name="b">验证码保存图片</param>
        /// <returns></returns>
        public string GetVerification(byte[] b, string savepath)
        {
            MemoryStream ms1 = new MemoryStream(b);
            Bitmap bm = (Bitmap)Image.FromStream(ms1);
            ms1.Close();
            if (File.Exists(savepath))
            {
                File.Delete(savepath);
            }
            bm.Save(savepath);
            var Ocr = new IronOcr.AutoOcr();
            var Result = Ocr.Read(bm);
            return Result.Text;
        }

        /// <summary>
        /// 登录  /ac_session.php
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="passwd">密码</param>
        /// <param name="verification">验证码</param>
        /// <param name="randcode">随机验证值</param>
        /// <returns></returns>
        public string Login(string name, string passwd, string verification, string randcode)
        {
            var request = new RestRequest($"ac_session.php", Method.POST);
            request.AddParameter("account", name);
            request.AddParameter("pwd", passwd);
            request.AddParameter("patternCode", verification);
            request.AddParameter("randcode", randcode);
            var response = restclient.Execute(request);
            cookies = response.Cookies;
            if (response.StatusCode == HttpStatusCode.OK && response.Content.Contains("1:登入成功::0:"))
            {
                return response.Content.Substring("1:登入成功::0:".Length, response.Content.Length - "1:登入成功::0:".Length);
            }
            return "";
        }

        /// <summary>
        /// 获取所有赛事 /targetList.php
        /// </summary>
        /// <returns></returns>
        public List<Match> GetAllHead()
        {
            List<Match> matches = new List<Match>();
            var request = new RestRequest($"targetList.php", Method.POST);
            var response = restclient.Execute(request);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response.Content);
            List<HtmlNode> nodes = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class,'market_game')] ").ToList();
            foreach (var item in nodes)
            {
                string onclick = item.GetAttributeValue("onclick", "");
                Match match = GetMatch(onclick);
                if (match != null && !matches.Exists(P => P.gameid == match.gameid))
                {
                    matches.Add(match);
                }
                foreach (HtmlNode item1 in item.ChildNodes)
                {
                    string onclick1 = item1.GetAttributeValue("onclick", "");
                    Match match1 = GetMatch(onclick1);
                    if (match1 != null && !matches.Exists(P => P.gameid == match1.gameid))
                    {
                        matches.Add(match1);
                    }
                }
            }
            matches = matches.Distinct().ToList();
            return matches;
        }

        /// <summary>
        /// 获取赔率  /ac_targetList.php   MarketOrder  /targetList.php
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public List<Info> GetInfo(Match head)
        {          
            var request = new RestRequest($"ac_targetList.php", Method.POST);
            request.AddParameter("gameid", head.gameid);
            request.AddParameter("ga12", head.ga12);
            request.AddParameter("gametime", head.gametime);
            request.AddParameter("noCache", "");

            CookieContainer _cookieJar = new CookieContainer();
            RestResponseCookie cookie= cookies.FirstOrDefault();
            _cookieJar.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            restclient.CookieContainer = _cookieJar;            
            var response = restclient.Execute(request);           
            return  GetInfo(response.Content,head);           
        }

        /// <summary>
        /// 处理赛事字符串
        /// marketTab( '22031703.1', '20200917035202237', '2020-09-19 18:00', '爱沙甲', '卡勒威 v 潭美卡' )
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private Match GetMatch(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length < 20)
            {
                return null;
            }
            json = json.Substring("marketTab(".Length, json.Length - "marketTab(".Length - 1).TrimEnd(')').Replace("'", "");
            string[] att = json.Split(',');
            Match match = new Match();
            match.gameid = att[0].Trim();
            match.ga12 = att[1].Trim();
            match.gametime = att[2].Trim();
            match.alliance = att[3].Trim();
            match.team = att[4].Trim();
            return match;
        }

        /// <summary>
        /// 转换具体的比赛信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<Info> GetInfo(string html, Match head)
        {
            List<Info> infos = new List<Info>();
            if(string.IsNullOrEmpty(html))
            {
                return infos;
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            List<HtmlNode> nodes = htmlDoc.DocumentNode.SelectNodes("//button").ToList();
            foreach (HtmlNode item in nodes)
            {
                if(item.InnerText!="下单")
                {
                    continue;
                }
                string onclick = item.GetAttributeValue("onclick", "");
                onclick = onclick.Substring("MarketOrder(".Length, onclick.Length - "MarketOrder(".Length - 1).TrimEnd(')').Replace("'", "");
                string[] att = onclick.Split(',');
                Info info = new Info();
                info.match = head;
                info.types = att[3];
                info.score = att[4];
                info.odds =Convert.ToDecimal( att[5]);
                infos.Add(info);
            }
            return infos;
        }

    }
}
