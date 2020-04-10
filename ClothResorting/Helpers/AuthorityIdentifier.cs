using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class AuthorityIdentifier
    {
        private ApplicationDbContext _context;

        public AuthorityIdentifier()
        {
            _context = new ApplicationDbContext();
        }

        public bool IsInRole(string userName, string roleName)
        {
            var userInDb = _context.Users
                .SingleOrDefault(x => x.UserName == userName);

            var roleId = _context.Roles.SingleOrDefault(x => x.Name == roleName).Id;

            var roleIdsUnderUser = _context.Users
                .Include(x => x.Roles)
                .SingleOrDefault(x => x.UserName == userName)
                .Roles.Select(x => x.RoleId).ToList();

            if (roleIdsUnderUser.Contains(roleId))
                return true;

            return false;
        }
    }
}