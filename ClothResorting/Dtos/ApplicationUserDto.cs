using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }

        public string Email { get; set; }
        public DateTime LatestLogin { get; set; }

        public string UserName { get; set; }

        public string[] Roles { get; set; }

        public string[] CustomerCodes { get; set; }
    }
}