using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.ApiTransformModels;

namespace ClothResorting.Controllers.Api
{
    public class ChargingItemManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public ChargingItemManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/chargingitemmanagement/?vendor={vendor}&departmentCode={departmentCode}
        [HttpGet]
        public IHttpActionResult GetChargingItems([FromUri]string vendor, [FromUri]string departmentCode)
        {
            var chargingItemsDto = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Name == vendor && x.UpperVendor.DepartmentCode == departmentCode)
                .Select(Mapper.Map<ChargingItem, ChargingItemDto>);

            return Ok(chargingItemsDto);
        }

        // GET /api/chargingitemmanagement/?itemId={itemId}
        [HttpGet]
        public IHttpActionResult GetChargingItemsFromItemId([FromUri]int itemId)
        {
            var chargingItemsDto = Mapper.Map < ChargingItem, ChargingItemDto>(_context.ChargingItems.Find(itemId));
            return Ok(chargingItemsDto);
        }

        // GET /api/chargingitemmanagement/?customerId={customerId}
        [HttpGet]
        public IHttpActionResult GetChargingItemsFromCustomerId([FromUri]int customerId)
        {
            var chargingItemsDto = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Id == customerId)
                .Select(Mapper.Map<ChargingItem, ChargingItemDto>);

            return Ok(chargingItemsDto);
        }

        // POST /api/chargingitemmanagement/
        [HttpPost]
        public IHttpActionResult CreateNewChargingItem([FromBody]ChargingItemJsonObj obj)
        {
            UpperVendor vendorInDb = null;

            //如果customerId等于0，就通过名字和部门代码来找到customer
            if (obj.CustomerId == 0)
            {
                vendorInDb = _context.UpperVendors
                    .SingleOrDefault(x => x.Name == obj.Vendor && x.DepartmentCode == obj.DepartmentCode);

                var sameNameItem = _context.ChargingItems
                    .Include(x => x.UpperVendor)
                    .Where(x => x.UpperVendor.Name == obj.Vendor && x.UpperVendor.DepartmentCode == obj.DepartmentCode && x.Name == obj.Name);

                if (sameNameItem.Count() != 0)
                {
                    throw new Exception("The name: " + obj.Name + " has already been taken. Please change it and try again.");
                }
            }
            else
            {
                vendorInDb = _context.UpperVendors.Find(obj.CustomerId);

                var sameNameItem = _context.ChargingItems
                    .Include(x => x.UpperVendor)
                    .Where(x => x.UpperVendor.Id == obj.CustomerId && x.Name == obj.Name);

                if (sameNameItem.Count() != 0)
                {
                    throw new Exception("The name: " + obj.Name + " has already been taken. Please change it and try again.");
                }
            }

            var newItem = new ChargingItem
            {
                ChargingType = obj.ChargingType,
                Name = obj.Name,
                Rate = obj.Rate,
                Description = obj.Description,
                Unit = obj.Unit,
                UpperVendor = vendorInDb
            };



            _context.ChargingItems.Add(newItem);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<ChargingItem, ChargingItemDto>(_context.ChargingItems.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // PUT /api/chargingitemmanagement/?itemId={itemId}
        [HttpPut]
        public IHttpActionResult UpdateChargingItem([FromUri]int itemId, [FromBody]ChargingItemJsonObj obj)
        {
            var itemInDb = _context.ChargingItems.Find(itemId);

            itemInDb.ChargingType = obj.ChargingType;
            itemInDb.Name = obj.Name;
            itemInDb.Rate = obj.Rate;
            itemInDb.Unit = obj.Unit;
            itemInDb.Description = obj.Description;

            var sameNameItem = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Name == obj.Vendor && x.UpperVendor.DepartmentCode == obj.DepartmentCode && x.Name == obj.Name);

            if (sameNameItem.Count() != 0)
            {
                throw new Exception("The name: " + obj.Name + " has already been taken. Please change it and try again.");
            }

            _context.SaveChanges();

            var sampleDto = Mapper.Map<ChargingItem, ChargingItemDto>(_context.ChargingItems.Find(itemId));

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // DELETE /api/chargingitemmanagement/?itemId={itemId}
        [HttpDelete]
        public void DeleteItem([FromUri]int itemId)
        {
            var itemInDb = _context.ChargingItems.Find(itemId);
            _context.ChargingItems.Remove(itemInDb);
            _context.SaveChanges();
        }
    }
}
