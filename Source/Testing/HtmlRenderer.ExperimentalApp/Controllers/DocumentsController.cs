using HtmlRenderer.ExperimentalApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HtmlRenderer.ExperimentalApp.Controllers
{
    public class DocumentsController : ApiController
    {
        private const string BasePath = @"C:\DEV_ATL\GitHub\HTML-Renderer\HTML-Renderer\Source\Testing\HtmlRenderer.ExperimentalApp\Data\Files\";

        public static string GenerateTestCode()
        {
            var files = AllDocuments.Select(doc =>
            {
                string path = Path.Combine(Path.GetDirectoryName(doc.Path), Path.GetFileNameWithoutExtension(doc.Path));
                string[] parts = path.Substring(BasePath.Length).Split('\\');
                return new { Keyword = parts[0], File = parts[1], Path = path };
            });

            string method = @"
        [TestMethod]
        [TestCategory('{0}')]
        public void Test_{2}()
        {{
            DomTestUtils.TestDom('{0}', '{1}');
        }}
        ";

            method = method.Replace('\'', '\"');

            StringBuilder sb = new StringBuilder();
            foreach(var group in files.GroupBy(file => file.Keyword))
            {
                if (group.Key != "amazon")
                    continue;

                sb.AppendLine("        #region " + group.Key);
                sb.AppendLine();

                foreach(var file in group)
                {
                    string name = GetMethodName(file.Path);
                    sb.AppendFormat(method, group.Key, file.File, name);
                }

                sb.AppendLine();
                sb.AppendLine("        #endregion");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetMethodName(string path)
        {
            string url;
            using (StreamReader reader = new StreamReader(path + ".txt"))
            {
                reader.ReadLine();
                url = reader.ReadLine().Substring(5);
            }

            if (!(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                url = "http://" + url;

            int idx = url.IndexOf("://");
            url = url.Substring(idx + 3);

            StringBuilder sb = new StringBuilder();
            foreach(char ch in url)
            {
                if (((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z')) || ((ch >= '0') && (ch <= '9')))
                    sb.Append(ch);
                else
                    sb.Append('_');
            }
            return sb.ToString();
        }

        private static Document[] GetDocuments()
        {
            List<Document> docs = new List<Document>();
            foreach (string file in Directory.GetFiles(BasePath, "*.html", SearchOption.AllDirectories))
            {
                string dom = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".dom";

                Document doc = new Document();
                doc.Id = docs.Count;
                doc.Status = File.Exists(dom) ? DocumentStatus.Done : DocumentStatus.New;
                doc.Path = file;
                doc.Url = GetUrl(file);
                docs.Add(doc);
            }
            return docs.ToArray();
        }

        private static readonly Document[] AllDocuments = GetDocuments();

        private static readonly ConcurrentQueue<Document> NewDocuments = new ConcurrentQueue<Document>(AllDocuments.Where(doc => doc.Status == DocumentStatus.New));

        private static string GetUrl(string path)
        {
            path = path.Substring(90);
            path = String.Join("/", path.Split('\\').Select(p => WebUtility.UrlEncode(p)));
            return path;
        }

        public Document GetNextDocument()
        {
            Document doc = null;
            NewDocuments.TryDequeue(out doc);
            return doc;
        }

        public IHttpActionResult PutDocument(int id, Document doc)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(ModelState);

            if ((id < 0) || (id >= AllDocuments.Length))
                return this.NotFound();


            Document original = AllDocuments[id];
            if ((original.Path != doc?.Path) || (original.Url != doc?.Url))
                return this.BadRequest("Sorry, seems something wrong. Couldn't determine record to update.");

            original.Status = DocumentStatus.Done;
            original.Dom = doc.Dom;

            string path = Path.Combine(Path.GetDirectoryName(original.Path), Path.GetFileNameWithoutExtension(original.Path));
            File.WriteAllText(path + ".dom", original.Dom);

            return this.Ok(original);
        }
    }
}
