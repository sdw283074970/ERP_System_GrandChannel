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
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using System;

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
                var userInDb = _context.Users.Single(x => x.UserName == userName);
                userInDb.LatestLogin = DateTime.Now;
                _context.SaveChanges();
                return Ok(new { Code = 20000, Token = user.Id });
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
            var user = _context.Users
                .Include(x => x.Roles)
                .Include(x => x.Vendors)
                .SingleOrDefault(x => x.Id == token);

            var roles = GetUserRoles(user);
            var customerCode = "";

            //检查是不是Customer账户，并检查是否这个customer账户拥有一个vendor账户
            if (roles.First() == "customer")
            {
                var vendors = _context.UpperVendors
                    .Include(x => x.ApplicationUser)
                    .Where(x => x.Status == Status.Active 
                        && x.DepartmentCode == DepartmentCode.FBA 
                        && x.ApplicationUser.Id == token);

                //虽然理论上一个账户可以控制多个vendor账户，但是为了设计简单且轻量，强制人为规定一个账户只能有用一个vendor账户，多了就报错
                if (vendors != null)
                {
                    if (vendors.Count() > 1)
                        throw new System.Exception("检测到该账户上有多个Vendor账户，请联系技术人员解决");
                    else
                        customerCode = vendors.First().CustomerCode;
                }
            }

            return Ok(new
            {
                Code = 20000,
                Name = user.Email,
                CustomerCode = customerCode,
                Avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                Introduction = "No introduction",
                Roles = roles
            });
        }

        // GET /api/users/
        [HttpGet]
        public IHttpActionResult GetAllUsers()
        {
            //var usersDto = _context.Users.Select(Mapper.Map<ApplicationUser, ApplicationUserDto>);
            var usersInDb = _context.Users
                .Include(x => x.Roles)
                .Include(x => x.Vendors)
                .Where(x => x.Id != null);

            var roleList = _context.Roles.Where(x => x.Id != null).ToList();
            var userDto = new List<ApplicationUserDto>();

            foreach(var u in usersInDb)
            {
                var roleNames = new List<string>();
                var dto = new ApplicationUserDto();

                dto.UserName = u.UserName;
                dto.Email = u.Email;
                dto.LatestLogin = u.LatestLogin;

                foreach(var r in u.Roles)
                {
                    roleNames.Add(TranslateRoleName(roleList.SingleOrDefault(x => x.Id == r.RoleId).Name));
                }

                dto.Roles = roleNames.ToArray();
                dto.CustomerCodes = u.Vendors.Select(x => x.CustomerCode).ToArray();
                userDto.Add(dto);
            }

            return Ok(userDto);
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
            return Ok(new { Code = 20000, Data = "Sing out success" });
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

        private string[] GetUserRoles(ApplicationUser user)
        {
            if (user.Roles.SingleOrDefault(x => x.RoleId == "4eb2760d-9486-490f-a95a-d7e99ac1257b") != null)
            {
                return new string[] { "customer" };
            }
            else if (user.Roles.SingleOrDefault(x => x.RoleId == "fbbe09ed-8afe-430f-bd59-6295afa6c476") != null)
            {
                return new string[] { "admin" };
            }
            else if (user.Roles.SingleOrDefault(x => x.RoleId == "a9751d35-1f00-42d0-8ca3-ba0e8d7ab6cb") != null)
            {
                return new string[] { "accounting" };
            }
            else if (user.Roles.SingleOrDefault(x => x.RoleId == "d1934a07-751a-4387-9284-9be502ad4617") != null)
            {
                return new string[] { "sales" };
            }
            else if (user.Roles.SingleOrDefault(x => x.RoleId == "f6e3dfd7-9a23-4825-a668-d4cbb8bbd64a") != null)
            {
                return new string[] { "office" };
            }
            else if (user.Roles.SingleOrDefault(x => x.RoleId == "1dd0bf46-2297-496c-8e30-1e50befdcbf0") != null)
            {
                return new string[] { "warehouse" };
            }
            else
            {
                return new string[] { "guest" };
            }
        }

        private string TranslateRoleName(string name)
        {
            switch(name)
            {
                case "CanDeleteEverything":
                    return "admin";
                case "CanViewAsClientOnly": 
                    return "customer";
                case "CanOperateAsT5":
                    return "accounting";
                case "CanOperateAsT4":
                    return "sales";
                case "CanOperateAsT3":
                    return "office";
                case "CanOperateAsT2":
                    return "warehouse";
                case "CanOperateAsT1":
                    return "guest";
                case null:
                    return "guest";
                default:
                    return "guest";
            }
        }
    }
}
