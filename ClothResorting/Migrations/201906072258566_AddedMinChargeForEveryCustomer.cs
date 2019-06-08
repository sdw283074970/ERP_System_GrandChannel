namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMinChargeForEveryCustomer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "OutboundMinCharge", c => c.Single(nullable: false));
            AddColumn("dbo.UpperVendors", "InboundMinCharge", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpperVendors", "InboundMinCharge");
            DropColumn("dbo.UpperVendors", "OutboundMinCharge");
        }
    }
}
