using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class AuthAppInfo
    {
        public int Id { get; set; }

        public string AppName { get; set; }

        public string AppKey { get; set; }

        public string SecretKey { get; set; }

        public string Memo { get; set; }

        public DateTime CreateDate { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public AuthAppInfo()
        {
            CreateDate = DateTime.Now;
        }

        public AuthAppInfo Create(ApplicationUser user)
        {
            return new AuthAppInfo { 
                AppKey = Guid.NewGuid().ToString("N"),
                SecretKey = Guid.NewGuid().ToString("N"),
                ApplicationUser = user
            };
        }
    }
}