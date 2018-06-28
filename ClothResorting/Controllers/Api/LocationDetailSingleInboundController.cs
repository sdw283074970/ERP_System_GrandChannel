﻿using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class LocationDetailSingleInboundController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow = DateTime.Now;

        public LocationDetailSingleInboundController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/locationdetailsingleinbound
        [HttpPost]
        public IHttpActionResult CreateSingleInboundRecord([FromBody]SingleInboundJsonObj obj)
        {
            var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
                .Single(c => c.PurchaseOrder == obj.PurchaseOrder);

            var speciesInDbs = _context.SpeciesInventories.Where(c => c.Id > 0);

            var record = new LocationDetail {
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Size = obj.Size,
                InboundDate = _timeNow,
                OrgNumberOfCartons = obj.Ctns,
                InvNumberOfCartons = obj.Ctns,
                OrgPcs = obj.Quantity,
                InvPcs = obj.Quantity,
                Location = obj.Location,
                PurchaseOrderInventory = purchaseOrderInventoryInDb
            };

            //检查speciesInDbToList中是否有这种类型的记录。有则直接在种类的基础上加pcs数量
            var speciesInDb = speciesInDbs
                .SingleOrDefault(c => c.PurchaseOrder == record.PurchaseOrder
                    && c.Style == record.Style
                    && c.Color == record.Color
                    && c.Size == record.Size);

            //如果没有则说明这是新种类，添加进数据库
            if (speciesInDb == null)
            {
                _context.SpeciesInventories.Add(new SpeciesInventory {
                    PurchaseOrder = obj.PurchaseOrder,
                    Style = obj.Style,
                    Color = obj.Color,
                    Size = obj.Size,
                    AdjPcs = obj.Quantity,
                    OrgPcs = obj.Quantity,
                    InvPcs = obj.Quantity
                });

                _context.LocationDetails.Add(record);
                _context.SaveChanges();
            }
            else//如果有，则调整该种类的pcs数据
            {

                purchaseOrderInventoryInDb.InvPcs += obj.Quantity;
                purchaseOrderInventoryInDb.InvCtns += obj.Ctns;

                speciesInDb.OrgPcs += obj.Quantity;
                speciesInDb.AdjPcs += obj.Quantity;
                speciesInDb.InvPcs += obj.Quantity;

                _context.LocationDetails.Add(record);
                _context.SaveChanges();
            }
            
            var sample = _context.LocationDetails
                .OrderByDescending(c => c.Id)
                .First();

            //每添加一次单条inbound记录，返回所有结果，实现局部刷新表格
            var result = new List<LocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PurchaseOrderInventory)
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder)
                .OrderByDescending(c => c.Id)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            return Created(Request.RequestUri + "/" + sample.Id, resultDto);
        }
    }
}
