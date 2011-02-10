using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Security.Cryptography;

using HtmlAgilityPack;

using NLog;


namespace LunchCrawler.Common
{
    public static partial class Utils
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

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


        public static bool IsLink(HtmlNode node)
        {
            return node.NodeType == HtmlNodeType.Element && 
                node.Name.Equals("a", StringComparison.InvariantCultureIgnoreCase) && 
                node.Attributes.Contains("href");
        }

        public static LunchMenuDocument GetLunchMenuDocumentForUrl(string url)
        {
            return GetLunchMenuDocumentForUrl(url, 10);
        }

        /// <summary>
        /// Attempts to fetch and load a HtmlDocument for a given URL.
        /// Also determines the MIME-type for the stream and computes a hash if needed.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static LunchMenuDocument GetLunchMenuDocumentForUrl(string url, int timeout)
        {
            var document = new LunchMenuDocument();
            var htmlDoc = new HtmlDocument();

            const int buffsize = 1024;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(GetUri(url));
                request.Timeout = timeout * 1000;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var headerEncoding = TryGetEncoding(response.ContentEncoding) ??
                                         TryGetEncoding(response.CharacterSet) ??
                                         Encoding.UTF8;

                    var buf = new byte[buffsize];
                    var ms = new MemoryStream();
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        return null;
                    }
                    var count = responseStream.Read(buf, 0, buffsize);

                    document.MimeType = MimeDetector.DetermineMIMEType(buf);
                    if (document.MimeType == "text/html")
                    {
                        do
                            ms.Write(buf, 0, count);
                        while ((count = responseStream.Read(buf, 0, buffsize)) != 0);

                        var bytes = ms.GetBuffer();

                        var docEncoding = htmlDoc.DetectEncodingHtml(headerEncoding.GetString(bytes));
                        var convertedBytes = Encoding.Convert(docEncoding ?? headerEncoding, Encoding.Unicode, bytes);
                        var convertedData = Encoding.Unicode.GetString(convertedBytes);

                        htmlDoc.LoadHtml(convertedData);
                    }
                    else
                    {
                        _logger.Info("Discarded invalid mimetype '{0}' for URL: {1}", document.MimeType, url);
                    }
                }
            }
            catch
            {
                return null;
            }

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // TODO: handle any parse errors
            }
            
            if (htmlDoc.DocumentNode != null)
            {
                document.HtmlDocument = htmlDoc;

                // let's also compute a hash for the document
                document.Hash = ComputeHashForDocument(htmlDoc, url);
            }

            return document;
        }


        /// <summary>
        /// Tries to download content for simple occasions, returns empty string on failure.
        /// </summary>
        public static string GetContentForURL(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(GetUri(url));
                }
            }
            catch
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Adds http:// for an Uri if needed.
        /// </summary>
        private static Uri GetUri(string url)
        {
            Uri resultUri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out resultUri))
            {
                return resultUri;
            }

            var resultUrl = url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ? url : string.Format("http://{0}", url);
            return new Uri(resultUrl);
        }


        /// <summary>
        /// Attempts to compute a hash for a HTML document.
        /// </summary>
        private static string ComputeHashForDocument(HtmlDocument htmlDoc, string url)
        {
            try
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
            catch (Exception exHash)
            {
                _logger.Error("Couldn't compute hash for URL: " + url, exHash);
                return null;
            }
        }


        public static string HtmlDecode(string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        /// <summary>
        /// Determines whether a node should be skipped.
        /// Checks for comments, child-nodes, scripts etc.
        /// </summary>
        public static bool ShouldSkipNode(HtmlNode node)
        {
            var excludes = new[] { "script", "style" };

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
            try
            {
                var uri = GetUri(url);
                return string.Format("{0}{1}", uri.Host, uri.LocalPath);
            } 
            catch (FormatException)
            {
                return url;
            }
        }
    }
}
