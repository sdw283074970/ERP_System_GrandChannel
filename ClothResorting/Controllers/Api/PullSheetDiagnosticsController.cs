using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class PullSheetDiagnosticsController : ApiController
    {
        private ApplicationDbContext _context;

        public PullSheetDiagnosticsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pullsheetdiagnostics/?shipOrderId={shipOrderId}&shipOrderType={shipOrderType}
        [HttpGet]
        public IHttpActionResult GetRecords([FromUri]int shipOrderId, [FromUri]string shipOrderType)
        {
            if (shipOrderType == OrderType.Garment)
            {
                var result = _context.PullSheetDiagnostics
                    .Include(x => x.ShipOrder)
                    .Where(x => x.ShipOrder.Id == shipOrderId)
                    .Select(Mapper.Map<PullSheetDiagnostic, PullSheetDiagnosticDto>);

                return Ok(result);
            }
            else if (shipOrderType == OrderType.FBA)
            {
                var result = _context.PullSheetDiagnostics
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.Id == shipOrderId)
                    .Select(Mapper.Map<PullSheetDiagnostic, PullSheetDiagnosticDto>);

                return Ok(result);
            }

            return Ok();
        }

        // DELETE /api/pullsheetdiagnostics/?shipOrderId={shipOrderId}&shipOrderType={shipOrderType}
        [HttpDelete]
        public void CleanAllRecord([FromUri]int shipOrderId, [FromUri]string shipOrderType)
        {
            if (shipOrderType == OrderType.Garment)
            {
                var diagnosticsInDb = _context.PullSheetDiagnostics
                    .Include(x => x.ShipOrder)
                    .Where(x => x.ShipOrder.Id == shipOrderId);

                _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            }
            else if (shipOrderType == OrderType.FBA)
            {
                var diagnosticsInDb = _context.PullSheetDiagnostics
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.Id == shipOrderId);

                _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            }
            _context.SaveChanges();
        }
    }
}
