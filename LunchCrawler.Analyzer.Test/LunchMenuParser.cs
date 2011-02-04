using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;


namespace LunchCrawler.Analyzer.Test
{
    public class LunchMenuParser
    {
        public void ParseLunchMenu(string url)
        {
            Console.WriteLine("-> {0}\n", url);
            var htmlDoc = Utils.GetHtmlDocumentForUrl(url);

            var detectedFeatures = htmlDoc.DocumentNode.DescendantNodes()
                                                       .Where(node => !ShouldSkipNode(node))
                                                       .Select(LunchMenuDetection.DetectFeature)
                                                       .Where(feature => feature.Type != LunchMenuFeatureType.Unknown)
                                                       .ToList();
            PrintDetectedFeatures(detectedFeatures);
        }

        private static bool ShouldSkipNode(HtmlNode node)
        {
            // only nodes without child-nodes, comments, scripts or root DOCUMENT-type
            return node.HasChildNodes ||
                   node.NodeType == HtmlNodeType.Comment ||
                   node.NodeType == HtmlNodeType.Document ||
                   (node.ParentNode != null && node.ParentNode.Name.ToLower().Contains("script"));
        }


        private static void PrintDetectedFeatures(IEnumerable<LunchMenuFeature> detectedFeatures)
        {
            Console.OutputEncoding = Encoding.Default;
            foreach (var feature in detectedFeatures)
            {
                if (feature.Type == LunchMenuFeatureType.Weekday)
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
