namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVendorInSeveralModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpeciesInventories", "Vendor", c => c.String());
            AddColumn("dbo.ReplenishmentLocationDetails", "Vendor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReplenishmentLocationDetails", "Vendor");
            DropColumn("dbo.SpeciesInventories", "Vendor");
        }
    }
}
