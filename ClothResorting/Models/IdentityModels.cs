using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using ClothResorting.Models.Interface;
using ClothResorting.Models.FBAModels;
using System.Collections.Generic;

namespace ClothResorting.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ICollection<OAuthInfo> OAuthInfo { get; set; }
        public string CustomerCode { get; set; }
        public ICollection<UpperVendor> Vendors { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
        public DbSet<CartonBreakdownOutbound> CartonBreakdownOutbounds { get; set; }
        public DbSet<ReplenishmentLocationDetail> ReplenishmentLocationDetails { get; set; }
        public DbSet<PurchaseOrderInventory> PurchaseOrderInventories { get; set; }
        public DbSet<SpeciesInventory> SpeciesInventories { get; set; }
        public DbSet<AdjustmentRecord> AdjustmentRecords { get; set; }
        public DbSet<POSummary> POSummaries { get; set; }
        public DbSet<RegularCartonDetail> RegularCartonDetails { get; set; }
        public DbSet<FCRegularLocationDetail> FCRegularLocationDetails { get; set; }
        public DbSet<CartonInside> CartonInsides { get; set; }
        public DbSet<ShipOrder> ShipOrders { get; set; }
        public DbSet<PickDetail> PickDetails { get; set; }
        public DbSet<PullSheetDiagnostic> PullSheetDiagnostics { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<GeneralLocationSummary> GeneralLocationSummaries { get; set; }
        public DbSet<UpperVendor> UpperVendors { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<ChargingItem> ChargingItems { get; set; }
        public DbSet<OAuthInfo> OAuthInfo { get; set; }
        public DbSet<OutboundHistory> OutboundHistories { get; set; }

        public DbSet<PermanentSKU> PermanentSKUs { get; set; }

        //FBA
        public DbSet<FBAMasterOrder> FBAMasterOrders { get; set; }
        public DbSet<FBAOrderDetail> FBAOrderDetails { get; set; }
        public DbSet<FBAPallet> FBAPallets { get; set; }
        public DbSet<FBACartonLocation> FBACartonLocations { get; set; }
        public DbSet<FBAPalletLocation> FBAPalletLocations { get; set; }
        public DbSet<FBAShipOrder> FBAShipOrders { get; set; }
        public DbSet<FBAPickDetail> FBAPickDetails { get; set; }
        public DbSet<FBAAddressBook> FBAAddressBooks { get; set; }
        public DbSet<FBAPickDetailCarton> FBAPickDetailCartons { get; set; }
        public DbSet<ChargingItemDetail> ChargingItemDetails { get; set; }
        public DbSet<EFile> EFiles { get; set; }
        public DbSet<OperationLog> OperationLogs { get; set; }

        //General
        public DbSet<InstructionTemplate> InstructionTemplates { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //一对零关系插入‘零’对象时报错，显示无法插入，原因为无外键暴露。暂时用一对多关系顶替。
            //modelBuilder.Entity<FCRegularLocationDetail>()
            //    .HasOptional(c => c.PickingRecord)
            //    .WithRequired(c => c.FCRegularLocationDetail);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class FBADbContext : DbContext
    {
        public FBADbContext()
            : base("FBAConnection")
        {
        }

        public DbSet<ChargeTemplate> ChargeTemplates{ get; set; }
        public DbSet<ChargeMethod> ChargeMethods { get; set; }

        public static FBADbContext Create()
        {
            return new FBADbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}