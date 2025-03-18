using System.Net;

namespace nuxt_shop.Models
{
    public class Result
    {
        public bool success { get; set; } = true;
        public string messages { get; set; } 
        public object data { get; set; }
        public HttpStatusCode? code { get; set; } = HttpStatusCode.OK;
    }
}
