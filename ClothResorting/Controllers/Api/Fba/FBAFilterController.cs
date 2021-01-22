using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
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
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
            _logger = new Logger(_context);
        }

        // POST /api/fba/fbafilter/?userId={foo}
        [HttpGet]
        public IHttpActionResult GetWarehouseLocations([FromUri]string userId)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == userId);
            var warehouseAuths = user.WarehouseAuths.Split(',');
            var locationInDb = _context.WarehouseLocations.Where(x => x.IsActive == true && warehouseAuths.Contains(x.WarehouseCode));
            var result = AutoMapper.Mapper.Map<IEnumerable<WarehouseLocation>, IEnumerable<WarehouseLocationDto>>(locationInDb);
            return Ok(result);
        }

        // POST /api/fba/fbafilter/?orderType={orderType}
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
