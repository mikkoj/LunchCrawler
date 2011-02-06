using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Text;
using System.Security.Cryptography;

using HtmlAgilityPack;


namespace LunchCrawler.Common
{
    public static class Utils
    {
        private static Encoding TryGetEncoding(string encoding)
        {
            try
            {
                return Encoding.GetEncoding(encoding);
            }
            catch
            {
                return null;
            }
        }

        public static LunchMenuDocument GetLunchMenuDocumentForUrl(string url)
        {
            var document = new LunchMenuDocument();
            var htmlDoc = new HtmlDocument();

            const int buffsize = 1024;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var headerEncoding = TryGetEncoding(response.ContentEncoding) ?? TryGetEncoding(response.CharacterSet) ?? Encoding.UTF8;

                    var buf = new byte[buffsize];
                    var ms = new MemoryStream();
                    int count;

                    while ((count = response.GetResponseStream().Read(buf, 0, buffsize)) != 0)
                        ms.Write(buf, 0, count);

                    var bytes = ms.GetBuffer();
                   
                    var docEncoding = htmlDoc.DetectEncodingHtml(headerEncoding.GetString(bytes));
                    var convertedBytes = Encoding.Convert(docEncoding ?? headerEncoding, Encoding.Unicode, bytes);
                    var convertedData = Encoding.Unicode.GetString(convertedBytes);

                    htmlDoc.LoadHtml(convertedData);
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // handle any parse errors
            }
            
            if (htmlDoc.DocumentNode != null)
            {
                document.HtmlDocument = htmlDoc;

                // let's also compute a hash for the document
                document.Hash = ComputeHashForDocument(htmlDoc);
            }

            return document;
        }


        private static string ComputeHashForDocument(HtmlDocument htmlDoc)
        {
            var cleanDoc = new HtmlDocument();
            cleanDoc.LoadHtml(htmlDoc.DocumentNode.InnerHtml);

            var nodesToBeRemoved = cleanDoc.DocumentNode
                                           .DescendantNodes()
                                           .Where(ShouldSkipNode)
                                           .ToList();

            nodesToBeRemoved.ForEach(node => cleanDoc.DocumentNode.RemoveChild(node, true));

            var cleanHtml = cleanDoc.DocumentNode.InnerHtml.Trim();
            var cleanBytes = Encoding.Unicode.GetBytes(cleanHtml);
            var hasher = new SHA256Managed();
            return BitConverter.ToString(hasher.ComputeHash(cleanBytes));
        }


        public static string HtmlDecode(string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        public static bool ShouldSkipNode(HtmlNode node)
        {
            var excludes = new[] { "script", "style" };
            // only nodes without child-nodes, comments, scripts or root DOCUMENT-type
            return node.HasChildNodes ||
                   node.NodeType == HtmlNodeType.Comment ||
                   node.NodeType == HtmlNodeType.Document ||
                   (node.ParentNode != null && Array.Exists(excludes, exclude => node.ParentNode.Name.ToLower().Contains(exclude)));
        }

        public static string CleanContentForConsole(string content)
        {
            var inputdata = Encoding.Unicode.GetBytes(HtmlDecode(content));
            var consoleString = Console.OutputEncoding.GetString(Encoding.Convert(Encoding.Unicode, Console.OutputEncoding, inputdata));
            return consoleString.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }


        public static string GetBaseUrl(string url)
        {
            var uri = new Uri(url);
            var splitHostName = uri.Host.Split('.');
            if (splitHostName.Length >= 2)
            {
                var baseUrl = string.Format("{0}.{1}{2}",
                                            splitHostName[splitHostName.Length - 2],
                                            splitHostName[splitHostName.Length - 1],
                                            uri.LocalPath);
                return baseUrl;
            }
            
            return url;
        }
    }
}
