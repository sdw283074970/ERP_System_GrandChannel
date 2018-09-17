using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class InventorySearchController : ApiController
    {
        private ApplicationDbContext _context;

        public InventorySearchController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/inventorysearch/?container={container}&purchaseOrder={po}&style={style}&color={color}&customer={customer}&size={size}&location={location}
        [HttpGet]
        public IHttpActionResult SearchInInventory([FromUri]string container, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string customer, [FromUri]string size, [FromUri]string location)
        {
            var locationDetailsInDb = _context.FCRegularLocationDetails
                .Where(x => x.AvailableCtns != 0 || x.AvailablePcs != 0);
            return Ok();
        }
    }
}
