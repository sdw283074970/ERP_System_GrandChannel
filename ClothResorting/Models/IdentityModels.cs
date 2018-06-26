using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using ClothResorting.Models.Interface;

namespace ClothResorting.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<CartonDetail> CartonDetails { get; set; }
        public DbSet<PurchaseOrderSummary> PurchaseOrderSummaries { get; set; }
        public DbSet<PreReceiveOrder> PreReceiveOrders { get; set; }
        public DbSet<SizeRatio> SizeRatios { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<CartonBreakDown> CartonBreakDowns { get; set; }
        public DbSet<RetrievingRecord> RetrievingRecords { get; set; }
        public DbSet<LoadPlanRecord> LoadPlanRecords { get; set; }
        public DbSet<CartonBreakdownOutbound> CartonBreakdownOutbounds { get; set; }
        public DbSet<LocationDetail> LocationDetails { get; set; }
        public DbSet<RegularLocationDetail> RegularLocationDetails { get; set; }
        public DbSet<PermanentLocation> PermanentLocations { get; set; }
        public DbSet<PermanentLocIORecord> PermanentLocIORecord { get; set; }
        public DbSet<PurchaseOrderInventory> PurchaseOrderInventories { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}