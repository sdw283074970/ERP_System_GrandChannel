using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Helpers
{
    public class Logger
    {
        private ApplicationDbContext _context;
        private string _userName;
        private HttpContext _httpContext;

        public Logger(ApplicationDbContext context)
        {
            _context = context;
            _httpContext = HttpContext.Current;
            //_userName = _httpContext.User.Identity.Name.Split('@')[0];
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        public Logger(ApplicationDbContext context, string userName)
        {
            _context = context;
            _userName = userName;
        }

        public async Task AddCreatedLogAsync<T>(object oldValue, object newValue, string description, string exception, string level) where T : class
        {
            var log = await CreateLogAsync<T>(oldValue, newValue, description, exception, level);
            
            log.OperationType = OperationType.Create;

            _context.OperationLogs.Add(log);
            _context.SaveChanges();
        }

        public async Task AddUpdatedLogAndSaveChangesAsync<T>(object oldValue, object newValue, string description, string exception, string level) where T : class
        {
            var log = await CreateLogAsync<T>(oldValue, newValue, description, exception, level);

            log.OperationType = OperationType.Update;

            _context.OperationLogs.Add(log);
            _context.SaveChanges();
        }

        public async Task AddDeletedLogAsync<T>(object oldValue, string description, string exception, string level) where T : class
        {
            var log = await CreateLogAsync<T>(oldValue, null, description, exception, level);

            log.OperationType = OperationType.Delete;

            _context.OperationLogs.Add(log);
        }

        private async Task<OperationLog> CreateLogAsync<T>(object oldValue, object newValue, string description, string exception, string level) where T : class
        {
            var entityName = _context.GetTableName<T>();
            var oldValueStr = JsonConvert.SerializeObject(oldValue);
            var newValueStr = JsonConvert.SerializeObject(newValue);

            MemoryStream m = new MemoryStream();
            _httpContext.Request.InputStream.CopyTo(m);
            m.Position = 0;

            using ( StreamReader reader = new StreamReader(m))
            {
                var body = await reader.ReadToEndAsync();

                var newLog = new OperationLog
                {
                    Description = description,
                    Level = level,
                    Exception = exception,
                    OldValue = oldValueStr,
                    NewValue = newValueStr,
                    OperationDate = DateTime.Now,
                    RequestUri = _httpContext.Request.Url.AbsoluteUri,
                    User = _userName,
                    EntityName = entityName,
                    RequestBody = body,
                    UserIp = GetIPAddress()
                };

                return newLog;
            }
        }

        protected string GetIPAddress()
        {
            string ipAddress = _httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return _httpContext.Request.ServerVariables["REMOTE_ADDR"];
        }
    }

    public static class ContextExtensions
    {
        public static string GetTableName<T>(this ApplicationDbContext context) where T : class
        {
            ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;

            return objectContext.GetTableName<T>();
        }

        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            string sql = context.CreateObjectSet<T>().ToTraceString();
            Regex regex = new Regex(@"FROM\s+(?<table>.+)\s+AS");
            Match match = regex.Match(sql);

            string table = match.Groups["table"].Value;
            return table;
        }
    }
}