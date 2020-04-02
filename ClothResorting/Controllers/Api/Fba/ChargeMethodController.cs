using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using Microsoft.Office.Interop.Excel;

namespace ClothResorting.Controllers.Api.Fba
{
    public class ChargeMethodController : ApiController
    {
        private FBADbContext _context;

        public ChargeMethodController()
        {
            _context = new FBADbContext();
        }

        // GET /api/fba/chargemethod/?templateId={templateId}
        [HttpGet]
        public IHttpActionResult GetAllChargeMethod([FromUri]int templateId)
        {
            var methodsDto = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Id == templateId)
                .OrderBy(x => x.From)
                .Select(Mapper.Map<ChargeMethod, ChargeMethodDto>);

            return Ok(methodsDto);
        }

        // POST /api/fba/chargemethod/?templateId={templateId}&from={from}&to={to}&fee={fee}
        [HttpPost]
        public IHttpActionResult CreateNewChargeMethod([FromUri]int templateId, [FromUri]int from, [FromUri]int to, [FromUri]double fee)
        {
            int duration = to - from + 1;
            var chargeTemplateInDb = _context.ChargeTemplates.Find(templateId);

            //try
            //{
            //    if (period.Split('-').Count() > 1)
            //    {
            //        weeks = int.Parse(period.Split('-')[1]) - int.Parse(period.Split('-')[0]);

            //    }
            //    else
            //    {
            //        weeks = int.Parse(period);
            //    }
            //}
            //catch(Exception e)
            //{
            //    throw new Exception("Period is invalid.");
            //}

            var newMethod = new ChargeMethod
            {
                Fee = fee,
                From = from,
                To = to,
                Duration = duration,
                ChargeTemplate = chargeTemplateInDb,
                TimeUnit = chargeTemplateInDb.TimeUnit,
                Currency = chargeTemplateInDb.Currency
            };

            _context.ChargeMethods.Add(newMethod);
            _context.SaveChanges();

            var sample = _context.ChargeMethods.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + sample.Id, Mapper.Map<ChargeMethod, ChargeMethodDto>(sample));
        }

        // PUT /api/fba/chargemethod/?methodId={methodId}&from={from}&to={to}&fee={fee}
        [HttpPut]
        public void UpdateChargeMethod([FromUri]int methodId, [FromUri]int from, [FromUri]int to, [FromUri]float fee)
        {
            var methodInDb = _context.ChargeMethods.Find(methodId);
            methodInDb.From = from;
            methodInDb.To = to;
            methodInDb.Fee = fee;

            _context.SaveChanges();
        }

        // DELETE /api/fba/chargemethod/?methodId={methodId}
        [HttpDelete]
        public void DeleteMethod([FromUri]int methodId)
        {
            _context.ChargeMethods.Remove(_context.ChargeMethods.Find(methodId));
            _context.SaveChanges();
        }
    }
}
