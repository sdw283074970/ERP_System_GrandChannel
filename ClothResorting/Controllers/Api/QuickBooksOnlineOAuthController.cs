using System.Web.Http;
using System.Linq;
using System.Web;
using ClothResorting.Helpers;
using Microsoft.AspNet.Identity;
using ClothResorting.Models;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class QuickBooksOnlineOAuthController : ApiController
    {
        private IntuitOAuthor _intuitOAuth;
        private ApplicationDbContext _context;
        private string _userName;

        public QuickBooksOnlineOAuthController()
        {
            _intuitOAuth = new IntuitOAuthor();
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
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

        // POST /apiquickbooksonlineoauth/?invoiceId={invoiceId}
        [HttpPost]
        public IHttpActionResult SyncInvoiceToQBO([FromUri]int invoiceId)
        {
            var invoiceInDb = _context.Invoices
                .Include(x => x.InvoiceDetails)
                .SingleOrDefault(x => x.Id == invoiceId);

            //使用Refresh token刷新或得新鲜的Access Token
            var userId = User.Identity.GetUserId<string>();

            System.Threading.Tasks.Task.Run(async () => { await _intuitOAuth.RefreshToken(userId); }).Wait();

            //同步invoice到QBO中
            var service = new QBOServiceManager();

            var invoiceResult = service.SyncInvoiceToQBO(invoiceId);

            invoiceInDb.InvoiceNumber = invoiceResult.Invoice.DocNumber;
            invoiceInDb.UploadedDate = invoiceResult.Time;
            invoiceInDb.UploadedBy = _userName;

            _context.SaveChanges();

            return Created(Request.RequestUri, "Sync Success!");
        }
    }
}
