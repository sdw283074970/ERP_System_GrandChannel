using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class UpperVendorController : ApiController
    {
        private ApplicationDbContext _context;

        public UpperVendorController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/uppervendor/
        [HttpGet]
        public IHttpActionResult GetAllUpperVendors()
        {
            var list = new List<string>();

            var vendors = _context.UpperVendors.Where(x => x.Id > 0);

            foreach(var vendor in vendors)
            {
                list.Add(vendor.Name);
            }

            return Ok(list.ToArray());
        }
    }
}
