using ClothResorting.Models.FBAModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ClothResorting.Manager.NetSuit
{
    public class NetSuitManager
    {
        public ReturnData SendTransOrderShippedRequest(FBAShipOrder order, IEnumerable<FBAPickDetailCarton> pickedCtnList)
        {
            var url = "https://2298410.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=251&deploy=1";

            var lines = new List<TransLine>();

            foreach(var p in pickedCtnList)
            {
                if (lines.Where(x => x.ItemNum == p.FBACartonLocation.ShipmentId).Count() != 0)
                    continue;

                lines.Add(new TransLine { 
                    Quantity = 1,
                    ItemNum = p.FBACartonLocation.ShipmentId
                });
            }

            var body = new TransOrderRequestBody {
                Data = new ShippedData {
                    TransOrderNo = order.ShipOrderNumber,
                    TransOrderId = order.Id.ToString(),
                    Memo = "",
                    TranDate = order.ReleasedDate.ToString("yyyy-MM-dd"),
                    Lines = lines
                }
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public ReturnData SendTransOrderInboundRequest(FBAMasterOrder order)
        {
            var url = "https://2298410.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=248&deploy=1";

            var lines = new List<TransLine>();

            foreach (var d in order.FBAOrderDetails)
            {
                if (lines.Where(x => x.ItemNum == d.ShipmentId).Count() != 0)
                    continue;

                lines.Add(new TransLine
                {
                    Quantity = d.ActualQuantity,
                    ItemNum = d.ShipmentId
                });
            }

            var body = new TransOrderRequestBody
            {
                Data = new ShippedData
                {
                    TransOrderNo = order.Container,
                    TransOrderId = order.Id.ToString(),
                    Memo = "",
                    TranDate = order.InboundDate.ToString("yyyy-MM-dd"),
                    Lines = lines
                }
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public ReturnData SendDirectOrderShippedRequest(FBAShipOrder order, IEnumerable<FBAPickDetailCarton> pickedCtnList)
        {
            var url = "https://2298410.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=297&deploy=1";

            var skuList = new List<Sku>();

            foreach(var p in pickedCtnList)
            {
                skuList.Add(new Sku
                {
                    SkuNo = p.FBACartonLocation.ShipmentId,
                    SkuCode = p.FBACartonLocation.ShipmentId,
                    Qty = 1
                });
            }

            var body = new DirectSaleOrderRequestBody
            {
                DeliverOrderId = order.Id.ToString(),
                SourceId = order.Id.ToString(),
                SourceNo = order.ShipOrderNumber,
                TrackNo = "1",
                SkuList = new List<Sku>()
            };

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public static string SendHttpRequest(string url, string stringifiedJsonData, string method)
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
            request.Accept = "application/json";
            request.UserAgent = "APIExplorer";
            //request.Headers.Add("Authorization", )
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
    }

    public class TransOrderRequestBody
    {
        public dynamic Data { get; set; }
    }

    public class TransLine
    {
        public string ItemNum { get; set; }

        public int Quantity { get; set; }
    }

    public class ShippedData
    {
        public string TransOrderNo { get; set; }

        public string TransOrderId { get; set; }

        public string TranDate { get; set; }

        public string Memo { get; set; }

        public ICollection<TransLine> Lines { get; set; }
    }

    public class ReturnData
    {
        public string RETURN_CODE { get; set; }

        public TransOrderReturnDate RETURN_DATA { get; set; }

        public string RETURN_MSG { get; set; }
    }

    public class TransOrderReturnDate
    {
        public string TransOrderNo { get; set; }

        public bool Status { get; set; }

        public string ErrMsg { get; set; }
    }

    public class Sku
    {
        public string SkuNo { get; set; }

        public string SkuCode { get; set; }

        public int Qty { get; set; }
    }

    public class DirectSaleOrderRequestBody
    {
        public string DeliverOrderId { get; set; }

        public string SourceNo { get; set; }

        public string SourceId { get; set; }

        public string TrackNo { get; set; }

        public ICollection<Sku> SkuList { get; set; }
    }
}