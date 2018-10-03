using ClothResorting.Helpers;
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
    public class SIRegularPLController : ApiController
    {
        public ApplicationDbContext _context;

        public SIRegularPLController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/siregularpl/{id}
        [HttpPost]
        public void UploadSIRegularPL([FromUri]int id)
        {
            var fileSavePath = "";

            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractSIPOSummaryAndCartonDetail(id, OrderType.Prepack);

            var killer = new ExcelKiller();

            killer.Dispose();
        }
    }
}
