﻿using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class ReplenishmentOrderOutboundController : ApiController
    {
        private ApplicationDbContext _context;

        public ReplenishmentOrderOutboundController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/ReplenishmentOrderOutbound
        //[HttpPost]
        //public IHttpActionResult CreatePermanentLocIORecord()
        //{
        //    var fileSavePath = "";

        //    //从httpRequest中获取文件并写入磁盘系统
        //    var filesGetter = new FilesGetter();

        //    fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"E:\TempFiles\");

        //    if (fileSavePath == "")
        //    {
        //        return BadRequest();
        //    }

        //    var extractor = new LoadPlanExtracter(fileSavePath);
        //    var recorder = new OutboundRecorder();

        //    var pickRequests = extractor.GetPickRequestsFromXlsx();

        //    var records = recorder.OutputReplenishmentOrderIORecord(pickRequests);

        //    var resultDto = Mapper.Map<IEnumerable<PermanentLocIORecord>, IEnumerable<PermanentLocIORecordDto>>(records);

        //    //强行关闭进程
        //    var killer = new ExcelKiller();

        //    killer.Dispose();

        //    return Created(Request.RequestUri + "/" + 12345, resultDto);
        //}
    }
}
