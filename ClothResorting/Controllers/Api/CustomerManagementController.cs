using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public IHttpActionResult CreateNewCustomer([FromUri]string name, [FromUri]string customerCode, [FromUri]string departmentCode)
        {
            _context.UpperVendors.Add(new UpperVendor {
                CustomerCode = customerCode,
                DepartmentCode = departmentCode,
                Name = name
            });

            _context.SaveChanges();

            var result = _context.UpperVendors.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + result.Id, Mapper.Map<UpperVendor, UpperVendorDto>(result));
        }

        [HttpDelete]
        //DELETE /api/customer/{id}
        public void DeleteCustomer([FromUri]int id)
        {
            var customerInDb = _context.UpperVendors.Find(id);

            try
            {
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
