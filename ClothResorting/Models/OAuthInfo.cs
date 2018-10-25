using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class OAuthInfo
    {
        public int Id { get; set; }

        public string PlatformName { get; set; }

        public string AccessToken { get; set; }

        public string refreshToken { get; set; }

        public string RealmId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}