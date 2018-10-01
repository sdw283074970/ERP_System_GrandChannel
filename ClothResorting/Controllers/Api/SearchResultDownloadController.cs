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
    public class SearchResultDownloadController : ApiController
    {
        private ApplicationDbContext _context;

        public SearchResultDownloadController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/searchresultdownload/?container={container}&po={po}&style={style}&color={color}&customer={customer}&size={size}
        [HttpGet]
        public void DownloadPackingListSearchResult([FromUri]string container, [FromUri]string po, [FromUri]string color, [FromUri]string style, [FromUri]string customer, [FromUri]string size)
        {
            var searchResults = _context.RegularCartonDetails.Where(x => x.Id > 0).ToList();

            if (container != "NULL")
            {
                searchResults = searchResults.Where(x => x.Container == container).ToList();
            }

            if (po != "NULL")
            {
                searchResults = searchResults.Where(x => x.PurchaseOrder == po).ToList();
            }

            if (style != "NULL")
            {
                searchResults = searchResults.Where(x => x.Style == style).ToList();
            }

            if (color != "NULL")
            {
                searchResults = searchResults.Where(x => x.Color == color).ToList();
            }

            if (customer != "NULL")
            {
                searchResults = searchResults.Where(x => x.Customer == customer).ToList();
            }

            if (size != "NULL")
            {
                searchResults = searchResults.Where(x => x.SizeBundle == size).ToList();
            }

            var generator = new ExcelGenerator();

            generator.GenerateSearchResultsExcelFile(searchResults, container, po, style, color, customer, size);
        }
    }
}
