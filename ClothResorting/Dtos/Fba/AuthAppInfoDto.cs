using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class AuthAppInfoDto
    {
        public int Id { get; set; }

        public string AppName { get; set; }

        public string AppKey { get; set; }

        public string SecretKey { get; set; }

        public string Memo { get; set; }

        public DateTime CreateDate { get; set; }
    }
}