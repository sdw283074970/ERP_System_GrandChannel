namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedChargingDetailIntoUpperVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "Customer_Id", c => c.Int());
            CreateIndex("dbo.ChargingItemDetails", "Customer_Id");
            AddForeignKey("dbo.ChargingItemDetails", "Customer_Id", "dbo.UpperVendors", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChargingItemDetails", "Customer_Id", "dbo.UpperVendors");
            DropIndex("dbo.ChargingItemDetails", new[] { "Customer_Id" });
            DropColumn("dbo.ChargingItemDetails", "Customer_Id");
        }
    }
}
