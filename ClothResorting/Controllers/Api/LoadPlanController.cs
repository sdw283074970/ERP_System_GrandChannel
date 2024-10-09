﻿using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class LoadPlanController : ApiController
    {
        private ApplicationDbContext _context;

        public LoadPlanController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/loadplan/?shipOrderId={shipOrderId}
        [HttpPost]
        public void CreateReplenishmentPickDetailFormLoadPlan([FromUri]int shipOrderId)
        {
            var fileSavePath = "";
            var filesGetter = new FilesGetter();
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);

            fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"E:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var extractor = new LoadPlanExtracter(fileSavePath);

            extractor.PickReplenishmentLoadPlan(shipOrderId);

            shipOrderInDb.Status = Status.Picking;
            _context.SaveChanges();
        }
    }
}
