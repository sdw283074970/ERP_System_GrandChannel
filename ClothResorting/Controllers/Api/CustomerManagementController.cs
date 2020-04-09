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
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using System.Web;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Controllers.Api
{
    public class CustomerManagementController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public CustomerManagementController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser").Split('@')[0]) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        [HttpGet]
        //GET /api/customermanagement/
        public IHttpActionResult GetAllCustomer()
        {
            return Ok(_context.UpperVendors.Select(Mapper.Map<UpperVendor, UpperVendorDto>));
        }

        //GET /api/customermanagement/?customerId={customerId}
        [HttpGet]
        public IHttpActionResult GetWOTemplateByCustomerId([FromUri]int customerId)
        {
            var woDto = _context.InstructionTemplates
                .Include(x => x.Customer)
                .Where(x => x.Customer.Id == customerId)
                .Select(Mapper.Map<InstructionTemplate, InstructionTemplateDto>);

            return Ok(woDto);
        }

        // GET /api/customermanagement/?templateId={templateId}
        [HttpGet]
        public IHttpActionResult GetTemplate([FromUri]int templateId)
        {
            return Ok(Mapper.Map<InstructionTemplate, InstructionTemplateDto>(_context.InstructionTemplates.Find(templateId)));
        }

        [HttpPost]
        //POST /api/customermanagement/?name={name}&customerCode={customerCode}&departmentCode={departmentCode}&warningQuantityLevel={warningQuantityLevel}
        public IHttpActionResult CreateNewCustomer([FromUri]string name, [FromUri]string customerCode, [FromUri]string departmentCode, [FromUri]string firstAddressLine, [FromUri]string secondAddressLine, [FromUri]string telNumber, [FromUri]string emailAddress, [FromUri]string contactPerson, [FromUri]int warningQuantityLevel)
        {
            if (_context.UpperVendors.Where(x => x.CustomerCode == customerCode).Count() != 0)
            {
                throw new Exception("Customer Code " + customerCode + " has been taken. Please try another one.");
            }

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
                Status = Status.Active,
                WarningQuantityLevel = warningQuantityLevel
            };

            _context.UpperVendors.Add(customer);

            //暂时停止为新客户自动建立收费项目
            //var generator = new ChargingItemGenerator();
            //generator.GenerateChargingItems(_context, customer);

            _context.SaveChanges();

            var result = _context.UpperVendors.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + result.Id, Mapper.Map<UpperVendor, UpperVendorDto>(result));
        }

        [HttpPost]
        //POST /api/customermanagement/
        public IHttpActionResult CreateNewCustomerByModel([FromBody]UpperVendorDto model)
        {
            if (_context.UpperVendors.Where(x => x.CustomerCode == model.CustomerCode).Count() != 0)
            {
                throw new Exception("Customer Code " + model.CustomerCode + " has been taken. Please try another one.");
            }

            var customer = new UpperVendor
            {
                CustomerCode = model.CustomerCode,
                DepartmentCode = model.DepartmentCode,
                Name = model.Name,
                FirstAddressLine = model.FirstAddressLine,
                SecondAddressLine = model.SecondAddressLine,
                TelNumber = model.TelNumber,
                EmailAddress = model.EmailAddress,
                ContactPerson = model.ContactPerson,
                Status = Status.Active,
                WarningQuantityLevel = model.WarningQuantityLevel
            };

            _context.UpperVendors.Add(customer);

            //暂时停止为新客户自动建立收费项目
            //var generator = new ChargingItemGenerator();
            //generator.GenerateChargingItems(_context, customer);

            _context.SaveChanges();

            var result = _context.UpperVendors.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + result.Id, Mapper.Map<UpperVendor, UpperVendorDto>(result));
        }

        //POST /api/customermanagement/?customerId={customerId}&description={description}&isChargingItem={isChargingItem}&isAppliedToAll={isAppliedToAll}&isApplyToShipOrder={isApplyToShipOrder}&isApplyToMasterOrder={isApplyToMasterOrder}
        [HttpPost]
        public IHttpActionResult CreateNewChargingDetailTemplate([FromUri]int customerId, [FromUri]string description, [FromUri]bool isChargingItem, [FromUri]bool isAppliedToAll, [FromUri]bool isApplyToShipOrder, [FromUri]bool isApplyToMasterOrder)
        {
            var fbaCustomers = _context.UpperVendors
                .Where(x => x.DepartmentCode == "FBA");

            if (!isAppliedToAll)
            {
                var newTemplate = new InstructionTemplate
                {
                    IsApplyToMasterOrder = isApplyToMasterOrder,
                    IsApplyToShipOrder = isApplyToShipOrder,
                    CreateBy = _userName,
                    Description = description,
                    CreateDate = DateTime.Now
                };

                var customerInDb = fbaCustomers.SingleOrDefault(x => x.Id == customerId);
                newTemplate.Customer = customerInDb;

                if (isChargingItem)
                {
                    newTemplate.Status = FBAStatus.WaitingForCharging;
                }
                else
                {
                    newTemplate.Status = FBAStatus.NoNeedForCharging;
                }

                _context.InstructionTemplates.Add(newTemplate);
            }
            else
            {
                var templateList = new List<InstructionTemplate>();

                foreach(var f in fbaCustomers)
                {
                    var newTemplate = new InstructionTemplate
                    {
                        IsApplyToMasterOrder = isApplyToMasterOrder,
                        IsApplyToShipOrder = isApplyToShipOrder,
                        CreateBy = _userName,
                        Description = description,
                        CreateDate = DateTime.Now
                    };

                    newTemplate.Customer = f;

                    if (isChargingItem)
                    {
                        newTemplate.Status = FBAStatus.WaitingForCharging;
                    }
                    else
                    {
                        newTemplate.Status = FBAStatus.NoNeedForCharging;
                    }

                    templateList.Add(newTemplate);
                }

                _context.InstructionTemplates.AddRange(templateList);
            }

            _context.SaveChanges();

            var resultDto = Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(_context.ChargingItemDetails.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        //POST /api/customermanagement/?customerId={customerId}
        [HttpPost]
        public IHttpActionResult CreateNewChargingDetailTemplateByModel([FromUri]int customerId, [FromBody]InstructionTemplateDto template)
        {
            var fbaCustomers = _context.UpperVendors
                .Where(x => x.DepartmentCode == "FBA");

            if (!template.IsApplyToAll)
            {
                var newTemplate = new InstructionTemplate
                {
                    IsApplyToMasterOrder = template.IsApplyToMasterOrder,
                    IsApplyToShipOrder = template.IsApplyToShipOrder,
                    IsInstruction = template.IsInstruction,
                    IsOperation = template.IsOperation,
                    IsCharging = template.IsCharging,
                    CreateBy = _userName,
                    Description = template.Description,
                    CreateDate = DateTime.Now,
                    Status = template.IsCharging ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging
                };

                var customerInDb = fbaCustomers.SingleOrDefault(x => x.Id == customerId);
                newTemplate.Customer = customerInDb;

                _context.InstructionTemplates.Add(newTemplate);
            }
            else
            {
                var templateList = new List<InstructionTemplate>();

                foreach (var f in fbaCustomers)
                {
                    var newTemplate = new InstructionTemplate
                    {
                        IsApplyToMasterOrder = template.IsApplyToMasterOrder,
                        IsApplyToShipOrder = template.IsApplyToShipOrder,
                        IsInstruction = template.IsInstruction,
                        IsOperation = template.IsOperation,
                        IsCharging = template.IsCharging,
                        CreateBy = _userName,
                        Description = template.Description,
                        CreateDate = DateTime.Now,
                        Status = template.IsCharging ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging
                    };

                    newTemplate.Customer = f;
                    templateList.Add(newTemplate);
                }

                _context.InstructionTemplates.AddRange(templateList);
            }

            _context.SaveChanges();

            var resultDto = Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(_context.ChargingItemDetails.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        //PUT /api/customermanagement/?warningQuantityLevel={warningQuantityLevel}
        [HttpPut]
        public void UpdateCustomer([FromUri]int customerId, [FromUri]string firstAddressLine, [FromUri]string secondAddressLine, [FromUri]string telNumber, [FromUri]string emailAddress, [FromUri]string contactPerson, [FromUri]int warningQuantityLevel)
        {
            var customerInDb = _context.UpperVendors.Find(customerId);

            customerInDb.FirstAddressLine = firstAddressLine;
            customerInDb.SecondAddressLine = secondAddressLine;
            customerInDb.TelNumber = telNumber;
            customerInDb.EmailAddress = emailAddress;
            customerInDb.ContactPerson = contactPerson;
            customerInDb.WarningQuantityLevel = warningQuantityLevel;

            _context.SaveChanges();
        }

        // PUT /api/customermanagement/?customerId={customerId}&userAccount={userAccount}
        [HttpPut]
        public void LinkToUser([FromUri]int customerId, [FromUri]string userAccount)
        {
            var userInDb = _context.Users.SingleOrDefault(x => x.UserName == userAccount);
            var customerInDb = _context.UpperVendors
                .Include(x => x.ApplicationUser)
                .Single(x => x.Id == customerId);

            if (userAccount == "DISMISS")
                customerInDb.ApplicationUser = null;
            else if (userInDb == null)
                throw new Exception("User account '" + userAccount + "'was not found in system.");
            else
                customerInDb.ApplicationUser = userInDb;

            _context.SaveChanges();
        }

        //PUT /api/customermanagement/
        [HttpPut]
        public void UpdateCustomerByModel([FromBody]UpperVendor model)
        {
            var customerInDb = _context.UpperVendors.Find(model.Id);

            customerInDb.FirstAddressLine = model.FirstAddressLine;
            customerInDb.SecondAddressLine = model.SecondAddressLine;
            customerInDb.TelNumber = model.TelNumber;
            customerInDb.EmailAddress = model.EmailAddress;
            customerInDb.ContactPerson = model.ContactPerson;
            customerInDb.WarningQuantityLevel = model.WarningQuantityLevel;

            _context.SaveChanges();
        }

        // PUT /api/customermanagement/?templateId={templateId}
        [HttpPut]
        public void UpdateInstructionTemplate([FromUri]int templateId, [FromBody]InstructionTemplateDto template)
        {
            var templateInDb = _context.InstructionTemplates.Find(templateId);

            templateInDb.Description = template.Description;
            templateInDb.IsInstruction = template.IsInstruction;
            templateInDb.IsOperation = template.IsOperation;
            templateInDb.IsCharging = template.IsCharging;
            templateInDb.IsApplyToMasterOrder = template.IsApplyToMasterOrder;
            templateInDb.IsApplyToShipOrder = template.IsApplyToShipOrder;
            templateInDb.Status = template.IsCharging ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging;

            _context.SaveChanges();
        }

        [HttpDelete]
        //DELETE /api/customermanagement/{id}
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

        //DELET /api/customermanagement/?instructionId={instructionId}
        [HttpDelete]
        public void DeleteInstructionTemplate([FromUri]int instructionId)
        {
            var instructionInDb = _context.InstructionTemplates.Find(instructionId);
            _context.InstructionTemplates.Remove(instructionInDb);
            _context.SaveChanges();
        }
    }
}
