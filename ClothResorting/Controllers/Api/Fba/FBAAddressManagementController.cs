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
    public class FBAAddressManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAAddressManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbaaddressmanagement/
        [HttpGet]
        public IHttpActionResult GetAllAddress()
        {
            return Ok(Mapper.Map<IEnumerable<FBAAddressBook>, IEnumerable<FBAAddressBookDto>>(_context.FBAAddressBooks.ToList()));
        }

        // GET /api/fba/fbaaddressmanagement/{id}
        [HttpGet]
        public IHttpActionResult GetAddress([FromUri]int id)
        {
            return Ok(Mapper.Map<FBAAddressBook, FBAAddressBookDto>(_context.FBAAddressBooks.Find(id)));
        }

        // POST /api/fba/fbaaddressmanagement/
        [HttpPost]
        public IHttpActionResult CreateNewAddressBook([FromBody]AddressBookDto obj)
        {
            if (_context.FBAAddressBooks.Where(x => x.WarehouseCode == obj.AddressCode).Count() != 0)
            {
                throw new Exception("Code " + obj.AddressCode + " has been taken. Please try another one.");
            }

            var address = new FBAAddressBook
            {
                WarehouseCode = obj.AddressCode,
                Address = obj.AddressDetail,
                Memo = obj.Memo
            };

            _context.FBAAddressBooks.Add(address);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAAddressBook, FBAAddressBookDto>(_context.FBAAddressBooks.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // PUT /api/fba/fbaaddressmanagement/{id}
        [HttpPut]
        public void UpdateAddress([FromUri]int id, [FromBody]AddressBookDto obj)
        {
            var bookInDb = _context.FBAAddressBooks.Find(id);

            //如果当前要修改对象的CODE与数据库的不同且其他对象有相同的CODE，说明CODE已经被其他对象使用了，返回异常
            if (bookInDb.WarehouseCode != obj.AddressCode && _context.FBAAddressBooks.Where(x => x.WarehouseCode == obj.AddressCode).Count() > 0)
            {
                throw new Exception("Code " + obj.AddressCode + " has been taken. Please try another one.");
            }

            bookInDb.WarehouseCode = obj.AddressCode;
            bookInDb.Address = obj.AddressDetail;
            bookInDb.Memo = obj.Memo;

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbaaddressmanagement/{id}
        [HttpDelete]
        public void DeleteAddreee([FromUri]int id)
        {
            var bookInDb = _context.FBAAddressBooks.Find(id);

            _context.FBAAddressBooks.Remove(bookInDb);
            _context.SaveChanges();
        }
    }
    
    public class AddressBookDto
    {
        public string AddressCode { get; set; }

        public string AddressDetail { get; set; }

        public string Memo { get; set; }
    }
}
