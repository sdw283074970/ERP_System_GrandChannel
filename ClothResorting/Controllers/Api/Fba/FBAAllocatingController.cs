using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAAllocatingController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAAllocatingController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbaallocating/?grandNumber={grandNumber}
        public IHttpActionResult GetAllocatablePallets([FromUri]string grandNumber)
        {
            return Ok(_context.FBAPallets.Where(x => x.GrandNumber == grandNumber).Select(Mapper.Map<FBAPallet, FBAPalletDto>));
        }
    }
}
