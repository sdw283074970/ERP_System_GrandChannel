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

        // GET /api/uppervendor/?departmentCode={departmentCode}
        [HttpGet]
        public IHttpActionResult GetAllUpperVendors([FromUri]string departmentCode)
        {
            var list = new List<string>();

            var vendors = _context.UpperVendors.Where(x => x.DepartmentCode == departmentCode).OrderBy(x => x.CustomerCode);

            if (departmentCode == "CD")
            {
                foreach (var vendor in vendors)
                {
                    list.Add(vendor.Name);
                }
            }
            else if (departmentCode == "FBA")
            {
                foreach (var vendor in vendors)
                {
                    list.Add(vendor.CustomerCode);
                }
            }

            return Ok(list.ToArray());
        }
    }
}
