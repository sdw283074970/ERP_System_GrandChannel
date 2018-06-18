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
    public class NewBulkloadResultsController : ApiController
    {
        private ApplicationDbContext _context;

        public NewBulkloadResultsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/newbulkloadresults 按时间返回最新的添加记录，主要在添加完散货后返回给用户确认
        public IHttpActionResult GetMostRecentRecord()
        {
            var recentBulkloads = new List<RecentBulkload>();

            var query = _context.CartonBreakDowns
                .Include(c => c.CartonDetail)
                .OrderByDescending(c => c.Id)
                .ToList();

            var date = query.First().ReceivedDate;

            var results = query
                .OrderBy(c => c.Id)
                .Where(c => c.ReceivedDate == date);

            foreach (var r in results)
            {
                recentBulkloads.Add(new RecentBulkload
                {
                    PurchaseOrder = r.PurchaseOrder,
                    Style = r.Style,
                    Color = r.Color,
                    Size = r.Size,
                    NumberOfCartons = (int)r.CartonDetail.ActualReceived,
                    Pcs = (int)r.ActualPcs,
                    Location = r.Location,
                    InboundDate = (DateTime)r.ReceivedDate
                });
            }

            return Ok(recentBulkloads);
        }
    }
}
