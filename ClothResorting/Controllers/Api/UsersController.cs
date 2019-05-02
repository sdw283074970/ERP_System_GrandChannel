using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels.StaticModels;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class UsersController : ApiController
    {
        private ApplicationDbContext _context;

        public UsersController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/users/
        [HttpGet]
        public IHttpActionResult GetAllUsers()
        {
            var usersDto = _context.Users.Select(Mapper.Map<ApplicationUser, ApplicationUserDto>);

            return Ok(usersDto);
        }

        // GET /api/users/?userId={userId}
        [HttpGet]
        public IHttpActionResult GetUsersRoles([FromUri]string userId, [FromUri]string operation)
        {
            var roleIds = _context.Users.Find(userId).Roles.Select(x => x.RoleId).ToList();
            var rolesInDb = _context.Roles.ToList();
            var resultList = new List<IdentityRoleDto>();

            if (operation == FBAOperation.GetAddedRoles)
            {
                foreach (var i in rolesInDb)
                {
                    if (roleIds.Contains(i.Id))
                        resultList.Add(Mapper.Map<IdentityRole, IdentityRoleDto>(i));
                }
            }
            else if (operation == FBAOperation.GetAddableRoles)
            {
                foreach (var i in rolesInDb)
                {
                    if (!roleIds.Contains(i.Id))
                        resultList.Add(Mapper.Map<IdentityRole, IdentityRoleDto>(i));
                }
            }

            return Ok(resultList);
        }

        // POST /api/users/?userId={userId}&roleId={roleId}
        [HttpPost]
        public IHttpActionResult AssignRoleToUser([FromUri]string userId, [FromUri]string roleId)
        {
            var userInDb = _context.Users.Find(userId);
            var userRole = new IdentityUserRole {
                RoleId = roleId,
                UserId = userId
            };

            userInDb.Roles.Add(userRole);
            _context.SaveChanges();

            return Created(Request.RequestUri, "Role added successfully.");
        }

        // DELETE /api/users/api/users/?userId={userId}&roleId={roleId}
        [HttpDelete]
        public void RemoveRoleFromUser([FromUri]string userId, [FromUri]string roleId)
        {
            var userInDb = _context.Users.Find(userId);
            userInDb.Roles.Remove(userInDb.Roles.SingleOrDefault(x => x.RoleId == roleId));

            _context.SaveChanges();
        }
    }
}
