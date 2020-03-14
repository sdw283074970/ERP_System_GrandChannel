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

            var newItem = new ChargingItem
            {
                ChargingType = obj.ChargingType,
                Name = obj.Name,
                Rate = obj.Rate,
                Description = obj.Description,
                Unit = obj.Unit
            };

            //如果customerId等于0，就通过名字和部门代码来找到customer
            if (obj.CustomerId == 0)
            {
                vendorInDb = _context.UpperVendors
                    .SingleOrDefault(x => x.Name == obj.Vendor && x.DepartmentCode == obj.DepartmentCode);

                var sameNameItem = _context.ChargingItems
                    .Include(x => x.UpperVendor)
                    .Where(x => x.UpperVendor.Name == obj.Vendor && x.Name == obj.Name);

                if (sameNameItem.Count() != 0)
                {
                    throw new Exception("The name: " + obj.Name + " has already been taken. Please change it and try again.");
                }

                newItem.UpperVendor = vendorInDb;
                _context.ChargingItems.Add(newItem);
            }
            else
            {
                vendorInDb = _context.UpperVendors.Find(obj.CustomerId);

                //是否将新加的收费项目应用到所有FBA客户
                if (!obj.IsApplyToAll)
                {
                    var sameNameItem = _context.ChargingItems
                        .Include(x => x.UpperVendor)
                        .Where(x => x.UpperVendor.Id == obj.CustomerId && x.Name == obj.Name);

                    if (sameNameItem.Count() != 0)
                    {
                        throw new Exception("The name: " + obj.Name + " has already been taken. Please change it and try again.");
                    }

                    newItem.UpperVendor = vendorInDb;
                    _context.ChargingItems.Add(newItem);
                }
                else
                {
                    var fbaCustomersInDb = _context.UpperVendors
                        .Include(x => x.ChargingItems)
                        .Where(x => x.DepartmentCode == "FBA");

                    var itemList = new List<ChargingItem>();

                    var customerList = fbaCustomersInDb.ToList();

                    var sameNameItem = _context.ChargingItems
                        .Include(x => x.UpperVendor)
                        .Where(x => x.Name == obj.Name)
                        .ToList();

                    foreach (var s in sameNameItem)
                    {
                        customerList.Remove(customerList.SingleOrDefault(x => x.CustomerCode == s.UpperVendor.CustomerCode));
                    }

                    foreach (var c in customerList)
                    {
                        var customerInDb = fbaCustomersInDb.SingleOrDefault(x => x.CustomerCode == c.CustomerCode);
                        itemList.Add(new ChargingItem
                        {
                            ChargingType = obj.ChargingType,
                            Name = obj.Name,
                            Rate = obj.Rate,
                            Description = obj.Description,
                            Unit = obj.Unit,
                            UpperVendor = customerInDb
                        });
                    }

                    _context.ChargingItems.AddRange(itemList);
                }
            }

            _context.SaveChanges();

            var sampleDto = Mapper.Map<ChargingItem, ChargingItemDto>(_context.ChargingItems.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // PUT /api/chargingitemmanagement/?itemId={itemId}
        [HttpPut]
        public IHttpActionResult UpdateChargingItem([FromUri]int itemId, [FromBody]ChargingItemJsonObj obj)
        {
            var itemInDb = _context.ChargingItems.Find(itemId);

            if (!obj.IsApplyToAll)
            {
                itemInDb.ChargingType = obj.ChargingType;
                itemInDb.Name = obj.Name;
                itemInDb.Unit = obj.Unit;
            }
            else
            {
                //直接更改所有客户与目标名称相同项目的名称，类型和单位
                var sameOriginalNameItemsInDb = _context.ChargingItems
                    .Where(x => x.Name == itemInDb.Name);

                //首先查找每一个客户下是否已经有同名的要更改的目标名称的收费项目。如果有，则报错
                var sameOriginalNameBeforeModified = _context.ChargingItems
                    .Where(x => x.Name == obj.Name);

                if (sameOriginalNameBeforeModified.Count() > 0)
                {
                    throw new Exception("名称 " + obj.Name + " 已经被某些客户的收费项目占用。请联系管理员解决。");
                }

                foreach(var s in sameOriginalNameItemsInDb)
                {
                    s.Name = obj.Name;
                    s.ChargingType = obj.ChargingType;
                    s.Unit = obj.Unit;
                }

                //直接更改所已经收费项目中与目标名称相同项目的名称，类型和单位
                var sameOriginalNameChargedItemInDb = _context.InvoiceDetails
                    .Where(x => x.Activity == itemInDb.Name);

                foreach(var s in sameOriginalNameChargedItemInDb)
                {
                    s.Activity = obj.Name;
                    s.ChargingType = obj.ChargingType;
                    s.Unit = obj.Unit;
                }
            }

            itemInDb.Rate = obj.Rate;
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
