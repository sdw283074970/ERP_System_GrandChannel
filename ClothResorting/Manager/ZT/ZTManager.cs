﻿using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ClothResorting.Manager.ZT
{
    public class ZTManager
    {
        private Logger _logger;
        private ApplicationDbContext _context;

        public ZTManager(ApplicationDbContext context)
        {
            _context = context;
            _logger = new Logger(_context);
        }

        // 中台出库请求，在Ready和Release按钮按下后触发
        public ResponseBody UpdateOunboundOrderRequest(FBAShipOrder order)
        {
            var url = "https://hzero-gateway.hzero-uat.nearbyexpress.com/hitf/v1/rest/invoke?namespace=HZERO&serverCode=HORD&interfaceCode=homs-order.shipment.wmsUpdate";
            var token = GetAccessToken();
            var statusCode = order.Status == "Ready" ? "PICKED" : (order.Status == "Released" ? "SHIPPED" : "UNPICKED");
            var body = new RequestBody
            {
                // 测试
                Payload = JsonConvert.SerializeObject(new { ShipmentCode = order.ShipOrderNumber, SystemSource = "CHINO", ShipmentStatusCode = statusCode, TrackingNumber = 0, ShippingFee = 0, DeliveryDate = order.ReleasedDate.ToString("yyyy-MM-dd hh:mm:ss") }, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                })

                // 生产
                //Payload = JsonConvert.SerializeObject(new { ShipmentCode = order.ShipOrderNumber, SystemSource = "CHINO", ShipmentStatusCode = order.Status == statusCode, TrackingNumber = 0, ShippingFee = 0 }, new JsonSerializerSettings
                //{
                //    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                //})
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            }), "POST", token);

            var responseBody = new ResponseBody();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ResponseBody>(responseString);
            }

            return responseBody;
        }

        // 中台入库请求，在入库单complete按钮按下后触发
        public ResponseBody SendInboundCompleteRequest(FBAMasterOrder order)
        {
            var url = "https://hzero-gateway.hzero-uat.nearbyexpress.com/hitf/v1/rest/invoke?namespace=HZERO&serverCode=HORD&interfaceCode=homs-order.rma.returnStatus";
            var token = GetAccessToken();

            var lines = new List<Line>();

            foreach (var d in order.FBAOrderDetails)
            {
                lines.Add(new Line {
                    ReturnSkuId = d.ShipmentId,
                    RmaReturnQuantity = d.ActualQuantity
                });
            }

            var body = new RequestBody
            {
                Payload = JsonConvert.SerializeObject(new { Lines = lines, ReturnStatus = 1, RmaCode = order.Container, SystemSource = "CHINO" }, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                })
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            }), "POST", token);

            var responseBody = new ResponseBody();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ResponseBody>(responseString);
            }

            return responseBody;
        }

        public string SendHttpRequest(string url, string stringifiedJsonData, string method, string accessToken)
        {
            var result = string.Empty;

            // 发送请求
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
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 1000;      //提高每秒默认请求数量
            var headers = request.Headers.ToString();
            _logger.AddRequestLog(url, headers, stringifiedJsonData, null);

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            try
            {
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
                _logger.AddRequestLog(url, headers, stringifiedJsonData, e.Message);
                throw new Exception(e.Message);
            }

            return result;
        }

        public string GetAccessToken()
        {
            var url = "https://hzero-gateway.hzero-uat.nearbyexpress.com/oauth/oauth/token";

            var result = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            var outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);

            outgoingQueryString.Add("grant_type", "client_credentials");
            outgoingQueryString.Add("client_id", "wms");
            outgoingQueryString.Add("client_secret", "DE715D4979BBE0A778BB9A23267DCE51");

            var postBytes = Encoding.UTF8.GetBytes(outgoingQueryString.ToString());

            //var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            //request.ContentType = "multipart/form-data; boundary=" + boundary;

            //var body = JsonConvert.SerializeObject(new { grant_type = "client_credentials", client_id = "wms", client_secret = "DE715D4979BBE0A778BB9A23267DCE51" });
            //var data = Encoding.UTF8.GetBytes(body);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(postBytes, 0, postBytes.Length);
                reqStream.Close();
            }

            var response = request.GetResponse();

            var stream = response.GetResponseStream();

            // 获取响应
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            result = JsonConvert.DeserializeObject<TokenResponseBody>(result).Access_token;

            return result;
        }
    }

    public class RequestBody
    {
        public dynamic PathVariableMap { get; set; }

        public dynamic Payload { get; set; }

        public dynamic ContentType { get; set; }

        public RequestBody()
        {
            PathVariableMap = new { TenantId = 0 };

            ContentType = new { MimeType = "application/json", Charset = "UTF-8" };
        }
    }

    public class Line
    {
        public int RmaReturnQuantity { get; set; }

        public string ReturnSkuId { get; set; }
    }

    public class Payload
    {
        public string Msg { get; set; }

        public string Code { get; set; }

        public string Content { get; set; }

        public bool Error { get; set; }
    }

    public class ResponseBody
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public Payload Payload { get; set; }
    }

    public class TokenResponseBody
    {
        public string Access_token { get; set; }

        public string Token_type { get; set; }

        public int Expires_in { get; set; }

        public string Scope { get; set; }
    }
}