using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
using System.Security.Cryptography;
using System.Web.UI;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using ClothResorting.Helpers;
using Microsoft.AspNet.Identity;
using ClothResorting.Models;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class QuickBooksOnlineOAuthController : ApiController
    {
        private IntuitOAuthor _intuitOAuth;
        private ApplicationDbContext _context;

        public QuickBooksOnlineOAuthController()
        {
            _intuitOAuth = new IntuitOAuthor();
            _context = new ApplicationDbContext();
        }

        // GET /api/qucikbooksonlineOauth/
        [HttpGet]
        public IHttpActionResult GeAuthtReturnUrl()
        {
            var authURL = _intuitOAuth.ConnectToQB();

            return Ok(authURL);
        }

        // POST /api/quickbooksonlineoauth/?state={state}&code={code}&realmId={realmId}
        [HttpPost]
        public IHttpActionResult GetTokenResponse([FromUri]string state, [FromUri]string code, [FromUri]string realmId)
        {
            var userId = User.Identity.GetUserId<string>();

            System.Threading.Tasks.Task.Run(async() => { await _intuitOAuth.GetAccessToken(userId, code, realmId); }).Wait(); 

            return Created(Request.RequestUri + "/" + userId, code);
        }

        // POST /apiquickbooksonlineoauth/{id}(invoiceId)
        [HttpPost]
        public IHttpActionResult SyncInvoiceToQBO([FromUri]int id)
        {
            var invoiceInDb = _context.Invoices
                .Include(x => x.InvoiceDetails)
                .SingleOrDefault(x => x.Id == id);

            //使用Refresh token刷新或得新鲜的Access Token
            var userId = User.Identity.GetUserId<string>();

            System.Threading.Tasks.Task.Run(async () => { await _intuitOAuth.RefreshToken(userId); }).Wait();

            //同步invoice到QBO中

            //查询所有的客户
            var result = _intuitOAuth.GetAllCustomer(userId);

            return Created(Request.RequestUri, result);
        }
    }
}
