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
using System.Web;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class ReceivingReportController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public ReceivingReportController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /receivingreport/?preid={preId}&container={container}
        [HttpGet]
        public IHttpActionResult GetAllCartonDetails([FromUri]int preid, [FromUri]string container)
        {
            var cartonDetails = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preid
                    && c.POSummary.Container == container)
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>)
                .ToList();

            return Ok(cartonDetails);
        }

        // GET //api/receivingreportexcel/?&container={container}&operation={operation}
        [HttpGet]
        public IHttpActionResult DownloadPakcingList([FromUri]string container, [FromUri]string operation)
        {
            if (operation == "UnloadingList")
            {
                var generator = new ExcelGenerator(@"E:\Template\Prelocation-Template.xlsx");

                var fullPath = generator.GeneratePreallocatingReport(container);

                var downloader = new Downloader();

                downloader.DownloadByFullPath(fullPath);
            }
            else if (operation == "Label")
            {
                var generator = new PDFGenerator();

                generator.GenerateLabelPdf(container);
            }

            return Ok();
        }

        // GET /receivingreport/?cartonDetailId={cartonDetailId}
        [HttpGet]
        public IHttpActionResult GetPreLocations([FromUri]int cartonDetailId)
        {
            var locationStr = _context.RegularCartonDetails.Find(cartonDetailId).PreLocation;

            var parser = new StringParser();
            var list = parser.ParseStrToPreLoc(locationStr);

            return Ok(list);
        }

        // PUT /receivingreport/?cartonDetailId={cartonDetailId}
        [HttpPut]
        public void UpdateLocationStr([FromUri]int cartonDetailId, [FromBody]IEnumerable<PreLocation> objArray)
        {
            var parser = new StringParser();

            var str = parser.ParsePreLocToStr(objArray);

            var cartonDetailInDb = _context.RegularCartonDetails.Find(cartonDetailId);

            cartonDetailInDb.PreLocation = str;

            _context.SaveChanges();
        }

        // PUT /receivingreport/?preId={preId}&container={container}
        [HttpPut]
        public void UpdateComment([FromUri]int preId, [FromUri]string container,[FromBody]IEnumerable<PreInfo> objArray)
        {
            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId
                    && x.POSummary.Container == container);

            foreach(var o in objArray)
            {
                var detail = cartonDetailsInDb.SingleOrDefault(x => x.Id == o.Id);

                detail.Adjustor = _userName;
                detail.Comment = o.Comment;

                //当预分配字符串不包含冒号以及分号的时候，视为全部分配到一个库位，自动补上冒号和分号
                if (!o.PreLocation.Contains(":") && !o.PreLocation.Contains(";"))
                {
                    detail.PreLocation = o.PreLocation + ":" + detail.Cartons + ";";
                }
                //当预分配字符只包含分号时，视为意图全部分配到一个库位，自动在分号前插入冒号和全部箱数
                else if (o.PreLocation.Contains(";") && !o.PreLocation.Contains(":"))
                {
                    detail.PreLocation = o.PreLocation.Substring(0, o.PreLocation.Length - 1) + ":" + detail.Cartons + ";";
                }
                //当预分配字符结尾不是分号时，自动补上分号
                else if (o.PreLocation[o.PreLocation.Length - 1] != ';')
                {
                    detail.PreLocation = o.PreLocation + ";";
                }
                else
                {
                    detail.PreLocation = o.PreLocation;
                }
            }

            _context.SaveChanges();

            //var cartonDetailInDb = _context.RegularCartonDetails.Find(obj.Id);

            //cartonDetailInDb.Comment = obj.Comment;
            //cartonDetailInDb.Adjustor = _userName;

            //_context.SaveChanges();
        }
    }

    public class PreInfo
    {
        public int Id { get; set; }

        public string PreLocation { get; set; }

        public string Comment { get; set; }
    }
}
