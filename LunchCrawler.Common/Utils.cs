using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
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


        public static bool IsLink(HtmlNode node)
        {
            return node.NodeType == HtmlNodeType.Element && node.Name == "a" && node.Attributes["href"] != null;
        }


        public static LunchMenuDocument GetLunchMenuDocumentForUrl(string url)
        {
            var document = new LunchMenuDocument();
            var htmlDoc = new HtmlDocument();

            const int buffsize = 1024;

            // TODO: timeout & joillain response codeilla 1x retry (?)
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(GetUri(url));
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
                // TODO: handle any parse errors
            }
            
            if (htmlDoc.DocumentNode != null)
            {
                document.HtmlDocument = htmlDoc;

                // let's also compute a hash for the document
                document.Hash = ComputeHashForDocument(htmlDoc);
            }

            return document;
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

            var resultUrl = url.StartsWith("http") ? url : string.Format("http://{0}", url);
            return new Uri(resultUrl);
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
            try
            {
                var uri = new Uri(url);
                return string.Format("{0}{1}", uri.Host, uri.LocalPath);
            } 
            catch (FormatException)
            {
                return url;
            }
        }


        /// <summary>
        /// Creates a deep-clone of any given object.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">Object to be deep-cloned.</param>
        /// <returns>A deep-clone of the object.</returns>
        public static T DeepClone<T>(this T obj) where T : new()
        {
            if (!typeof(T).IsSerializable && !(typeof(T) is ISerializable))
            {
                throw new InvalidOperationException("A serializable Type is required");
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
