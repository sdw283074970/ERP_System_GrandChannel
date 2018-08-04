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
    public class PullSheetPdfController : ApiController
    {
        private ApplicationDbContext _context;

        public PullSheetPdfController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pullsheetpdf/{id}{pullsheetId}
        [HttpGet]
        public IHttpActionResult Print([FromUri]int id)
        {
            var generator = new PDFGenerator();

            generator.GeneratePickDetailPdf(id);

            return Ok();
        }
    }
}
