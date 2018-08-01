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

namespace ClothResorting.Controllers.Api
{
    public class PullSheetDiagnosticsController : ApiController
    {
        private ApplicationDbContext _context;

        public PullSheetDiagnosticsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pullsheetdiagnostics/{id}(pullSheetId)
        public IHttpActionResult GetRecords([FromUri]int id)
        {
            var result = _context.PullSheetDiagnostics
                .Include(x => x.PullSheet)
                .Where(x => x.PullSheet.Id == id)
                .Select(Mapper.Map<PullSheetDiagnostic, PullSheetDiagnosticDto>);

            return Ok(result);
        }

        // DELETE /api/pullsheetdiagnostics/{id}(pullSheetId)
        [HttpDelete]
        public void CleanAllRecord([FromUri]int id)
        {
            var diagnosticsInDb = _context.PullSheetDiagnostics
                .Include(x => x.PullSheet)
                .Where(x => x.PullSheet.Id == id);

            _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            _context.SaveChanges();
        }
    }
}
