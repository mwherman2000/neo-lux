using LunarParser;
using LunarParser.JSON;
using System;
using System.Net;

namespace Neo.Lux.Utils
{
    public enum RequestType
    {
        GET,
        POST
    }

    public static class RequestUtils
    {
        public static DataNode Request(RequestType kind, string url, DataNode data = null)
        {
            string contents;

            try
            {
                switch (kind)
                {
                    case RequestType.GET:
                        {
                            contents = GetWebRequest(url);
                            //Console.WriteLine($"Request.GET: contents: {contents}");
                            break;
                        }
                    case RequestType.POST:
                        {
                            var paramData = data != null ? JSONWriter.WriteToString(data) : "{}";
                            contents = PostWebRequest(url, paramData);
                            //Console.WriteLine($"Request.POST: contents: {contents}");
                            break;
                        }
                    default:
                        {
                            Console.WriteLine($"Request: unknown kind: {kind}");
                            return null;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request: exception: {e.ToString()}");
                return null;
            }

            if (string.IsNullOrEmpty(contents))
            {
                Console.WriteLine($"Request: contents null or empty");
                return null;
            }

            //File.WriteAllText("response.json", contents);

            var root = JSONReader.ReadFromString(contents);
            if (root == null)
            {
                Console.WriteLine($"Request: root == null");
            }
            return root;
        }

        public static string GetWebRequest(string url)
        {
            using (var  client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
            {
                return client.DownloadString(url);
            }
        }

        public static string PostWebRequest(string url, string paramData)
        {
            using (var client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
            {
                return client.UploadString(url, paramData);
            }
        }
    }
}
