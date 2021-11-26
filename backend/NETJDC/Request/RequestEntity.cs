using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Systems;

namespace NETJDC.Request
{
    public class ReqSliderCaptcha
    {
        public string Phone { get; set; }

        public List<SliderCaptchaData> point { get; set; } = new List<SliderCaptchaData>();

       
    }
    public class RequestWSKEY
    {
        public int qlkey { get; set; } = 0;
        public string wskey { get; set; } = "";
        public string remarks { get; set; }
    }
    public class RequestEntity
    {
        public string Phone { get; set; }
        public int qlkey { get; set; } = 0;
        public string QQ { get; set; } = "";
        public string Code { get; set; }
    }
    public class RequestDEL
    {
        public int qlkey { get; set; } = 0;

        public string qlid { get; set; }
    }
    public class Requestremarks
    {
        public int qlkey { get; set; } = 0;

        public string remarks { get; set; }

        public string qlid { get; set; }
    }
}
