using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

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
                var cartonDetailInDb = _context.SilkIconCartonDetails
                    .Include(s => s.CartonBreakdowns)
                    .SingleOrDefault(c => c.Id == id);
                cartonDetailInDb.Location = package.Location;

                //同步更新carton内部pcs的location
                foreach(var bk in cartonDetailInDb.CartonBreakdowns)
                {
                    bk.Location = package.Location;
                }
            }

            _context.SaveChanges();

            var cartonSample = _context.SilkIconCartonDetails
                .SingleOrDefault(c => c.Id == sampleId);

            var cartonDto = Mapper.Map<SilkIconCartonDetail, SilkIconCartonDetailDto>(cartonSample);

            return Created(new Uri(Request.RequestUri + "/" + sampleId), cartonDto);
        }
    }
}
