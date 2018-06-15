using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class RetrievingDetailPrePackOrderController : ApiController
    {
        private ApplicationDbContext _context;

        public RetrievingDetailPrePackOrderController()
        {
            _context = new ApplicationDbContext();
        }


    }
}
