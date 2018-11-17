using ClothResorting.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace ClothResorting.Helpers
{
    public class WebServiceManager
    {
        //private ApplicationDbContext _context;

        //public WebServiceManager()
        //{
        //    _context = new ApplicationDbContext();
        //}

        public static string SendCreateRequest(string url, string stringifiedJsonData, string method, string accessToken)
        {
            var result = string.Empty;
            
            //发送请求
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.UTF8.GetBytes(stringifiedJsonData);

            request.Method = method;
            request.ContentType = "application/json";
            //request.Timeout = 20000;
            request.KeepAlive = false;
            request.ServicePoint.Expect100Continue = false;
            request.ContentLength = data.Length;
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Accept = "application/json";
            request.UserAgent = "APIExplorer";
            ServicePointManager.DefaultConnectionLimit = 1000;      //提高每秒默认请求数量

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            var response = request.GetResponse();

            var stream = response.GetResponseStream();

            //获取响应
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static string SendQueryRequest(string url, string accessToken)
        {
            var result = string.Empty;

            try
            {
                //发送请求
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";
                request.ContentType = "application/plain";
                //request.Timeout = 800;
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Accept = "application/json";
                request.UserAgent = "APIExplorer";

                var response = request.GetResponse();

                var stream = response.GetResponseStream();

                //获取响应
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }
    }
}