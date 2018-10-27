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

namespace ClothResorting.Helpers
{
    public class QBOServiceManager
    {
        private ApplicationDbContext _context;
        private string _baseUrl;
        private string _userId;
        private OAuthInfo _oauthInfo;

        public QBOServiceManager()
        {
            _context = new ApplicationDbContext();
            _baseUrl = QuickBookBaseURLs.SandBox;
            _userId = HttpContext.Current.User.Identity.GetUserId<string>();
            _oauthInfo = _context.Users
                .Include(x => x.OAuthInfo)
                .SingleOrDefault(x => x.Id == _userId)
                .OAuthInfo
                .SingleOrDefault(x => x.PlatformName == Platform.QBO);
        }

        //将系统中的收费服务项目与QBO中的收费服务对比，将系统中有但QBO中没有的项目同步到QBO中去
        public void SyncChargingItemToQBO()
        {
            //获取系统中所有不重名的收费项目列表(待解决)
            var itemList = _context.ChargingItems.ToList();
            //var list = _context.ChargingItems.Select(x => new { Type = x.ChargingType, Name = x.Name.Distinct() });

            //获取QBO中的收费列表
            var queryStatement = "SELECT * FROM Item WHERE Type = 'Service'";
            var stringifiedJsonObj = WebServiceManager.SendQueryRequest(QBOUrlGenerator.QueryRequestUrl(_baseUrl, _oauthInfo.RealmId, queryStatement), _oauthInfo.AccessToken);

            //将获得的Json对象反序列化
            IList<string> result = new List<string>();
            using (var input = new StringReader(stringifiedJsonObj))
            {
                result = JSON.Deserialize<IList<ChargingItem>>(input).Select(x => x.Name).ToList();
            }

            //对比两表，将重复的表抛弃
            foreach(var item in itemList)
            {
                if (result.Contains(item.Name))
                {
                    itemList.Remove(item);
                }
            }

            //将筛选后的表做成QBO能接受的对象格式并序列化
            string stringifieJsonDate = string.Empty;
            using (var output = new StringWriter())
            {
                JSON.Serialize(itemList, output);
                stringifieJsonDate = output.ToString();
            }

            //调用建立新Item的API在QBO中建立不重复的收费项目
            var response = WebServiceManager.SendCreateRequest(QBOUrlGenerator.CreateRequestUrl(_baseUrl, _oauthInfo.RealmId, "Item"), stringifieJsonDate, "POST", _oauthInfo.AccessToken);
        }
    }
}