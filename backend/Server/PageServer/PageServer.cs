using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using IServer;
using IServer.IPageServer;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using PuppeteerSharp.Mobile;
using RestSharp;
using RestSharp.Extensions;
using Systems;

namespace Server.PageServer
{
    public class Jpage
    {
        public DateTime regdate { get; set; }

        public Page Page { get; set; }
    }
    public class PageServer : IPageServer
    {
        private MainConfig _mainConfig;
        private OpenCVServer.OpenCVServer cv;
        public PageServer(MainConfig mainConfig, OpenCVServer.OpenCVServer openCV)
        {
            _mainConfig = mainConfig;
            cv = openCV;
        }
        public static Dictionary<string, Page> pagelist = new Dictionary<string, Page>();
        static readonly object _locker = new object();
        public Page AddPage(string phone, Page page)
        {
            lock (_locker)
            {
                string MaxTab = _mainConfig.MaxTab;
                if (string.IsNullOrEmpty(MaxTab)) MaxTab = "4";
                if (!pagelist.ContainsKey(phone))
                {
                    if (pagelist.Count < int.Parse(MaxTab))
                    {

                        pagelist.Add(phone, page);
                        return page;
                    }

                }
                else
                    return pagelist[phone];


            }
            return null;
        }
        public void Delpage(string phone, Page page)
        {
            lock (_locker)
            {
                if (pagelist.ContainsKey(phone))
                {
                    pagelist.Remove(phone);
                }
            }
        }
        public Page GetPage(string Phone)
        {
            lock (_locker)
            {
                if (!pagelist.ContainsKey(Phone))
                {

                    return null;
                }
                else
                    return pagelist[Phone];
            }
        }
        public Page GetPage()
        {
            lock (_locker)
            {
                System.Threading.Thread.Sleep(500);
                if (pagelist.Count > 0)
                    return pagelist.First().Value;
                else
                    return null;
            }
        }
        public int GetPageCount()
        {
            lock (_locker)
            {
                System.Threading.Thread.Sleep(500);

                return pagelist.Count;

            }
        }
        public void info()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"                                                                   ");
            Console.WriteLine(@"                 ,--.                     ,---._                           ");
            Console.WriteLine(@"               ,--.'|                   .-- -.' \     ,---,      ,----..   ");
            Console.WriteLine(@"           ,--,:  : |       ,---.       |    |   :  .'  .' `\   /   /   \  ");
            Console.WriteLine(@"        ,`--.'`|  ' :      /__./|       :    ;   |,---.'     \ |   :     : ");
            Console.WriteLine(@"        |   :  :  | | ,---.;  ; |       :        ||   |  .`\  |.   |  ;. / ");
            Console.WriteLine(@"        :   |   \ | :/___/ \  | |       |    :   ::   : |  '  |.   ; /--`  ");
            Console.WriteLine(@"        |   : '  '; |\   ;  \ ' |       :         |   ' '  ;  :;   | ;     ");
            Console.WriteLine(@"        '   ' ;.    ; \   \  \: |       |    ;   |'   | ;  .  ||   : |     ");
            Console.WriteLine(@"        |   | | \   |  ;   \  ' .   ___ l         |   | :  |  '.   | '___  ");
            Console.WriteLine(@"        '   : |  ; .'   \   \   ' /    /\    J   :'   : | /  ; '   ; : .'| ");
            Console.WriteLine(@"        |   | '`--'      \   `  ;/  ../  `..-    ,|   | '` ,/  '   | '/  : ");
            Console.WriteLine(@"        '   : |           :   \ |\    \         ; ;   :  .'    |   :    /  ");
            Console.WriteLine(@"        ;   |.'            '---'' \    \       ,' |   ,.'      \   \ .'   ");
            Console.WriteLine(@"        '---'                      '-- - ....--'  '-- '         `---`     ");
            Console.WriteLine(@"");
            Console.WriteLine(@"");
            Console.WriteLine(@"                                                                     Version:" + _mainConfig.Version);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(@"                                                                     By Nolan  ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("序列号:" + _mainConfig.CPUID);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine("Running on Linux!");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Console.WriteLine("Running on macOS!");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.WriteLine("Running on Windows!");
            Console.WriteLine($"系统架构：{RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"系统名称：{RuntimeInformation.OSDescription}");
            Console.WriteLine($"进程架构：{RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"是否64位操作系统：{Environment.Is64BitOperatingSystem}");
            Console.WriteLine("-------------------------------------------------------------------------------------");
        }
        public async Task<bool> CCHECK()
        {
            var type = Enum.GetName(typeof(UpTypeEum), _mainConfig.UPTYPE);
            Console.WriteLine("上传模式：" + type);
            Console.WriteLine("回收时间：" + _mainConfig.Closetime + "分钟");
            Console.WriteLine("自动滑块：" + _mainConfig.AutoCaptchaCount);
            if (!string.IsNullOrEmpty(_mainConfig.PUSH_PLUS_TOKEN) && !string.IsNullOrEmpty(_mainConfig.PUSH_PLUS_USER)) Console.WriteLine("开启PUSH_PLU 上线推送服务");
            if (!string.IsNullOrEmpty(_mainConfig.Debug)) Console.WriteLine("Debug 模式");
            if (_mainConfig.Config.Count > 0 && !string.IsNullOrEmpty(_mainConfig.XDDToken))
            {
                Console.WriteLine("配置有误配置了XDD 有配置了青龙，请将青龙配置删除 如 Config:[]");
            }
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("浏览器自检");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            try
            {
                
                //  if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)&& RuntimeInformation.OSArchitecture.)
                var options = new LaunchOptions
                {
                    Args = new string[] { "--no-sandbox", "--disable-setuid-sandbox" },
                    //Headless = false,
                    DefaultViewport = new ViewPortOptions
                    {
                        Width = 375,
                        Height = 667,
                        IsMobile = true
                    }

                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64)
                {
                    Console.WriteLine("不支持arm！！！！！！！");
                }
                using (var browser = await Puppeteer.LaunchAsync(options))
                using (var page = await browser.NewPageAsync())
                {
                    //设置手机模式
                    DeviceDescriptor deviceOptions = Puppeteer.Devices.GetValueOrDefault(DeviceDescriptorName.IPhone7);
                    await page.EmulateAsync(deviceOptions);
                    //await page.SetUserAgentAsync("Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1");
                    string Url = "https://plogin.m.jd.com/login/login?appid=300&returnurl=https%3A%2F%2Fwq.jd.com%2Fpassport%2FLoginRedirect%3Fstate%3D2087738584%26returnurl%3Dhttps%253A%252F%252Fhome.m.jd.com%252FmyJd%252Fnewhome.action%253Fsceneval%253D2%2526ufc%253D%2526&source=wq_passport";
                    await page.GoToAsync(Url, 0, null);

                    await page.WaitForTimeoutAsync(2000);
                    await Waitopen(page);
                    string js = "document.body.outerText";
                    var pageouterText = await page.EvaluateExpressionAsync(js);
                    var pagetext = pageouterText.ToString();
                    pagetext = pagetext.Replace("\n", "");
                    pagetext = pagetext.Replace("\r", "");
                    Console.WriteLine("页面文字" + pagetext);
                    Console.WriteLine("关闭页面");
                    await page.CloseAsync();
                    await page.DisposeAsync();
                    await browser.CloseAsync();
                    await browser.DisposeAsync();
                    Console.WriteLine("浏览器关闭");
                    Console.WriteLine("浏览器自检成功");
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("打开浏览器失败");
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("-------------------------------------------------------------------------------------");
            return true;
        }

        public async Task<bool> BrowserInit()
        {
            var browserFetcher = new BrowserFetcher();
            var aa = await browserFetcher.DownloadAsync();
            try
            {
                var path = aa.ExecutablePath;
                Bash($"chmod 777 {path}");
            }
            catch (Exception e)
            {
                Console.WriteLine("执行 CHOMD 777 浏览器地址错位 可以忽略");
                // Console.WriteLine(e.ToString()) ;
            }
            return aa.Downloaded;
        }
        public void Bash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
        public async Task<ResultModel<object>> OpenJDTab(int qlkey, string Phone, bool UploadQL = true)
        {
            DateTime expdate = DateTime.Now;

            ResultModel<object> result = ResultModel<object>.Create(false, "");
            if (UploadQL)
            {
                var qlconfig = _mainConfig.GetConfig(qlkey);
                if (qlconfig == null)
                {
                    result.message = "未找到相应的服务器配置。请刷新页面后再试";
                    result.data = new { Status = 404 };
                    return result;
                }
            }

            Page page = GetPage();
            Browser browser = null;
            if (page == null)
            {

                var options = new LaunchOptions
                {
                    Args = new string[] { "--no-sandbox", "--disable-setuid-sandbox" },
                    //Headless = false,
                    DefaultViewport = new ViewPortOptions
                    {
                        Width = 375,
                        Height = 667,
                        IsMobile = true
                    }

                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64)
                {
                    Console.WriteLine("不支持arm！！！！！！！");
                }
               
                browser = await Puppeteer.LaunchAsync(options);
            }
            else
                browser = page.Browser;

            string MaxTab = _mainConfig.MaxTab;
            if (string.IsNullOrEmpty(MaxTab)) MaxTab = "4";
            var Tablist = await browser.PagesAsync();
            if (Tablist.Length > int.Parse(MaxTab) + 1)
            {
                result.message = MaxTab + "个网页资源已经用完。请稍候再试!";
                result.success = false;
                return result;
            }

            string Url = "https://plogin.m.jd.com/login/login?appid=300&returnurl=https%3A%2F%2Fwq.jd.com%2Fpassport%2FLoginRedirect%3Fstate%3D2087738584%26returnurl%3Dhttps%253A%252F%252Fhome.m.jd.com%252FmyJd%252Fnewhome.action%253Fsceneval%253D2%2526ufc%253D%2526&source=wq_passport";
            ///、 string Url = "https://bean.m.jd.com/bean/signIndex.action";
            var context = await browser.CreateIncognitoBrowserContextAsync();
            page = await context.NewPageAsync();
            ///屏蔽 WebDriver 检测
             //await page.EvaluateFunctionOnNewDocumentAsync("function(){Object.defineProperty(navigator, 'webdriver', {get: () => undefined})}");
            DeviceDescriptor deviceOptions = Puppeteer.Devices.GetValueOrDefault(DeviceDescriptorName.IPhone7);
            await page.EmulateAsync(deviceOptions);
            await page.GoToAsync(Url, 0, null);
            await page.WaitForTimeoutAsync(200);
            await Waitopen(page);
            await page.ClickAsync("input[type=checkbox]");
            var aa = await GetPhoneCode(Phone, page);
            await page.WaitForTimeoutAsync(210);
            await page.ClickAsync("button[report-eventid='MLoginRegister_SMSReceiveCode']", new PuppeteerSharp.Input.ClickOptions { ClickCount = 3 });

            ///傻逼等待代码
            await WaitSendSms(page);
            string js = "document.body.outerText";
            var pageouterText = await page.EvaluateExpressionAsync(js);
            var pagetext = pageouterText.ToString();
            DebugLOG("SendSms 发送验证码或安全滑块", pagetext);
            var ckcount = 0;
            var tabcount = GetTableCount();
            if (_mainConfig.UPTYPE == UpTypeEum.ql)
            {
                var data = await getCount(qlkey);
                ckcount = data.ckcount;
            }
            int closetime = int.Parse(_mainConfig.Closetime);
            System.Timers.Timer timer = new System.Timers.Timer(60000 * closetime);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(async (s, e) =>
            {
                Console.WriteLine("    手机：" + Phone + " tabe 自动回收 时间:" + DateTime.Now.ToString());
                Delpage(Phone, page);
                await page.CloseAsync();
                await page.DisposeAsync();
                var oldpage = GetPage();
                if (oldpage == null)
                {
                    await browser.CloseAsync();
                    await browser.DisposeAsync();
                }
                timer.Dispose();
            });
            timer.AutoReset = false;

            if (pagetext.Contains("拖动滑块填充拼图"))
            {
                Console.WriteLine("    手机：" + Phone + " tabe 创建 时间:" + DateTime.Now.ToString());
                timer.Start();
                Console.WriteLine(Phone + "安全验证");
                var baseimg = await GetIMG(page, 0);
                result.data = new { Status = 666, ckcount = ckcount, tabcount = tabcount, big = baseimg.big, small = baseimg.small };
                result.message = "出现安全验证,";
                return result;
            }
            if (pagetext.Contains("短信已经发送，请勿重复提交"))
            {
                await PageClose(Phone);
                result.data = new { Status = 505, pagetext = pagetext };
                result.message = "请刷新页面重新登陆。";
                return result;
            }
            if (pagetext.Contains("短信验证码发送次数已达上限"))
            {
                await PageClose(Phone);
                result.data = new { Status = 505, pagetext = pagetext };
                result.message = "对不起，短信验证码发送次数已达上限，请24小时后再试。";
                return result;
            }
            if (pagetext.Contains("您的账号存在风险"))
            {
                await PageClose(Phone);
                result.data = new { Status = 505, pagetext = pagetext };
                result.message = "您的" + Phone + "存在风险";
                return result;
            }
            if (pagetext.Contains("该手机号未注册，将为您直接注册。"))
            {
                await PageClose(Phone);

                result.data = new { Status = 501 };
                result.message = "该手机号未注册";
                return result;
            }
            Console.WriteLine("    手机：" + Phone + " tabe 创建 时间:" + DateTime.Now.ToString());

            timer.Start();
            if (pagetext.Contains("重新获取"))
            {
                result.success = true;
                Console.WriteLine(Phone + "获取验证码成功");
            }
            result.data = new { ckcount = ckcount, tabcount = tabcount };

            return result;
        }
        public async Task<bool> Waitopen(Page page)
        {
            try
            {
                await page.WaitForTimeoutAsync(500);
                string js = "document.body.outerText";
                var pageouterText = await page.EvaluateExpressionAsync(js);
                var pagetext = pageouterText.ToString();
                DebugLOG("Waitopen 等待打开网页", pagetext);
                if (pagetext.Contains("获取验证码"))
                {
                    return true;
                }
                else
                {

                    return await WaitSendSms(page);
                }
            }
            catch (Exception e)
            {
                return await Waitopen(page);
            }


        }
        /// <summary>
        /// 网络问题所以要写这种傻逼等待带代码
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<bool> WaitSendSms(Page page)
        {
            await page.WaitForTimeoutAsync(1000);
            string js = "document.body.outerText";
            var pageouterText = await page.EvaluateExpressionAsync(js);
            var pagetext = pageouterText.ToString();
            DebugLOG("WaitSendSms 等待是否发送验证码或安全滑块", pagetext);
            if (pagetext.Contains("安全验证") || pagetext.Contains("短信已经发送，请勿重复提交") || pagetext.Contains("您的账号存在风险") || pagetext.Contains("该手机号未注册，将为您直接注册。") || pagetext.Contains("重新获取"))
            {
                return true;
            }
            else
            {
                return await WaitSendSms(page);
            }
        }
        public async Task PageClose(string Phone)
        {
            var page = GetPage(Phone);
            if (page != null)
            {
                Delpage(Phone, page);
                var browser = page.Browser;
                await page.CloseAsync();
                await page.DisposeAsync();
                var oldpage = GetPage();
                if (oldpage == null)
                {
                    await browser.CloseAsync();
                    await browser.DisposeAsync();
                }
            }
        }

        public async Task ReSendSmSCode(string Phone)
        {
            var page = GetPage(Phone);
            if (page == null) throw new Exception("页面未找到,可能超时回收了.请刷新页面重新登录");
            await GetPhoneCode(Phone, page);
        }
        /// <summary>
        /// 因为网络出现的傻逼等待代码
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<bool> AwitVerifyCode(Page page)
        {
            try
            {
                await page.WaitForTimeoutAsync(1000);
                // 打开京东App，购物更轻松
                string js = "document.body.outerText";
                var pageouterText = await page.EvaluateExpressionAsync(js);
                var pagetext = pageouterText.ToString();
                DebugLOG("AwitVerifyCode 等待判断登陆", pagetext);
                //   
                if (pagetext.Contains("验证码输入错误") || pagetext.Contains("验证码错误多次") || pagetext.Contains("该手机号未注册") || pagetext.Contains("该手机号为运营商二次贩卖号") || pagetext.Contains("立即打开"))
                {
                    return true;
                }
                else
                {
                    return await AwitVerifyCode(page);
                }
            }
            catch (Exception e)
            {
                return await AwitVerifyCode(page);
            }

        }

        public async Task<ResultModel<object>> VerifyCode(int qlkey, string qq, string Phone, string Code)
        {

            ResultModel<object> result = ResultModel<object>.Create(false, "");
            if (_mainConfig.UPTYPE == UpTypeEum.ql)
            {
                var qlconfig = _mainConfig.GetConfig(qlkey);
                if (qlconfig == null)
                {
                    result.message = "未找到相应的服务器配置。请刷新页面后再试";
                    result.data = new { Status = 404 };
                    return result;
                }
            }


            Page page = GetPage(Phone);
            if (page == null)
            {
                result.message = "未找到当前号码的网页请稍候再试,或者网页超过" + _mainConfig.Closetime + "分钟已被回收";
                result.data = new { Status = 404 };
                return result;
            }
            await SetCode(Code, page);
            //await page.WaitForTimeoutAsync(400);
            Console.WriteLine("输入验证码" + Code);

            await page.ClickAsync("a[report-eventid='MLoginRegister_SMSLogin']");
            await AwitVerifyCode(page);

            string js = "document.body.outerText";
            var pageouterText = await page.EvaluateExpressionAsync(js);
            var pagetext = pageouterText.ToString();
            DebugLOG("VerifyCode 判断登陆", pagetext);
            if (pagetext.Contains("验证码输入错误"))
            {
                result.message = "验证码输入错误";
                return result;
            }
            if (pagetext.Contains("验证码错误多次"))
            {
                await PageClose(Phone);
                result.data = new { Status = 501 };
                result.message = "验证码错误多次,请过三分钟之后再来。";
                return result;
            }
            if (pagetext.Contains("该手机号未注册"))
            {
                await PageClose(Phone);

                result.data = new { Status = 501 };
                result.message = "该手机号未注册";
                return result;
            }
            if (pagetext.Contains("该手机号为运营商二次贩卖号"))
            {
                await PageClose(Phone);

                result.data = new { Status = 501 };
                result.message = "该手机号存在安全风险，请在JD自行登录看提示";
                return result;
            }
            if (pagetext.Contains("立即打开"))
            {
                var cookies = await page.GetCookiesAsync();
                var CKkey = cookies.FirstOrDefault(x => x.Name == "pt_key");
                var CKpin = cookies.FirstOrDefault(x => x.Name == "pt_pin");
                await PageClose(Phone);
                if (CKkey == null || CKpin == null)
                {
                    result.message = "获取Cookie失败，请重试";
                    result.data = new { Status = 404 };
                    return result;
                }

                var CCookie = CKkey.Name + "=" + CKkey.Value + ";" + CKpin.Name + "=" + CKpin.Value + ";";
                await PageClose(Phone);
                Console.WriteLine(Phone + "获取到ck");
                if (_mainConfig.UPTYPE == UpTypeEum.ql)
                {
                    result = await UploadQL(Phone, CCookie, CKpin.Value, qlkey);
                    return result;
                }
                if (_mainConfig.UPTYPE == UpTypeEum.xdd)
                {
                    result = await Uploadxdd(qq, CCookie);
                    return result;
                }
                int tabcount = GetTableCount();
                result.data = new { tabcount = tabcount };
                result.success = true;
                result.data = new { tabcount = tabcount, ck = CCookie };
                return result;
            }
            await PageClose(Phone);
            result.message = "登陆失败,请刷新页面";
            return result;


        }

        public async Task<ResultModel<object>> Uploadxdd(string qq, string ck)
        {
            //"code":200,"data":"null","message":"添加成功"
            ResultModel<object> result = ResultModel<object>.Create(false, "");

            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> dict = new Dictionary<string, string>
                     {
                         {"qq",qq},
                        {"token", _mainConfig.XDDToken},
                         {"ck", ck}
                    };

                var resultd = await client.PostAsync(_mainConfig.XDDurl, new FormUrlEncodedContent(dict));
                string resultContent = resultd.Content.ReadAsStringAsync().Result;


                JObject j = JObject.Parse(resultContent);
                int tabcount = GetTableCount();
                if (j["code"].ToString() == "200")
                {
                    result.success = true;
                    result.message = "添加xdd成功!";
                }
                else
                {
                    result.message = j["message"].ToString();
                }
                result.data = new { tabcount = tabcount };
                return result;
            }

        }
        public async Task<ResultModel<object>> UploadQL(string Phone, string ck, string ckpin, int qlkey)
        {
            ResultModel<object> result = ResultModel<object>.Create(false, "");
            var qlconfig = _mainConfig.GetConfig(qlkey);
            var Nickname = "";
            int MAXCount = qlconfig.QL_CAPACITY;
            Nickname = await GetNickname(ck);
            JArray data = await qlconfig.GetEnv();
            JToken env = null;
            var QLCount = await qlconfig.GetEnvsCount(); ;
            if (data != null)
            {
                env = data.FirstOrDefault(x => x["value"].ToString().Contains("pt_pin=" + ckpin + ";"));

            }
            string QLId = "";
            string timestamp = "";
            if (env == null)
            {
                if (QLCount >= MAXCount)
                {
                    result.message = "你来晚了，没有多余的位置了";
                    result.data = new { Status = 501 };
                    return result;
                }

                var addresult = await qlconfig.AddEnv(ck, "JD_COOKIE",Nickname);
                JObject addUser = (JObject)addresult.data[0];
                QLId = addUser["_id"].ToString();
                timestamp = addUser["timestamp"].ToString();

                await _mainConfig.pushPlusNotify(@" 服务器;" + qlconfig.QLkey + " " + qlconfig.QLName + "  <br>用户 " + Nickname + "   " + ckpin + " 已上线");
            }
            else
            {
                QLId = env["_id"].ToString();
                if (env["remarks"] != null)
                    Nickname = env["remarks"].ToString();


                var upresult = await qlconfig.UpdateEnv(ck, QLId, "JD_COOKIE", Nickname);
                timestamp = upresult.data["timestamp"].ToString();
                await _mainConfig.pushPlusNotify(@" 服务器;" + qlconfig.QLkey + " " + qlconfig.QLName + "  <br>用户 " + Nickname + "   " + ckpin + " 已更新 CK");
            }
            await qlconfig.Enable(QLId);
            var qin = await getCount(qlkey);
            await PageClose(Phone);
            result.success = true;
            result.data = new { qlid = QLId, nickname = Nickname, timestamp = timestamp, remarks = Nickname, qlkey = qlconfig.QLkey, ckcount = qin.ckcount, tabcount = qin.tabcount };
            return result;

        }
        public int GetTableCount()
        {
            string MaxTab = _mainConfig.MaxTab;
            var intabcount = GetPageCount();
            int tabcount = int.Parse(MaxTab) - intabcount;
            return tabcount;
        }
        public async Task<(int ckcount, int tabcount)> getCount(int qlkey)
        {
            var config = _mainConfig.GetConfig(qlkey);
            var qlcount = await config.GetEnvsCount();
            var ckcount = config.QL_CAPACITY - qlcount;
            string MaxTab = _mainConfig.MaxTab;
            var intabcount = GetPageCount();
            int tabcount = int.Parse(MaxTab) - intabcount;
            return (ckcount, tabcount);
        }
        private async Task Setphone(string phone, Page page)
        {
            await page.ClickAsync("input[report-eventid='MLoginRegister_SMSPhoneInput']", new PuppeteerSharp.Input.ClickOptions { ClickCount = 3 });

            await page.TypeAsync("input[report-eventid='MLoginRegister_SMSPhoneInput']", phone);
            await page.WaitForTimeoutAsync(200);
        }
        private async Task<bool> GetPhoneCode(string Phone, Page page)
        {
            page = AddPage(Phone, page);
            await Setphone(Phone, page);
            var CodeBtn = await page.XPathAsync("//button[@report-eventid='MLoginRegister_SMSReceiveCode']");
            var CodeProperties = await CodeBtn[0].GetPropertiesAsync();
            var CodeBtnClasses = CodeProperties["_prevClass"].ToString().Split(" ");
            Console.WriteLine(CodeProperties["_prevClass"].ToString());
            bool canSendCode = CodeBtnClasses.Contains("active");
            if (canSendCode)
            {
                return true;
            }
            else
            {
                await page.ReloadAsync();
                await page.WaitForTimeoutAsync(500);
                return await GetPhoneCode(Phone, page);
            }
        }
        private long GetTime()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);//ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
            return (long)ts.TotalSeconds;
        }
        private async Task<string> GetNickname(string cookie)
        {
            try
            {
                var url = @"https://me-api.jd.com/user_new/info/GetJDUserInfoUnion?orgFlag=JD_PinGou_New&callSource=mainorder&channel=4&isHomewhite=0&sceneval=2&_=" + GetTime() + "&sceneval=2&g_login_type=1&g_ty=ls";
                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Add("Cookie", cookie);
                    client.DefaultRequestHeaders.Add("Referer", "https://home.m.jd.com/myJd/newhome.action");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Host", "me-api.jd.com");
                    var result = await client.GetAsync(url);
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("获取nickname");
                    JObject j = JObject.Parse(resultContent);
                    // data?.userInfo.baseInfo.nickname
                    return j["data"]["userInfo"]["baseInfo"]["nickname"].ToString();
                }
            }
            catch (Exception e)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {

                        client.DefaultRequestHeaders.Add("Cookie", cookie);
                        client.DefaultRequestHeaders.Add("Referer", "https://home.m.jd.com/myJd/newhome.action");
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");
                        client.DefaultRequestHeaders.Add("Host", "me-api.jd.com");
                        var result = await client.GetAsync("https://wq.jd.com/user_new/info/GetJDUserInfoUnion?orgFlag=JD_PinGou_New&callSource=mainorder");
                        string resultContent = result.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("获取nickname");
                        JObject j = JObject.Parse(resultContent);
                        // data?.userInfo.baseInfo.nickname
                        return j["data"]["userInfo"]["baseInfo"]["nickname"].ToString();


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString()); ;
                    return "未知";
                }
              
            }


        }

        private static async Task SetCode(string Code, Page page)
        {
            await page.ClickAsync("#authcode", new PuppeteerSharp.Input.ClickOptions { ClickCount = 3 });
            await page.TypeAsync("#authcode", Code);

        }
        /// <summary>
        /// 因为网络出现的傻逼等待代码
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<bool> AwitAutoCaptcha(Page page)
        {
            try
            {
                await page.WaitForTimeoutAsync(500);
                // 打开京东App，购物更轻松
                string js = "document.body.outerText";
                var pageouterText = await page.EvaluateExpressionAsync(js);
                var pagetext = pageouterText.ToString();
                DebugLOG("AwitAutoCaptcha 等待判断滑块完成", pagetext);
                if (pagetext.Contains("重新获取") || pagetext.Contains("验证失败，请重新验证") || pagetext.Contains("拖动滑块填充拼图") || pagetext.Contains("短信验证码发送次数已达上限") || pagetext.Contains("该手机号未注册，将为您直接注册。") || pagetext.Contains("您的账号存在风险"))
                {
                    return true;
                }
                else
                {
                    return await AwitAutoCaptcha(page);
                }
            }
            catch (Exception e)
            {
                return await AwitAutoCaptcha(page);
            }

        }
        public void DebugLOG(string Fnc, string ms)
        {
            if (string.IsNullOrEmpty(_mainConfig.Debug)) return;
            Console.WriteLine("执行阶段  :" + Fnc);
            Console.WriteLine(ms);
            Console.WriteLine("-------------------------------------------------------------------");
        }
        public async Task<(string big, string small)> GetIMG(Page page, int time = 500)
        {
            await page.WaitForTimeoutAsync(time);
            var cpc_img = await page.QuerySelectorAsync("#cpc_img");
            var cpc_imgheader = await cpc_img.GetPropertyAsync("src");
            var cpc_imgsrc = await cpc_imgheader.JsonValueAsync();
            var small_img = await page.QuerySelectorAsync("#small_img");
            var small_imgheader = await small_img.GetPropertyAsync("src");
            var small_imgsrc = await small_imgheader.JsonValueAsync();
            string pattern = @"data:image.*base64,(.*)";
            Match m = Regex.Match(cpc_imgsrc.ToString(), pattern);
            var cpc_imgbase64 = m.Groups[1].ToString();
            Match m2 = Regex.Match(small_imgsrc.ToString(), pattern);
            var small_imgbase64 = m2.Groups[1].ToString();
            return (cpc_imgbase64, small_imgbase64);
        }

        public async Task<ResultModel<object>> VerifyCaptcha(string Phone, List<SliderCaptchaData> Pointlist)
        {
            ResultModel<object> result = ResultModel<object>.Create(false, "");
            var page = GetPage(Phone);
            if (page == null)
            {
                result.message = "未找到当前号码的网页请稍候再试,或者网页超过" + _mainConfig.Closetime + "分钟已被回收,请刷新重试";
                result.data = new { Status = 404 };
                return result;
            }
            if (Pointlist.Count == 0)
            {
                var imgdata = await GetIMG(page);
                Console.WriteLine("验证失败");
                result.data = new { Status = 666, big = imgdata.big, small = imgdata.small };
                return result;
            }
            var baseimg = await GetIMG(page);
            byte[] cpc_imgBytes = Convert.FromBase64String(baseimg.big);
            byte[] small_imgbaseBytes = Convert.FromBase64String(baseimg.small);
            Stream cpcstream = new MemoryStream(cpc_imgBytes);
            Stream smallstream = new MemoryStream(small_imgbaseBytes);
            var cpcmap = new Bitmap(new Bitmap(cpcstream));
            var smallmap = new Bitmap(new Bitmap(smallstream));
            var Rsct = cv.getOffsetX(cpcmap, smallmap);
            cpcmap.Dispose();
            cpcstream.Close();
            smallstream.Close();
            smallmap.Dispose();
            var slider = await page.WaitForXPathAsync("//div[@class='sp_msg']/img");
            var box = await slider.BoundingBoxAsync();
            await page.Mouse.MoveAsync(box.X, box.Y);
            await page.Mouse.DownAsync();
            Random r = new Random(Guid.NewGuid().GetHashCode());
            var Width = r.Next(0, 2);
            int MaxX = Rsct.X + Rsct.Width + Width;
            var Steps = r.Next(1, 10);
            decimal y = 0;
            Console.WriteLine("停顿" + Steps);
            foreach (var item in Pointlist)
            {
                y = box.Y + item.Y;
                await page.Mouse.MoveAsync(box.X + item.X, box.Y + item.Y, new PuppeteerSharp.Input.MoveOptions { Steps = Steps });
            }
            await page.Mouse.MoveAsync(MaxX, y, new PuppeteerSharp.Input.MoveOptions { Steps = Steps });
            await page.Mouse.UpAsync();
            await AwitAutoCaptcha(page);
            string js = "document.body.outerText";
            var pageouterText = await page.EvaluateExpressionAsync(js);
            var html = pageouterText.ToString();
            DebugLOG("AutoCaptcha 自动滑块完成判断是否获取", html);

            if (html.Contains("重新获取"))
            {
                Console.WriteLine("验证成功");
                result.success = true;
            }
            else
            {
                if (html.Contains("短信验证码发送次数已达上限"))
                {
                    await PageClose(Phone);
                    result.data = new { Status = 505 };
                    result.message = "对不起，" + Phone + "短信验证码发送次数已达上限，请24小时后再试。";
                    return result;
                }
                if (html.Contains("该手机号未注册，将为您直接注册。"))
                {
                    await PageClose(Phone);

                    result.data = new { Status = 505 };
                    result.message = "该手机号" + Phone + "未注册";
                    return result;
                }
                if (html.Contains("您的账号存在风险"))
                {
                    await PageClose(Phone);

                    result.data = new { Status = 505 };
                    result.message = "该手机号" + Phone + "存在风险";
                    return result;
                }
                if (html.Contains("验证失败，请重新验证"))
                {
                    await page.WaitForTimeoutAsync(1000);
                }
                var imgdata2 = await GetIMG(page, 0);
                Console.WriteLine("验证失败");
                result.data = new { Status = 666, big = imgdata2.big, small = imgdata2.small };
            }
            return result;
        }
        public async Task<ResultModel<object>> AutoCaptcha(string Phone)
        {
            ResultModel<object> result = ResultModel<object>.Create(false, "");
            Console.WriteLine(Phone + "滑块安全验证");

            var page = GetPage(Phone);
            var cpc_img = await page.QuerySelectorAsync("#cpc_img");
            var cpc_imgheader = await cpc_img.GetPropertyAsync("src");
            var cpc_imgsrc = await cpc_imgheader.JsonValueAsync();
            var small_img = await page.QuerySelectorAsync("#small_img");
            var small_imgheader = await small_img.GetPropertyAsync("src");
            var small_imgsrc = await small_imgheader.JsonValueAsync();
            string pattern = @"data:image.*base64,(.*)";
            Match m = Regex.Match(cpc_imgsrc.ToString(), pattern);
            var cpc_imgbase64 = m.Groups[1].ToString();
            Match m2 = Regex.Match(small_imgsrc.ToString(), pattern);
            var small_imgbase64 = m2.Groups[1].ToString();
            byte[] cpc_imgBytes = Convert.FromBase64String(cpc_imgbase64);
            byte[] small_imgbaseBytes = Convert.FromBase64String(small_imgbase64);
            Stream cpcstream = new MemoryStream(cpc_imgBytes);
            Stream smallstream = new MemoryStream(small_imgbaseBytes);
            var cpcmap = new Bitmap(new Bitmap(cpcstream));
            var smallmap = new Bitmap(new Bitmap(smallstream));
            var Rsct = cv.getOffsetX(cpcmap, smallmap);
            Console.WriteLine(Phone + "获取到坐标 X" + Rsct.X);
            var bw = cpcmap.Width;
            cpcmap.Dispose();
            cpcstream.Close();
            smallstream.Close();
            smallmap.Dispose();
            //var list = cv.GetPoints(Rsct, bw);
            //Console.WriteLine("轨迹数量;"+list.Count);
            PageMove.PageMove move = new PageMove.PageMove();
            await move.Move(page, Rsct);
            await AwitAutoCaptcha(page);
            string js = "document.body.outerText";
            var pageouterText = await page.EvaluateExpressionAsync(js);
            var html = pageouterText.ToString();
            DebugLOG("AutoCaptcha 自动滑块完成判断是否获取", html);
            if (html.Contains("重新获取"))
            {
                Console.WriteLine("验证成功");
                result.success = true;
            }
            else
            {
                if (html.Contains("短信验证码发送次数已达上限"))
                {
                    await PageClose(Phone);
                    result.data = new { Status = 505 };
                    result.message = "对不起，" + Phone + "短信验证码发送次数已达上限，请24小时后再试。";
                    return result;
                }
                if (html.Contains("该手机号未注册，将为您直接注册。"))
                {
                    await PageClose(Phone);

                    result.data = new { Status = 505 };
                    result.message = "该手机号" + Phone + "未注册";
                    return result;
                }
                if (html.Contains("您的账号存在风险"))
                {
                    await PageClose(Phone);

                    result.data = new { Status = 505 };
                    result.message = "该手机号" + Phone + "存在风险";
                    return result;
                }
                var imgdata2 = await GetIMG(page);
                Console.WriteLine("验证失败");
                result.data = new { Status = 666, big = imgdata2.big, small = imgdata2.small };
            }
            return result;
        }

        public async Task<string> WSkeyGetToken(string WSKEY)
        {
            var sv = "";
            var st = "";
            var uuid = "";
            var sign = "";
            var clientVersion = "";
            using (HttpClient client2 = new HttpClient())
            {

                // client.Headers
                var resultd = client2.GetAsync("https://hellodns.coding.net/p/sign/d/jsign/git/raw/master/sign").Result;
                string resultContent = resultd.Content.ReadAsStringAsync().Result;
                JObject j = JObject.Parse(resultContent);
                sv = j["sv"] != null ? j["sv"].ToString() : "";
                st = j["st"] != null ? j["st"].ToString() : "";
                uuid = j["uuid"] != null ? j["uuid"].ToString() : "";
                sign = j["sign"] != null ? j["sign"].ToString() : "";
                clientVersion = j["clientVersion"] != null ? j["clientVersion"].ToString() : "";
            }
            if (sv == "" || st == "" || uuid == "" || sign == "") throw new Exception("获取JD签名接口失效。请联系Nolan");
            Requset requset = new Requset();
            var Uri = new Uri($"https://api.m.jd.com/client.action?functionId=genToken&clientVersion=10.1.2&client=android&uuid=" + uuid + "&sign=" + sign + "&st=" + st + "&sv=" + sv);
            var headers = new Dictionary<string, string>();
            var pararms = new List<RestSharp.Parameter>();
            var pin = Regex.Match(WSKEY, "pin=(.*?);").Value.UrlEncode();
            var key = Regex.Match(WSKEY, "wskey=(.*?);").Value;

            if (string.IsNullOrWhiteSpace(pin) && string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("wskey格式不正确");
            }

           // WSKEY = $"pin={pin};{key}";
            headers.Add("cookie", $"{WSKEY}");

            pararms.Add(new RestSharp.Parameter("application/x-www-form-urlencoded", "body=%7B%22action%22%3A%22to%22%2C%22to%22%3A%22https%253A%252F%252Fplogin.m.jd.com%252Fcgi-bin%252Fm%252Fthirdapp_auth_page%253Ftoken%253DAAEAIEijIw6wxF2s3bNKF0bmGsI8xfw6hkQT6Ui2QVP7z1Xg%2526client_type%253Dandroid%2526appid%253D879%2526appup_type%253D1%22%7D&", ParameterType.RequestBody));

            var Content = await requset.HttpRequset(Uri, RestSharp.Method.POST, headers, pararms);
            if(Content == null|| (int)Content["code"] != 0) throw new Exception("WSKEY检查状态接口出错, 请稍后尝试");

            var client = new RestClient("https://un.m.jd.com/cgi-bin/app/appjmp?tokenKey=" + (string)Content["tokenKey"] + "&to=https://plogin.m.jd.com/cgi-bin/m/thirdapp_auth_page?token=AAEAIEijIw6wxF2s3bNKF0bmGsI8xfw6hkQT6Ui2QVP7z1Xg&client_type=android&appid=879&appup_type=1");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            var CKkey = response.Cookies.FirstOrDefault(x => x.Name == "pt_key");
            var CKpin = response.Cookies.FirstOrDefault(x => x.Name == "pt_pin");
            if (CKkey == null || CKpin == null)
            {
                throw new Exception("通过WSKYE获取Cookie失败");
            }
           if (CKkey.Value.Contains("fake")) throw new Exception("wsck状态失效或填写错误");

            //    var Cookies = response.Cookies;
            var CCookie = CKkey.Name + "=" + CKkey.Value + ";" + CKpin.Name + "=" + CKpin.Value + ";";
            return CCookie;
        }
    }
}
