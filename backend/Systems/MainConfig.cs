using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace Systems
{
    public enum UpTypeEum
    {
        none=0,
        ql=1,
        xdd=2,
    }
    public class MainConfig
    {
       
        public string Version { get; set; } = "1.5";
        /// <summary>
        /// 最大标签
        /// </summary>
        public string MaxTab { get; set; }
        /// <summary>
        /// 回收时间
        /// </summary>
        public string Closetime { get; set; }
        /// <summary>
        /// 网站标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 网站公共
        /// </summary>
        public string Announcement { get; set; }
        /// <summary>
        /// XDDurl
        /// </summary>
        public string XDDurl { get; set; }
        /// <summary>
        /// XDDTOKEN
        /// </summary>
        public string XDDToken { get; set; }
        /// <summary>
        /// Debug模式 默认关闭
        /// </summary>
        public string Debug { get; set; }

     
        private string _CPUID = "";
        /// <summary>
        /// KeyWord对应的KeyField
        /// </summary>
        public string CPUID
        {
            get
            {
                if (!String.IsNullOrEmpty(_CPUID))
                    return _CPUID;

                _CPUID = this.GetCpuid();

                return _CPUID;
            }
            set { _CPUID= this.GetCpuid(); }
        }
        /// <summary>
        /// 自动滑块次数
        /// </summary>
        public string AutoCaptchaCount { get; set; }
        /// <summary>
        /// 青龙配置
        /// </summary>
        public List< Qlconfig> Config = new List<Qlconfig>();

        public UpTypeEum UPTYPE { get; set; } = UpTypeEum.none;

        public string PUSH_PLUS_TOKEN { set; get; }

        public string PUSH_PLUS_USER { set; get; }
        public Qlconfig GetConfig(int Id)
        {
            var item = this.Config.FirstOrDefault(x => x.QLkey == Id);
            if (item == null) throw new Exception("未找相应到配置青龙");
            return item;

        }
        public async Task<ResultModel<object>> pushPlusNotify(string desp = " ")
        {
            string author = "<br><br>本通知 By：https://github.com/NolanHzy/nvjdcdocker";
            desp += author;
            //desp = desp.Split(/[\n\r] / g, '<br>');
            //"code":200,"data":"null","message":"添加成功"
            ResultModel<object> result = ResultModel<object>.Create(false, "");
            if (string.IsNullOrEmpty(this.PUSH_PLUS_TOKEN) || string.IsNullOrEmpty(this.PUSH_PLUS_USER)) return result;

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>
                     {
                         {"token",this.PUSH_PLUS_TOKEN},
                         {"topic", this.PUSH_PLUS_USER},
                         {"content", desp},
                         {"title", "Nolan 运行通知。"},

                    };

                    var resultd = await client.PostAsync("https://www.pushplus.plus/send", new FormUrlEncodedContent(dict));
                    string resultContent = resultd.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                    if (j["code"].ToString() == "200")
                    {
                        result.success = true;
                    }
                    else
                    {
                        result.message = j["message"].ToString();
                    }
                    return result;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("消息推送失败");
                return result;
            }

        }
        private string GetCpuid()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetWindoscpuid();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetLinxCPUID();
            return "";
        }
        private  string ExecuteCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash", // 使用 bash
                                            // FileName = "/bin/sh", // 使用 sh
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            var message = process.StandardOutput.ReadToEnd();

            return message;
        }
        private string GetLinxCPUID()
        {
            var ss= ExecuteCommand("cat /sys/class/dmi/id/product_uuid");
            return ss;
          

        }
        private string GetWindoscpuid()
        {
            try
            {
                string systemId = null;
                using (ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_ComputerSystemProduct"))
                {
                    foreach (var item in mos.Get())
                    {
                        systemId = item["UUID"].ToString();
                    }
                }
                return systemId;
            }
            catch
            {
                return "";
            }
        }
      
        public MainConfig()
        {
             string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Config.json");
            if (System.IO.File.Exists(ConfigPath))
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(ConfigPath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {

                        JObject o = (JObject)JToken.ReadFrom(reader);
                        Type t = this.GetType();
                        if ( o.Property("Config") != null)
                        {
                            var json = o["Config"].ToString();
                            this.Config= JsonConvert.DeserializeObject<List<Qlconfig>>(json);
                        }
                        foreach (PropertyInfo pi in t.GetProperties())
                        {
                            if(pi.Name!= "Config"&& pi.Name != "Version"&&pi.Name!= "CPUID" && pi.Name != "Sponsor" && o.Property(pi.Name) != null)
                            {
                                pi.SetValue(this, o[pi.Name].ToString(), null);
                            }
                           
                        }
                        if (this.Config.Count > 0) this.UPTYPE = UpTypeEum.ql;
                        if (!string.IsNullOrEmpty(this.XDDToken)) this.UPTYPE = UpTypeEum.xdd;
                        if (string.IsNullOrEmpty(this.AutoCaptchaCount)) this.AutoCaptchaCount = "5";
                        if (string.IsNullOrEmpty(this.Closetime)) this.Closetime = "3";
                        // this.CPUID = this.GetCpuid();
                    }
                }
            }
        }
    }
    public class Qlitem
    {
        public int QLkey { set; get; }
        public string QLName { set; get; }
        public int QL_CAPACITY { set; get; }
    }
    public class Qlconfig
    {
        public int QLkey { set; get; }
        public string QLName { set; get; }
        public string QLurl { set; get; }

        public string QL_CLIENTID { set; get; }

        public string QL_SECRET { set; get; }

        public int QL_CAPACITY { set; get; }

        public string QRurl { set; get; }

    
        public static long GetTime()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);//ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
            return (long)ts.TotalSeconds;
        }
        public async Task<string> GetToken()
        {
            try
            {
                string value = "" ;
                   var url = QLurl + "/open/auth/token?client_id=" + QL_CLIENTID + "&client_secret=" + QL_SECRET;
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        var result = await client.GetAsync(url);
                        string resultContent = result.Content.ReadAsStringAsync().Result;
                        JObject j = JObject.Parse(resultContent);
                        value = j["data"]["token"].ToString();
                       
                    }

                return value.ToString();
            }
            catch (Exception E)
            {
                throw new Exception("配置有误获取token失败");
            }

        }


        public async Task<JArray> GetEnv(String searchValue = "")
        {
            var token = await GetToken();
            var Url = QLurl + "/open/envs?searchValue=" + searchValue + "&t=" + GetTime();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var result = await client.GetAsync(Url);
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                    if (j["code"].ToString() != "200")
                    {

                        return null;
                    }
                    JArray array = (JArray)j["data"];
                    return array;
                }
            }
            catch (Exception e)
            {
                return null;
            }


        }
        public async Task<int> GetEnvsCount(String searchValue = "")
        {
            JArray data = await GetEnv(searchValue);
            if (data == null) return 0;

            var aa = data.Where(x => x["name"] != null && x["name"].ToString().ToUpper() == "JD_COOKIE").Count();
            return aa;
           
        }
        public async Task<int> GetEnvsWSKEYCount(String searchValue = "")
        {
            JArray data = await GetEnv("JD_WSCK");
            if (data == null) return 0;

            var aa = data.Where(x => x["name"] != null && x["name"].ToString().ToUpper() == "JD_WSCK").Count();
            return aa;

        }


        public async Task<ResultModel<JArray>> AddEnv(string Ck,string type, string remarks = "JDCOOKIE")
        {
            ResultModel<JArray> result = ResultModel<JArray>.Create(false, "");
            var token = await GetToken();
            var Url = QLurl + "/open/envs?t=" + GetTime();
            JArray jArray = new JArray();
            JObject jsonObject = new JObject();
            jsonObject.Add("value", Ck);
            jsonObject.Add("name", type );
            jsonObject.Add("remarks", remarks);
            jArray.Add(jsonObject);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpContent httpContent = new StringContent(jArray.ToString());
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var req = await client.PostAsync(Url, httpContent);
                    string resultContent = req.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                    if (j["code"].ToString() != "200")
                    {
                        result.message = "ck上传青龙失败";
                        return result;
                    }
                    result.success = true;
                    result.data = (JArray)j["data"];
                    result.message = "ck上传成功";
                }
            }
            catch (Exception e)
            {
                result.message = "ck上传青龙失败";
            }

            return result;
        }
        public async Task<JObject> GetEnvbyid(string Eid)
        {
            JArray data = await GetEnv();
            var env = data.FirstOrDefault(x => x["_id"].ToString() == Eid);
            return (JObject)env;
        }
        public async Task<ResultModel<JObject>> UpdateEnv(string Ck, string Eid,string type, string remarks = "JDCOOKIE")
        {
            ResultModel<JObject> result = ResultModel<JObject>.Create(false, "");
            var token = await GetToken();
            var Url = QLurl + "/open/envs?t=" + GetTime();
            var pocoObject = new
            {
                value = Ck,
                name = type,
                remarks = remarks,
                _id = Eid
            };
            string json = JsonConvert.SerializeObject(pocoObject);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");


                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var res = await client.PutAsync(Url, data);
                    string resultContent = res.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                    if (j["code"].ToString() != "200")
                    {
                        result.message = "更新账户错误，请重试";
                        return result;
                    }
                    result.success = true;
                    result.message = "ck更新/上传备注成功";
                    result.data = (JObject)j["data"];
                }
            }
            catch (Exception e)
            {
                result.message = e.Message;
            }

            return result;
        }
        public async Task Enable(string Eid)
        { 
           
            ResultModel<JObject> result = ResultModel<JObject>.Create(false, "");
            var token = await GetToken();
            var Url = this.QLurl + "/open/envs/enable?t=" + GetTime();


            var pocoObject = new string[]
            {
             Eid
            };
            string json = JsonConvert.SerializeObject(pocoObject);
            try
            {
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Content = data,
                        Method = HttpMethod.Put,
                        RequestUri = new Uri(Url)
                    };
                    //HttpContent httpContent = new StringContent(jArray.ToString());
                    //httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var req = await client.SendAsync(request);
                    string resultContent = req.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public async Task<ResultModel<JObject>> DelEnv(string Eid)
        {
            ResultModel<JObject> result = ResultModel<JObject>.Create(false, "");
            var token = await GetToken();
            var Url = this.QLurl + "/open/envs?t=" + GetTime();


            var pocoObject = new string[]
            {
             Eid
            };
            string json = JsonConvert.SerializeObject(pocoObject);
            try
            {
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Content = data,
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(Url)
                    };
                    //HttpContent httpContent = new StringContent(jArray.ToString());
                    //httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var req = await client.SendAsync(request);
                    string resultContent = req.Content.ReadAsStringAsync().Result;
                    JObject j = JObject.Parse(resultContent);
                    if (j["code"].ToString() != "200")
                    {
                        result.message = "删除账户错误，请重试";
                        return result;
                    }
                    result.success = true;
                    result.message = "账户已移除";
                    // result.data = (JObject)j["data"];
                }
            }
            catch (Exception e)
            {
                result.message = e.Message;
            }

            return result;
        }
    }
}
