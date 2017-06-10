using HtmlRenderer.ExperimentalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HtmlRenderer.ExperimentalApp.Controllers
{
    public class KeywordsController : ApiController
    {
        public IEnumerable<Keyword> GetAllKeywords()
        {
            return Keyword.Keywords;
        }

        public IHttpActionResult GetKeyword(int id)
        {
            if ((id < 0) || (id >= Keyword.Keywords.Length))
                return this.NotFound();

            return this.Ok(Keyword.Keywords[id]);
        }
    
        public IHttpActionResult PutKeyword(int id, Keyword keyword)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(ModelState);

            if ((id < 0) || (id >= Keyword.Keywords.Length))
                return this.NotFound();

            Keyword original = Keyword.Keywords[id];
            if (original.Text != keyword?.Text)
                return this.BadRequest("Sorry, seems something wrong. Couldn't determine record to update.");

            original.Urls = keyword.Urls;

            Keyword.SaveKeywords(@"C:\DEV_ATL\GitHub\HTML-Renderer\HTML-Renderer\Source\Testing\HtmlRenderer.ExperimentalApp\Data\Keywords.txt", Keyword.Keywords);

            return this.Ok(original);
        }
    }
}
