using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class NewBulkloadSingleReceivingController : ApiController
    {
        private ApplicationDbContext _context;

        public NewBulkloadSingleReceivingController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/NewBulkloadSingleReceiving
        [HttpPost]
        public IHttpActionResult CreateNewSingleBulkload([FromBody]CartonBreakdownRequestJsonObj obj)
        {
            var result = _context.CartonBreakDowns
                .Add(new CartonBreakDown {
                    PurchaseOrder = obj.PurchaseOrder,
                    Style = obj.Style,
                    Color = obj.Color,
                    Size = obj.Size,
                    ActualPcs = obj.GrandTotal,
                    Location = obj.Location,
                    AvailablePcs = obj.GrandTotal
                });

            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + result.Id, result);
        }
    }
}
