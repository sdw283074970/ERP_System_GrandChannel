using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Threading.Tasks;
using ClothResorting.Manager;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBADeleteAPIController : ApiController
    {
        private FBAexAPIValidator _validator;
        private ApplicationDbContext _context;
        private FBAShipOrderController _soController;
        private FBAMasterOrderController _moController;
        private CustomerCallbackManager _callbackManager;

        public FBADeleteAPIController()
        {
            _validator = new FBAexAPIValidator();
            _context = new ApplicationDbContext();
            _soController = new FBAShipOrderController();
            _moController = new FBAMasterOrderController();
            _callbackManager = new CustomerCallbackManager(_context);
        }

        // DELETE /api/FBADeleteAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteOrder([FromUri] string appKey, [FromUri] string customerCode, [FromUri] string requestId, [FromUri] string version, [FromUri] string sign, [FromBody] DeleteRequestBody body)
        {
            // 验证签名
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            var jsonResult = _validator.ValidateSign(appKey, customerInDb, requestId, version, sign);

            if (jsonResult.Code != 200)
                return Json(jsonResult);

            if (body.OrderType == "Inbound")
            {
                var inboundOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == body.Reference && x.Status == FBAStatus.Draft);

                if (inboundOrderInDb != null)
                {
                    _moController.DeleteMasterOrderById(inboundOrderInDb.Id);
                }
                else
                    return Json(new { Code = "404", Message = "Cannot find inbound order# " + body.Reference + " or its stauts is not 'Draft'." });
            }
            else if (body.OrderType == "Outbound")
            {
                var outboundOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == body.Reference && x.Status == FBAStatus.Draft);

                if (outboundOrderInDb != null)
                {
                    await _soController.DeleteShipOrder(outboundOrderInDb.Id);
                    _callbackManager.CallBackWhenOutboundOrderCancelled(outboundOrderInDb);
                }
                else
                    return Json(new { Code = "404", Message = "Cannot find outbound order# " + body.Reference + " or its stauts is not 'Draft'." });
            }

            return Json(new { Code = "200", Message = "Delete Success!"});
        }
    }

    public class DeleteRequestBody
    {
        [Required(ErrorMessage = "Order type is required.")]
        public string OrderType { get; set; }

        [Required(ErrorMessage = "Order reference is required.")]
        public string Reference { get; set; }
    }
}
