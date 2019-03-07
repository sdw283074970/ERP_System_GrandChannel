using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class CustomerManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public CustomerManagementController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        //GET /api/customermanagement/
        public IHttpActionResult GetAllCustomer()
        {
            return Ok(_context.UpperVendors.Select(Mapper.Map<UpperVendor, UpperVendorDto>));
        }

        [HttpPost]
        //POST /api/customer/?name={name}&customerCode={customerCode}&departmentCode={departmentCode}
        public IHttpActionResult CreateNewCustomer([FromUri]string name, [FromUri]string customerCode, [FromUri]string departmentCode, [FromUri]string firstAddressLine, [FromUri]string secondAddressLine, [FromUri]string telNumber, [FromUri]string emailAddress, [FromUri]string contactPerson)
        {
            var customer = new UpperVendor
            {
                CustomerCode = customerCode,
                DepartmentCode = departmentCode,
                Name = name,
                FirstAddressLine = firstAddressLine,
                SecondAddressLine = secondAddressLine,
                TelNumber = telNumber,
                EmailAddress = emailAddress,
                ContactPerson = contactPerson,
                Status = Status.Active
            };

            _context.UpperVendors.Add(customer);

            var generator = new ChargingItemGenerator();

            generator.GenerateChargingItems(_context, customer);

            _context.SaveChanges();

            var result = _context.UpperVendors.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + result.Id, Mapper.Map<UpperVendor, UpperVendorDto>(result));
        }

        [HttpDelete]
        //DELETE /api/customer/{id}
        public void DeleteCustomer([FromUri]int id)
        {
            var customerInDb = _context.UpperVendors
                .Include(x => x.ChargingItems)
                .SingleOrDefault(x => x.Id == id);

            try
            {
                _context.ChargingItems.RemoveRange(customerInDb.ChargingItems);
                _context.UpperVendors.Remove(customerInDb);
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new Exception("Cannot delete this customer because one or more work orders or ship orders rely on it. Make sure deleting all related work orders and ship orders before deleteing this customer.");
            }
        }
    }
}
