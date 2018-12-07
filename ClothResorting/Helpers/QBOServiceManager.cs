using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Jil;
using ClothResorting.Models.QBOModels;
using Newtonsoft.Json;
using System.Configuration;

namespace ClothResorting.Helpers
{
    public class QBOServiceManager
    {
        private ApplicationDbContext _context;
        private string _baseUrl;
        private string _userId;

        public QBOServiceManager()
        {
            _context = new ApplicationDbContext();
            _baseUrl = ConfigurationManager.AppSettings["baseUrl"];
            _userId = HttpContext.Current.User.Identity.GetUserId<string>();
        }

        //将系统中的收费服务项目与QBO中的收费服务对比，将系统中有但QBO中没有的项目同步到QBO中去
        #region Steps description
            //1.检查公司ERP系统中的收费项目是否与QBO中的收费项目同步，并同步未同步的部分
            //2.检查公司ERP系统中的Customer是否与QBO中的Customer同步，如已同步则查询对应Customer的Value，否则报错，显示要求手动同步
            //3.将要同步的Invoice对象转换成QBO能识别的对象并序列化
            //4.同步Invoice
        #endregion
        public void SyncInvoice(int invoiceId)
        {
            var oauthInfo = _context.Users
                .Include(x => x.OAuthInfo)
                .SingleOrDefault(x => x.Id == _userId)
                .OAuthInfo
                .SingleOrDefault(x => x.PlatformName == Platform.QBO);

            var invoiceInDb = _context.Invoices
                .Include(x => x.InvoiceDetails)
                .Include(x => x.UpperVendor)
                .SingleOrDefault(x => x.Id == invoiceId);

            var companyName = invoiceInDb.UpperVendor.Name;

            var companyId = string.Empty;

            #region Step 1
            //获取系统中所有不重名的收费项目列表(未解决)
            var itemList = _context.ChargingItems.ToList();

            //获取QBO中的收费列表
            var itemQuery = "SELECT * FROM Item WHERE Type = 'Service'";
            var itemJsonResponseData = WebServiceManager.SendQueryRequest(QBOUrlGenerator.QueryRequestUrl(_baseUrl, oauthInfo.RealmId, itemQuery), oauthInfo.AccessToken);

            //将获得的Json对象反序列化
            var itemResponseBody = new ItemResponseBody();
            using (var input = new StringReader(itemJsonResponseData))
            {
                //var responseBody = JSON.Deserialize<ResponseBody>(input);     //Jil的反序列化方法不太好用
                itemResponseBody = JsonConvert.DeserializeObject<ItemResponseBody>(itemJsonResponseData);
            }

            //对比两表，将不重复的item做成QBO能接受的对象格式并序列化
            foreach (var item in itemList)
            {
                if (itemResponseBody.QueryResponse.Item.Where(x => x.Name == item.Name).Count() == 0)
                {
                    var itemCreateRequestModel = new ItemCreateRequestModel {
                        //IncomeAccountRef = new IncomeAccountRef { Value = "26"},    //默认关联账户是1 Services账户
                        IncomeAccountRef = new IncomeAccountRef { Value = "1" },    //默认关联账户是1 Services账户
                        Name = item.Name,
                        Type = "Service"
                    };

                    string itemJsonData = JsonConvert.SerializeObject(itemCreateRequestModel);
                    //调用建立新Item的API在QBO中建立不重复的收费项目
                    var itemResponse = WebServiceManager.SendCreateRequest(QBOUrlGenerator.CreateRequestUrl(_baseUrl, oauthInfo.RealmId, "item"), itemJsonData, "POST", oauthInfo.AccessToken);
                }
            }
            #endregion

            #region Step 2
            var customerQuery = "SELECT * FROM CUSTOMER WHERE COMPANYNAME = '" + companyName + "'";

            var companyNameJsonResponseData = WebServiceManager.SendQueryRequest(QBOUrlGenerator.QueryRequestUrl(_baseUrl, oauthInfo.RealmId, customerQuery), oauthInfo.AccessToken);

            var customerResponseBody = new CustomerResponseBody();
            using (var input = new StringReader(itemJsonResponseData))
            {
                customerResponseBody = JsonConvert.DeserializeObject<CustomerResponseBody>(companyNameJsonResponseData);
            }

            //如果响应表示QBO中没有我们要查询的公司，则报错，提示手动在QBO中建立
            if (customerResponseBody.QueryResponse.MaxResults == 0)
            {
                throw new Exception("Company " + companyName + " was not found in QuickBooks. Please create that company in QuickBooks first and try again.");
            }
            else
            {
                companyId = customerResponseBody.QueryResponse.Customer.First().Id;
            }
            #endregion

            #region Step 3
            var invoice = new InvoiceCreateRequestBody {
                CustomerRef = new CustomerRef(),
            };
            var lineList = new List<Line>();

            //查询目标Invoice中的收费项目分别在QBO中的Id(value)
            //再次查询收费项目列表并将响应反序列化
            itemJsonResponseData = WebServiceManager.SendQueryRequest(QBOUrlGenerator.QueryRequestUrl(_baseUrl, oauthInfo.RealmId, itemQuery), oauthInfo.AccessToken);
            itemResponseBody = new ItemResponseBody();
            using (var input = new StringReader(itemJsonResponseData))
            {
                itemResponseBody = JsonConvert.DeserializeObject<ItemResponseBody>(itemJsonResponseData);
            }

            if (invoiceInDb.InvoiceDetails.Count == 0)
            {
                throw new Exception("Upload failed because the invoice is empty.");
            }

            //生成QBO能接受的invoice格式
            foreach (var i in invoiceInDb.InvoiceDetails)
            {
                var line = new Line {
                    Amount = i.Rate * i.Quantity,
                    DetailType = "SalesItemLineDetail",
                    SalesItemLineDetail = new SalesItemLineDetail {
                        ItemRef = new ItemRef { Value = itemResponseBody.QueryResponse.Item.SingleOrDefault(x => x.Name == i.Activity).Id },
                        UnitPrice = i.Rate,
                        Qty = i.Quantity
                    }
                };

                lineList.Add(line);
            }

            invoice.Line = lineList;
            invoice.CustomerRef.Value = companyId;

            var invoiceJsonData = JsonConvert.SerializeObject(invoice);
            var invoiceJsonResponseData = WebServiceManager.SendCreateRequest(QBOUrlGenerator.CreateRequestUrl(_baseUrl, oauthInfo.RealmId, "invoice"), invoiceJsonData, "POST", oauthInfo.AccessToken);
            
            //可以将返回的数据继续与数据库的invoice数据同步，暂留
            #endregion
        }
    }
}