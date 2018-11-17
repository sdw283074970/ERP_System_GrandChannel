using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

//namespace ClothResorting.Controllers.Api
//{
//    public class PermanentLocManagementController : ApiController
//    {
//        private ApplicationDbContext _context;

//        public PermanentLocManagementController()
//        {
//            _context = new ApplicationDbContext();
//        }

//        // GET /api/permanentlocmanagement
//        [HttpGet]
//        public IHttpActionResult GetAllPermanentLocation()
//        {
//            return Ok(_context.PermanentLocations
//                .Where(c => c.Id > 0)
//                .OrderByDescending(c => c.Id)
//                .ToList()
//                .Select(Mapper.Map<PermanentLocation, PermanentLocationDto>));
//        }

//        // POST /api/permanentlocmanagement
//        [HttpPost]
//        public IHttpActionResult CreateNewPermanentLocation([FromBody]PermanentLocJsonObj obj)
//        {
//            var location = new PermanentLocation
//            {
//                Location = obj.Location,
//                Vender = obj.Vender,
//                PurchaseOrder = obj.PurchaseOrder,
//                Style = obj.Style,
//                Color = obj.Color,
//                Size = obj.Size,
//                Quantity = 0
//            };

//            _context.PermanentLocations.Add(location);

//            _context.SaveChanges();

//            var id = _context.PermanentLocations.OrderByDescending(c => c.Id).First().Id;

//            var results = _context.PermanentLocations
//                .Where(c => c.Id > 0)
//                .OrderByDescending(c => c.Id)
//                .ToList()
//                .Select(Mapper.Map<PermanentLocation, PermanentLocationDto>);

//            return Created(Request.RequestUri + "/" + id, results);
//        }
//    }
//}
