using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;


namespace LunchCrawler.Common
{
    public static class Helpers
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

        public static HtmlDocument GetHtmlDocForUrl(string url)
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
    }
}
