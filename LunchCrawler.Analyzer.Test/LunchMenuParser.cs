using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Collections.Generic;

using HtmlAgilityPack;

using Lunch.Common.Enums;


namespace LunchCrawler.Analyzer.Test
{
    public class LunchMenuParser
    {
        public void ParseLunchMenu(string url)
        {
            Console.WriteLine("-> {0}\n", url);
            var htmlDoc = GetHtmlDocForUrl(url);

            var detectedFeatures = htmlDoc.DocumentNode.DescendantNodes()
                                                       .Where(node => !ShouldSkipNode(node))
                                                       .Select(LunchMenuDetection.DetectFeature)
                                                       .Where(feature => feature.Type != LunchFeatureType.Unknown)
                                                       .ToList();
            PrintDetectedFeatures(detectedFeatures);
        }

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

        private static HtmlDocument GetHtmlDocForUrl(string url)
        {
            var doc = new HtmlDocument();
            const int buffsize = 1024;

            try
            {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        var headerEncoding = TryGetEncoding(response.ContentEncoding) ?? TryGetEncoding(response.CharacterSet) ?? Encoding.UTF8;

                        var buf = new byte[buffsize];
                        var ms = new MemoryStream();
                        int count;

                        while ((count = response.GetResponseStream().Read(buf, 0, buffsize)) != 0)
                            ms.Write(buf, 0, count);

                        var bytes = ms.GetBuffer();
                        var docEncoding = doc.DetectEncodingHtml(headerEncoding.GetString(bytes));
                        var convertedBytes = Encoding.Convert(docEncoding ?? headerEncoding, Encoding.Unicode, bytes);
                        var convertedData = Encoding.Unicode.GetString(convertedBytes);

                        doc.LoadHtml(convertedData);
                    }
            }
            catch (Exception ex)
            {
                // jotain
            }

            return doc;
        }


        private static bool ShouldSkipNode(HtmlNode node)
        {
            // only nodes without child-nodes, comments, scripts or root DOCUMENT-type
            return node.HasChildNodes ||
                   node.NodeType == HtmlNodeType.Comment ||
                   node.NodeType == HtmlNodeType.Document ||
                   (node.ParentNode != null && node.ParentNode.Name.ToLower().Contains("script"));
        }


        private static void PrintDetectedFeatures(IEnumerable<LunchFeature> detectedFeatures)
        {
            Console.OutputEncoding = Encoding.Default;
            foreach (var feature in detectedFeatures)
            {
                if (feature.Type == LunchFeatureType.Weekday)
                {
                    Console.WriteLine();
                }

                var inputdata = Encoding.Unicode.GetBytes(HttpUtility.HtmlDecode(feature.InnerText.Trim()));
                var consoledata = Console.OutputEncoding.GetString(Encoding.Convert(Encoding.Unicode, Console.OutputEncoding, inputdata));

                Console.WriteLine("{0} ({1}, {2}): {3}",
                                  feature.Type,
                                  feature.Line,
                                  feature.LinePosition,
                                  consoledata);
            }
        }
    }
}
