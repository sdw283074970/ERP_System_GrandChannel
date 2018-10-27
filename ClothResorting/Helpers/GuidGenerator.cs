using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class GuidGenerator
    {
        internal static string GenerateGuid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}