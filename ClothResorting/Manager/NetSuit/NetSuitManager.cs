using ClothResorting.Helpers;
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

namespace ClothResorting.Manager.NetSuit
{
    public class NetSuitManager
    {
        private Logger _logger;
        private ApplicationDbContext _context;

        public NetSuitManager(ApplicationDbContext context)
        {
            _context = context;
            _logger = new Logger(_context);
        }

        public ReturnData SendStandardOrderShippedRequest(FBAShipOrder order, IEnumerable<FBAPickDetailCarton> pickedCtnList)
        {
            var url = "https://5802100-sb1.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=425&deploy=1";

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

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            }), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public ReturnData SendStandardOrderInboundRequest(FBAMasterOrder order)
        {
            var url = "https://5802100-sb1.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=430&deploy=1";

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

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            }), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public ReturnData SendDirectSellOrderShippedRequest(FBAShipOrder order, IEnumerable<FBAPickDetailCarton> pickedCtnList)
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

            var responseString = SendHttpRequest(url, JsonConvert.SerializeObject(body, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            }), "POST");

            var responseBody = new ReturnData();

            using (var input = new StringReader(responseString))
            {
                responseBody = JsonConvert.DeserializeObject<ReturnData>(responseString);
            }

            return responseBody;
        }

        public string SendHttpRequest(string url, string stringifiedJsonData, string method)
        {
            var result = string.Empty;

            var oauth = new OAuth.Manager();
            oauth["consumer_key"] = "43981534e855f4adca425575b4328a702ade9500a78447f2bff4dedcb3af753b";
            oauth["consumer_secret"] = "fea909450f17d77cb578c45f59d8829bcaf18cd649f9b5edd1d0af24506e5079";
            oauth["token"] = "bc9728a02cceb69ed4d02e80e8bed8078526395affdd4c13108b7fcaeead58b1";
            oauth["token_secret"] = "5f44e752e4af109cdb0208895382113d5dd6ab907b065b6100a80e5874089d59";
            var realm = "5802100_SB1";

            var authzHeader = oauth.GenerateCredsHeader(url, "POST", realm);

            //发送请求
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.UTF8.GetBytes(stringifiedJsonData);

            request.Method = method;
            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
            request.Headers.Add("Authorization", authzHeader);

            request.ContentType = "application/json";
            request.KeepAlive = false;
            request.ServicePoint.Expect100Continue = false;
            request.ContentLength = data.Length;
            request.Accept = "application/json";
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