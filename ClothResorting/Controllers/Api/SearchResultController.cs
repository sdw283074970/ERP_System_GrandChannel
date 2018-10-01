using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class SearchResultController : ApiController
    {
        private ApplicationDbContext _context;

        public SearchResultController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/searchresult/?container={container}&po={po}&style={style}&color={color}&customer={customer}&size={size}
        [HttpGet]
        public IHttpActionResult SearchPackingList([FromUri]string container, [FromUri]string po, [FromUri]string color, [FromUri]string style, [FromUri]string customer, [FromUri]string size)
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

            return Ok(Mapper.Map<IList<RegularCartonDetail>, IList<RegularCartonDetailDto>>(searchResults));
        }


        // POST /api/searchresult 需要DTO
        [HttpPost]
        public IHttpActionResult SearchByConditions([FromBody]string[] arr)
        {
            var result = new List<SearchResult>();
            var query = new List<CartonBreakDown>();

            //如果集装箱号为默认值，则返回所有库存不为0的breakdown结果，否则返回指定结果
            if (arr[0] == "default")
            {
                query = _context.CartonBreakDowns
                    .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                    .Where(c => c.AvailablePcs != 0)
                    .ToList();
            }
            else
            {
                query = _context.CartonBreakDowns
                    .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                    .Where(c => c.AvailablePcs != 0 
                        && c.PurchaseOrderSummary.PreReceiveOrder.ContainerNumber == arr[0])
                    .ToList();
            }

            //遍历搜索结果，将结果投射到searchresult模型中
            foreach (var q in query)
            {
                var c = new CartonBreakdownGetter(q);
                result.Add(new SearchResult
                {
                    ContainerNumber = c.GetContainerNumber(),
                    PurchaseOrder = c.GetPurchaseOrder(),
                    Vender = c.GetVendor(),
                    Style = c.GetStyle(),
                    Color = c.GetColor(),
                    CartonNumberRangeFrom = c.GetCartonNumberFrom(),
                    CartonNumberRangeTo = c.GetCartonNumberTo(),
                    RunCode = c.GetRunCode(),
                    Size = c.GetSize(),
                    ReceivedPcs = c.GetReceivedPcs(),
                    AvailablePcs = c.GetAvailablePcs(),
                    Location = c.GetLocation(),
                    RecievedDate = c.GetReceivedDate()
                });
            }

            //如果PO#为默认值，则返回结果不做任何改变，否则返回指定结果
            if(arr[1] != "default")
            {
                var po = arr[1];
                result = result.Where(r => r.PurchaseOrder == po).ToList();
            }

            //如果Vendor为默认值，则返回结果不做任何改变，否则返回指定结果
            if (arr[2] != "default")
            {
                var vendor = arr[2];
                result = result.Where(r => r.Vender == vendor).ToList();
            }

            //如果Style为默认值，则返回结果不做任何改变，否则返回指定结果
            if (arr[3] != "default")
            {
                var style = arr[3];
                result = result.Where(r => r.Style == style).ToList();
            }

            //如果Color为默认值，则返回结果不做任何改变，否则返回指定结果
            if (arr[4] != "default")
            {
                var color = arr[4];
                result = result.Where(r => r.Color == color).ToList();
            }

            //如果Size为默认值，则返回结果不做任何改变，否则返回指定结果
            if (arr[5] != "default")
            {
                var size = arr[5];
                result = result.Where(r => r.Size == size).ToList();
            }

            //uri临时设置一个，缺失DTO
            return Created(new Uri(Request.RequestUri + "/" + 1101), result);
        }
    }
}
