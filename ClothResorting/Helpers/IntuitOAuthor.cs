using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web.UI;
using System.Configuration;
using System.Web;
using Intuit.Ipp.OAuth2PlatformClient;
using System.Threading.Tasks;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Intuit.Ipp.Exception;
using System.Linq;
using Intuit.Ipp.ReportService;
using ClothResorting.Models;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Helpers
{
    public class IntuitOAuthor
    {
        private ApplicationDbContext _context;

        // OAuth2 client configuration
        static string redirectURI = ConfigurationManager.AppSettings["redirectUrl"];
        static string clientID = ConfigurationManager.AppSettings["clientId"];
        static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string appEnvironment = ConfigurationManager.AppSettings["appEnvironment"];
        static OAuth2Client oauthClient = new OAuth2Client(clientID, clientSecret, redirectURI, appEnvironment);
        //static string authCode;
        //static string idToken;
        //public static IList<JsonWebKey> keys;
        //public static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public IntuitOAuthor()
        {
            _context = new ApplicationDbContext();
        }

        //获取授权请求URL
        public string ConnectToQB()
        {
            output("Intiating OAuth2 call.");
            try
            {
                List<OidcScopes> scopes = new List<OidcScopes>();
                scopes.Add(OidcScopes.Accounting);
                var authorizationRequest = oauthClient.GetAuthorizationURL(scopes);

                return authorizationRequest;
            }
            catch (Exception ex)
            {
                output(ex.Message);
            }

            return "";
        }

        //获取通行口令
        public async System.Threading.Tasks.Task GetAccessToken(string userId, string code, string realmId)
        {
            var userInDb = _context.Users
                .Include(x => x.OAuthInfo)
                .SingleOrDefault(x => x.Id == userId);

            if (userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO) == null)
            {
                var tokenResponse = await oauthClient.GetBearerTokenAsync(code);

                var accessToken = tokenResponse.AccessToken;
                var refreshToken = tokenResponse.RefreshToken;

                _context.OAuthInfo.Add(new OAuthInfo
                {
                    PlatformName = Platform.QBO,
                    RefreshToken = refreshToken,
                    RealmId = realmId,
                    AccessToken = accessToken,
                    ApplicationUser = userInDb,
                    LastRequestCode = code
                });
            }
            //这个判断语句是为了防止用户二次刷新信息携带页面(code参数)造成的refreshToken无效的操作
            else if (code != userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).LastRequestCode)
            {
                var tokenResponse = await oauthClient.GetBearerTokenAsync(code);

                var accessToken = tokenResponse.AccessToken;
                var refreshToken = tokenResponse.RefreshToken;

                if (accessToken != null || refreshToken != null)
                {
                    userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).AccessToken = accessToken;
                    userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).RefreshToken = refreshToken;
                    userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).RealmId = realmId;
                    userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).LastRequestCode = code;
                }

                //验证返回的口令是否真的来自OAut服务器，如果是则将口令保存至数据库保存
                //var isTokenValid = await oauthClient.ValidateIDTokenAsync(tokenResponse.IdentityToken);

                //if (isTokenValid)
                //{
                //    _context.SaveChanges();
                //}

            }

            _context.SaveChanges();
        }

        //使用Refresh token刷新通行口令
        public async System.Threading.Tasks.Task RefreshToken(string userId)
        {
            var userInDb = _context.Users
                .Include(x => x.OAuthInfo)
                .SingleOrDefault(x => x.Id == userId);

            var lastRefreshToken = userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).RefreshToken;

            var tokenResponse = await oauthClient.RefreshTokenAsync(lastRefreshToken);

            var accessToken = tokenResponse.AccessToken;
            var refreshToken = tokenResponse.RefreshToken;

            if (userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO) != null)
            {
                userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).AccessToken = accessToken;
                userInDb.OAuthInfo.SingleOrDefault(x => x.PlatformName == Platform.QBO).RefreshToken = refreshToken;

                _context.SaveChanges();
            }
        }

        //调用API查询CUSTOMER。这是官方SDK方法，在建立URL时会报错无法识别URL，可能是自带方法的编码有BUG，弃用。
        //public Customer GetAllCustomer(string userId)
        //{
        //    var oauthInfo = _context.Users
        //        .Include(x => x.OAuthInfo)
        //        .SingleOrDefault(x => x.Id == userId)
        //        .OAuthInfo
        //        .SingleOrDefault(x => x.PlatformName == Platform.QBO);

        //    var oauthValidator = new OAuth2RequestValidator(oauthInfo.AccessToken);
        //    var serviceContext = new ServiceContext(oauthInfo.RealmId, IntuitServicesType.QBO, oauthValidator);

        //    serviceContext.RequestId = GuidGenerator.GenerateGuid();

        //    //因为Configuration的识别有问题，只能手动指定以下属性
        //    serviceContext.IppConfiguration.MinorVersion.Qbo = "8";
        //    //serviceContext.IppConfiguration.Message.Request.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Xml;
        //    //serviceContext.IppConfiguration.Message.Request.CompressionFormat = Intuit.Ipp.Core.Configuration.CompressionFormat.GZip;
        //    //serviceContext.IppConfiguration.Message.Response.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Xml;
        //    //serviceContext.IppConfiguration.Message.Response.CompressionFormat = Intuit.Ipp.Core.Configuration.CompressionFormat.GZip;

        //    var dataService = new DataService(serviceContext);

        //    serviceContext.RequestId = GuidGenerator.GenerateGuid();
        //    //var inService = new QueryService<Customer>(serviceContext);
        //    //try
        //    //{
        //        //var result = inService.ExecuteIdsQuery("SELECT * FROM Customer").FirstOrDefault();
        //        var customer = new Customer {
        //            GivenName = "QQQ",
        //            Title = "WWW",
        //            MiddleName = "EEE",
        //            FamilyName = "AAA"
        //        };

                

        //        var result = dataService.Add(customer) as Customer;

        //    //}
        //    //catch(Exception e)
        //    //{
        //    //    throw new Exception();
        //    //}
        //    //var customerTest = new CustomerCRUD();

        //    //var result = customerTest.CustomerFindAllTestUsingoAuth(serviceContext);

        //    return new Customer();
        //}

        #region 私有方法
        public void output(string logMsg)
        {
            StreamWriter sw = File.AppendText(GetLogPath() + "OAuth2SampleAppLogs.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, logMsg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }

        public string GetLogPath()
        {
            try
            {
                if (logPath == "")
                {
                    logPath = Environment.GetEnvironmentVariable("TEMP");
                    if (!logPath.EndsWith("\\")) logPath += "\\";
                }
            }
            catch
            {
                output("Log error path not found.");
            }
            return logPath;
        }
        #endregion
    }
}