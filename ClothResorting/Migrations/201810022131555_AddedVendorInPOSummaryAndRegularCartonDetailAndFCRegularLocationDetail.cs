namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVendorInPOSummaryAndRegularCartonDetailAndFCRegularLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "Vendor", c => c.String());
            AddColumn("dbo.POSummaries", "Vendor", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Vendor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "Vendor");
            DropColumn("dbo.POSummaries", "Vendor");
            DropColumn("dbo.FCRegularLocationDetails", "Vendor");
        }
    }
}
