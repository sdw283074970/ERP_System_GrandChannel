using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class ChargeTemplateController : ApiController
    {
        private FBADbContext _context;

        public ChargeTemplateController()
        {
            _context = new FBADbContext();
        }

        // GET /api/fba/chargetemplate/
        [HttpGet]
        public IHttpActionResult GetAllTemplates()
        {
            var templatesDto = _context.ChargeTemplates
                .Where(x => x.Id > 0)
                .Select(Mapper.Map<ChargeTemplate, ChargeTemplateDto>);

            return Ok(templatesDto);
        }

        // POST /api/fba/chargetemplate/?templateName={templateName}&customer={customer}
        [HttpPost]
        public IHttpActionResult CreateNewTemplate([FromUri]string templateName, [FromUri]string customer)
        {
            var newTemplate = new ChargeTemplate {
                TemplateName = templateName,
                Customer = customer
            };

            _context.ChargeTemplates.Add(newTemplate);
            _context.SaveChanges();

            var sample = _context.ChargeTemplates
                .OrderByDescending(x => x.Id)
                .First();

            return Created(Request.RequestUri + "/" + sample.Id, Mapper.Map<ChargeTemplate, ChargeTemplateDto>(sample));
        }
    }
}
