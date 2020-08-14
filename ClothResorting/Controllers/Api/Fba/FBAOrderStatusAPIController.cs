using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAOrderStatusAPIController : ApiController
    {
        private FBAexAPIValidator _validator;
        private ApplicationDbContext _context;

        public FBAOrderStatusAPIController()
        {
            _validator = new FBAexAPIValidator();
            _context = new ApplicationDbContext();
        }

        // POST /api/FBAOrderStatusAPI/?appKey=foo&customerCode=bar&requestId=foo&version=bar&sign=foo
        [HttpPost]
        public IHttpActionResult GetOrderStatus([FromUri]string appKey, [FromUri]string customerCode, [FromUri]string requestId, [FromUri]string version, [FromUri]string sign, [FromBody]OrderStatusQureyBody body)
        {
            // 验证签名
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            var jsonResult = _validator.ValidateSign(appKey, customerInDb, requestId, version, sign);

            if (jsonResult.Code != 200)
                return Ok(jsonResult);

            var qureyStauts = new List<QureyStatus>();

            if (body.OrderType == FBAOrderType.Inbound)
            {
                var qureyResults = QureyInboundOrders(body, out qureyStauts);
                jsonResult.QureyStatus = qureyStauts;
                jsonResult.QureyResults = new QureyResults { InboundOrders = qureyResults };
            }
            else if (body.OrderType == FBAOrderType.Outbound)
            {
                var qureyResults = QureyOutboundOrders(body, out qureyStauts);
                jsonResult.QureyStatus = qureyStauts;
                jsonResult.QureyResults = new QureyResults { OutboundOrders = qureyResults };
            }
            else
            {
                jsonResult.Message = "No operation applied.";
            }

            return Ok(jsonResult);
        }

        public IEnumerable<FBAMasterOrderDto> QureyInboundOrders(OrderStatusQureyBody qurey, out List<QureyStatus> qureyStatus)
        {
            var qureyStatusList = new List<QureyStatus>();

            var results = _context.FBAMasterOrders
                .Where(x => x.CreateDate >= qurey.FromDate && x.CreateDate <= qurey.ToDate)
                .Select(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>);

            if (qurey.Reference.Count() == 0)
            {
                qureyStatusList.Add(new QureyStatus { 
                    OrderType = "Inbound",
                    Reference = "NA",
                    Status = "Success",
                    Message = "No reference filter applied."
                });

                qureyStatus = qureyStatusList;

                if (qurey.Status == "" || qurey.Status == null || qurey.Status == "NA")
                {
                    return results;
                }
                else
                {
                    return results.Where(x => x.Status == qurey.Status);
                }
            }
            else
            {
                var qureyResults = new List<FBAMasterOrderDto>();

                foreach(var reference in qurey.Reference)
                {
                    var result = results.SingleOrDefault(x => x.Container == reference);

                    if (result == null)
                    {
                        qureyStatusList.Add(new QureyStatus
                        {
                            OrderType = "Inbound",
                            Reference = reference,
                            Status = "Failed",
                            Message = "Contianer: " + reference + " was not found in system."
                        });
                    }
                    else
                    {
                        qureyStatusList.Add(new QureyStatus
                        {
                            OrderType = "Inbound",
                            Reference = reference,
                            Status = "Success",
                            Message = "Container: " + reference + " existed in system."
                        });

                        qureyResults.Add(result);
                    }
                }

                qureyStatus = qureyStatusList;

                if (qurey.Status == "" || qurey.Status == null || qurey.Status == "NA")
                {
                    return qureyResults;
                }
                else
                {
                    return qureyResults.Where(x => x.Status == qurey.Status);
                }
            }
        }

        public IEnumerable<FBAShipOrderDto> QureyOutboundOrders(OrderStatusQureyBody qurey, out List<QureyStatus> qureyStatus)
        {
            var qureyStatusList = new List<QureyStatus>();

            var results = _context.FBAShipOrders
                .Where(x => x.CreateDate >= qurey.FromDate && x.CreateDate <= qurey.ToDate)
                .Select(Mapper.Map<FBAShipOrder, FBAShipOrderDto>);

            if (qurey.Reference.Count() == 0)
            {
                qureyStatusList.Add(new QureyStatus
                {
                    OrderType = "Outbound",
                    Reference = "NA",
                    Status = "Success",
                    Message = "No reference filter applied."
                });

                qureyStatus = qureyStatusList;

                if (qurey.Status == "" || qurey.Status == null || qurey.Status == "NA")
                {
                    return results;
                }
                else
                {
                    return results.Where(x => x.Status == qurey.Status);
                }
            }
            else
            {
                var qureyResults = new List<FBAShipOrderDto>();

                foreach (var reference in qurey.Reference)
                {
                    var result = results.SingleOrDefault(x => x.ShipOrderNumber == reference);

                    if (result == null)
                    {
                        qureyStatusList.Add(new QureyStatus
                        {
                            OrderType = "Outbound",
                            Reference = reference,
                            Status = "Failed",
                            Message = "Outbound order: " + reference + " was not found in system."
                        });
                    }
                    else
                    {
                        qureyStatusList.Add(new QureyStatus
                        {
                            OrderType = "Inbound",
                            Reference = reference,
                            Status = "Success",
                            Message = "Outbound order: " + reference + " existed in system."
                        });

                        qureyResults.Add(result);
                    }
                }

                qureyStatus = qureyStatusList;

                if (qurey.Status == "" || qurey.Status == null || qurey.Status == "NA")
                {
                    return qureyResults;
                }
                else
                {
                    return qureyResults.Where(x => x.Status == qurey.Status);
                }
            }
        }
    }

    public class OrderStatusQureyBody
    {
        [Required(ErrorMessage = "Order type is required.")]
        public string OrderType { get; set; }

        [Required(ErrorMessage = "Order type is required.")]
        public string[] Reference { get; set; }

        [Required(ErrorMessage = "From date is required.")]
        [RegularExpression(@"^\d{4}-\d{1,2}-\d{1,2}", ErrorMessage = "Must match DateTime format: yyyy-MM-dd")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required.")]
        [RegularExpression(@"^\d{4}-\d{1,2}-\d{1,2}", ErrorMessage = "Must match DateTime format: yyyy-MM-dd")]
        public DateTime ToDate { get; set; }

        public string Status { get; set; }
    }

    public class QureyStatus
    {
        public string OrderType { get; set; }

        public string Reference { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }
    }
}
