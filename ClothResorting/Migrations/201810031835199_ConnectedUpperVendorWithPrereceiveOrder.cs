namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedUpperVendorWithPrereceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "UpperVendor_Id", c => c.Int());
            CreateIndex("dbo.PreReceiveOrders", "UpperVendor_Id");
            AddForeignKey("dbo.PreReceiveOrders", "UpperVendor_Id", "dbo.UpperVendors", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PreReceiveOrders", "UpperVendor_Id", "dbo.UpperVendors");
            DropIndex("dbo.PreReceiveOrders", new[] { "UpperVendor_Id" });
            DropColumn("dbo.PreReceiveOrders", "UpperVendor_Id");
        }
    }
}
