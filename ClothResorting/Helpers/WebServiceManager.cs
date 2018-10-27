using ClothResorting.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Helpers;

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

            try
            {
                //发送请求
                var request = WebRequest.Create(url);
                var data = Encoding.UTF8.GetBytes(stringifiedJsonData);

                request.Method = method;
                request.ContentType = "application/json;charset=UTF-8";
                request.Timeout = 800;
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + accessToken);

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
            }
            catch(Exception e)
            {
                throw new Exception();
            }

            return result;
        }

        public static string SendQueryRequest(string url, string accessToken)
        {
            var result = string.Empty;

            try
            {
                //发送请求
                var request = WebRequest.Create(url);

                request.Method = "GET";
                request.ContentType = "application/plain";
                request.Timeout = 800;
                request.Headers.Add("Authorization", "Bearer " + accessToken);

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
                throw new Exception();
            }

            return result;
        }
    }
}