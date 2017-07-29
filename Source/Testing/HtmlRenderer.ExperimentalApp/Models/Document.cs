using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlRenderer.ExperimentalApp.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public string Dom { get; set; }

        public DocumentStatus Status { get; set; }
    }
}