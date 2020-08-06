using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAexAPIValidator
    {
        private ApplicationDbContext _context;

        public FBAexAPIValidator()
        {
            _context = new ApplicationDbContext();
        }

        public JsonResponse ValidateSign(string appKey, UpperVendor customerInDb, string requestId, string version, string sign)
        {
            // 参数验证加密验证
            var auth = _context.AuthAppInfos.SingleOrDefault(x => x.AppKey == appKey);
            if (auth == null)
            {
                return new JsonResponse { Code = 500, ValidationStatus = "Validate failed", Message = "Unregistered app request." };
            }
            var vs = auth.SecretKey.ToUpper() + "&appKey=" + appKey + "&customerCode=" + customerInDb.CustomerCode + "&requestId=" + requestId + "&version=" + version;
            var md5sign = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(vs))).Replace("-", "");
            //var md5sign = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(vs))).Replace("-", "S");

            if (md5sign != sign)
            {
                return new JsonResponse { Code = 501, ValidationStatus = "Validate failed", Message = "Invalid sign." };
            }

            // 检查customerCode是否存在，否则返回错误

            if (customerInDb == null)
            {
                return new JsonResponse { Code = 501, ValidationStatus = "Validate failed", Message = "Invalid sign." };
            }

            //防止重放攻击和网络延迟等非攻击意向的二次请求，如请求重复则返回错误
            if (HttpContext.Current.Cache[requestId] == null)
            {
                // 如果没有requestId,则缓存10分钟
                HttpContext.Current.Cache.Insert(requestId, requestId, null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            else
            {
                return new JsonResponse { Code = 504, ValidationStatus = "Validate failed", Message = "Duplicated request detectived. Request Id: " + requestId + " has already been processed. Please report this request Id: " + requestId + " to CSR of Grand Channel for more support." };
            }

            // 防止重放攻击的二道关卡，如重复则返回错误
            var logInDb = _context.OperationLogs.Where(x => x.RequestId == requestId);
            if (logInDb.Count() != 0)
            {
                return new JsonResponse { Code = 504, ValidationStatus = "Validate failed", Message = "Duplicated request detectived. Request Id: " + requestId + " has already been processed. Please report this request Id: " + requestId + " to CSR of Grand Channel for more support." };
            }

            // 检查version是否支持，否则返回错误
            if (version != "V1")
            {
                return new JsonResponse { Code = 505, ValidationStatus = "Validate failed", Message = "Invalid API version." };
            }

            return new JsonResponse { Code = 200, ValidationStatus = "Validate success", Message = "Successful model validation." };
        }
    }
}