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
    public class RegularPackingListController : ApiController
    {
        public ApplicationDbContext _context;

        public RegularPackingListController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/regularpackinglist/?preId={id}&vendor={vendor}
        [HttpPost]
        public void UploadRegularPackingList([FromUri]int preId, [FromUri]string vendor)
        {
            var fileSavePath = "";

            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"E:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractPOSummaryAndCartonDetail(preId, vendor);

            var killer = new ExcelKiller();

            killer.Dispose();
        }
    }
}
