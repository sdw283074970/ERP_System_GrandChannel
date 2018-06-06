using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class LocationUpdateController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationUpdateController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/cartonlocation/输入选中的cartonBreakDown的id数列以及Location，更新至数据库
        [HttpPost]
        public IHttpActionResult UpdateLocation([FromBody]LocationUpdatePackage package)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var sampleId = package.Arr[0];

            foreach (var id in package.Arr)
            {
                var cartonBreakDownInDb = _context.CartonBreakDowns.SingleOrDefault(c => c.Id == id);
                cartonBreakDownInDb.Location = package.Location;
            }

            _context.SaveChanges();

            return Ok(_context.CartonBreakDowns.SingleOrDefault(c => c.Id == sampleId));
        }
    }
}
