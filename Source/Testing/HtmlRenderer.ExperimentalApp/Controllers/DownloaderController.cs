using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HtmlRenderer.ExperimentalApp.Controllers
{
    public class DownloaderController : ApiController
    {
        [HttpPost]
        public IHttpActionResult PostDownload([FromBody]DownloadRequest request)
        {
            WebClient client = new WebClient();
            byte[] data = client.DownloadData(request.Url);
            //string html = System.Text.Encoding.UTF8.GetString(data);
            //data = System.Text.Encoding.UTF8.GetBytes(html);
            string base64 = Convert.ToBase64String(data);
            return this.Ok(base64);
        }
    
        public class DownloadRequest
        {
            public string Url { get; set; }
        }
    }
}
