using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class LocationDetailUnDoController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationDetailUnDoController()
        {
            _context = new ApplicationDbContext();
        }

        // DELETE /api/locaitionDetailUnDo
        public void UnDoUpload()
        {

        }
    }
}
