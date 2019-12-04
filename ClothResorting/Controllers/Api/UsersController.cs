using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels.StaticModels;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using System.Threading;
using System.Security.Claims;

namespace ClothResorting.Controllers.Api
{
    [AllowAnonymous]
    public class UsersController : ApiController
    {
        private ApplicationDbContext _context;
        //private ApplicationSignInManager _signInManager;
        //private AuthenticationManager _authenticationManager;

        public UsersController()
        {
            _context = new ApplicationDbContext();
            //SignInManager = signInManager;
        }
        //private HttpContextBase HttpContextBase { get; }

        public IAuthenticationManager AuthenticationManager { 
            get 
            { 
                return HttpContext.Current.GetOwinContext().Authentication; 
            } 
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        // GET /api/users/?userName={userName}&password={password}
        [HttpGet]
        public async Task<IHttpActionResult> Login([FromUri]string userName, [FromUri]string password)
        {
            //var s = HttpContext.Current.GetOwinContext();
            var user = await UserManager.FindAsync(userName, password);
            //var user = new ApplicationUser() { UserName = userName };
            //var result = await UserManager.CreateAsync(user, password);

            //如果数据库存在这个用户，则为这个用户重新分配一个token
            //为了保证服务器不超载，这里的token不做保存，只做校验（等它自然过期）
            if (user != null)
            {
                //var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
                //identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                //AuthenticationManager.SignIn(identity);

                //var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                //AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                //await SignInAsync(user, false);
                //System.Diagnostics.Process.Start("https://localhost:44364/Account/Login");
                return Ok(new { Code = 20000, Token = "admin-token" });
            }
            else
            {
                return Ok(new { Code = 30000, Message = "RequiresVerification" });
            }

            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return Ok(new { Code = 20000, Token = "admin-token" });
            //    case SignInStatus.LockedOut:
            //        return Ok(new { Code = 30000, Message = "Lockout" });
            //    case SignInStatus.RequiresVerification:
            //        return Ok(new { Code = 30000, Message = "RequiresVerification" });
            //    case SignInStatus.Failure:
            //    default:
            //        return Ok(new { Code = 30000, Message = "Invalid login attempt" });
            //}
        }

        // GET /api/users/?token={token}
        [HttpGet]
        public IHttpActionResult GetUserInfo([FromUri]string token)
        {
            return Ok(new { 
                Code = 20000,
                Name = "Super Admin",
                Avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                Introduction = "I am a super administrator",
                Roles = new string[] { "admin"} });
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

        // POST /api/users/
        [HttpPost]
        public IHttpActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return Ok(new { Code = 20000, Data = "success" });
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

        private void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var claimsIdentity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, claimsIdentity);
            FormsAuthentication.SetAuthCookie(user.UserName, false);
            //var httpContext = HttpContext.Current;
            //var version = 1;
            //var name = user.UserName;
            //var now = DateTime.Now.ToLocalTime();
            //var expiration = now.Add(TimeSpan.FromDays(30));
            //var userData = JsonConvert.SerializeObject(user);
            //var ticket = new FormsAuthenticationTicket(version, name, now, expiration, isPersistent, userData, FormsAuthentication.FormsCookiePath);

            //var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            //var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            //{
            //    HttpOnly = true,
            //    Secure = FormsAuthentication.RequireSSL,
            //    Path = FormsAuthentication.FormsCookiePath
            //};
            //cookie.Expires = ticket.Expiration;
            //if (FormsAuthentication.CookieDomain != null)
            //{
            //    cookie.Domain = FormsAuthentication.CookieDomain;
            //}

            //var url = HttpContext.Current.Request.Url.ToString();
            //if (!string.IsNullOrEmpty(url) && url.StartsWith("https"))
            //{
            //    cookie.Secure = true;
            //}
            //httpContext.Response.Cookies.Add(cookie);

            //httpContext.User = new GenericPrincipal(new FormsIdentity(ticket), new string[] { "admin" });
        }
    }
}
