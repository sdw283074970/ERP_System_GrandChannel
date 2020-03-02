using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAFilterController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;
        private Logger _logger;

        public FBAFilterController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@').First();
            _logger = new Logger(_context);
        }

        // POST /api/fba/fbamasterOrder/?orderType={orderType}
        [HttpPost]
        public IHttpActionResult GetCSRFilteredOrders([FromUri]string orderType, [FromBody]Filter filter)
        {
            var getter = new FBAGetter();

            if (orderType == FBAOrderType.MasterOrder)
                return Ok(getter.GetFilteredMasterOrder(filter));
            else
                return Ok(getter.GetFilteredShipOrder(filter));
        }
    }
}
