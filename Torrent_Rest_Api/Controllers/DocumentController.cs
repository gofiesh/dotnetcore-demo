
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Torrent_WebApi.Controllers
{
    [Route("doc")]
    [Produces("application/json")]
    public class DocumentController : Controller
    {
        // GET: doc/<docName>
        [HttpGet("{docName}")]
        virtual public IActionResult Get(string docName)
        {
            const string splitter = "<<<SPLITTER>>>";

            string content = System.IO.File.ReadAllText("Documents/" + docName + ".txt");

            content = "<p>" + content
                        .Replace("\r", "")
                        .Replace("\n===", "</p><h3>")
                        .Replace("===\n", "</h3><p>")
                        .Replace("\n==", "</p><h2>")
                        .Replace("==\n", "</h2><p>")
                        .Replace("\n=", "</p>" + splitter + "<h1>")
                        .Replace("=\n", "</h1><p>")
                        .Replace("\n\n", "</p><p>")
                        + "</p>";

            string[] raw = content.Split(splitter);

            List<object> contents = new List<object>();
            foreach (string r in raw)
                if (r.Contains("<h1>"))
                    contents.Add(new
                    {
                        // TODO: Use regex to extract the chapter title...
                        chapter = r.Substring(r.IndexOf("<h1>") + 4, r.IndexOf("</h1>") - 4),
                        text = r
                    });

            return new ObjectResult(new { pages = contents.ToArray() });
        }
    }
}