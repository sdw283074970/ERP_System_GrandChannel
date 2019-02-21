using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInventoryIndexController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAInventoryIndexController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbainventoryindex/?closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetRemainCustomerList([FromUri]DateTime closeDate)
        {


            return Ok();
        }
    }
}
