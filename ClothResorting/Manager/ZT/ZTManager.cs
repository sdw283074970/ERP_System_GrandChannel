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
        // 中台出库请求，在Ready和Release按钮按下后触发
        public ResponseBody SendShippedOrderRequest(FBAShipOrder order)
        {
            var url = "http://hzero-gateway.hzero-dev.nearbyexpress.com/hitf/v2/rest/invoke?namespace=HZERO&serverCode=HORD&interfaceCode=homs-order.shipment.wmsUpdate";
            var token = GetAccessToken();

            var body = new RequestBody
            {
                Payload = new { ShipmentId = order.ShipOrderNumber, SystemSource= "CHINO", ShipmentStatusCode = order.Status == "Ready" ? "PICKED" : "SHIPPED", TrackingNumber = "NA", ShippingFee = "NA"}
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body), "POST", token);

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
            var url = "http://hzero-gateway.hzero-dev.nearbyexpress.com/hitf/v1/rest/invoke?namespace=HZERO&serverCode=HORD&interfaceCode=homs-order.rma.returnStatus";
            var token = GetAccessToken();

            var lines = new List<Line>();

            foreach(var d in order.FBAOrderDetails)
            {
                lines.Add(new Line { 
                    ReturnSkuId = d.ShipmentId,
                    RmaReturnQuantity = d.ActualQuantity
                });
            }

            var body = new RequestBody
            {
                Payload = new { Lines = lines, ReturnStatus = 1, RmaCode = order.Container, SystemSource = "CHINO" }
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body), "POST", token);

            var responseBody = new ResponseBody();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ResponseBody>(responseString);
            }

            return responseBody;
        }

        public static string SendHttpRequest(string url, string stringifiedJsonData, string method, string accessToken)
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

        public string GetAccessToken()
        {
            var url = "https://hzero-gateway.hzero-dev.nearbyexpress.com/oauth/oauth/token";

            var result = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { grant_type = "client_credentials", client_id = "wms", client_secret = "DE715D4979BBE0A778BB9A23267DCE51" }));

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
    }

    public class RequestBody
    {
        public dynamic PathVariables { get; set; }

        public dynamic Payload { get; set; }

        public dynamic ContentType { get; set; }

        public RequestBody()
        {
            PathVariables = new { TenantId = 0 };

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