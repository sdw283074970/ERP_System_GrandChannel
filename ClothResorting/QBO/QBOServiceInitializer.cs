using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Intuit.Ipp.Core;
using Intuit.Ipp.Security;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace ClothResorting.QBO
{
    public class QBOServiceInitializer
    {
        private ApplicationDbContext _context;
        public QBOServiceInitializer()
        {
            _context = new ApplicationDbContext();
        }

        public ServiceContext InitializeQBOServiceContextUsingoAuth2()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId<string>();

            var oauthInfo = _context.Users
                .Include(x => x.OAuthInfo)
                .SingleOrDefault(x => x.Id == userId)
                .OAuthInfo
                .SingleOrDefault(x => x.PlatformName == Platform.QBO);

            var oauthValidator = new OAuth2RequestValidator(oauthInfo.AccessToken);

            ServiceContext context = new ServiceContext(oauthInfo.RealmId, IntuitServicesType.QBO, oauthValidator);

            //MinorVersion represents the latest features/fields in the xsd supported by the QBO apis.
            //Read more details here- https://developer.intuit.com/docs/0100_quickbooks_online/0200_dev_guides/accounting/querying_data

            context.IppConfiguration.MinorVersion.Qbo = "14";

            return context;
        }
    }
}