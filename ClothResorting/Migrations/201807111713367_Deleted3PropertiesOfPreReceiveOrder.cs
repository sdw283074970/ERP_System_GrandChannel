namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Deleted3PropertiesOfPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "ActualReceivedCtns", c => c.Int());
            DropColumn("dbo.PreReceiveOrders", "ActualReceived");
            DropColumn("dbo.PreReceiveOrders", "Available");
            DropColumn("dbo.PreReceiveOrders", "AvailablePcs");
            DropColumn("dbo.PreReceiveOrders", "InvPcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PreReceiveOrders", "InvPcs", c => c.Int(nullable: false));
            AddColumn("dbo.PreReceiveOrders", "AvailablePcs", c => c.Int());
            AddColumn("dbo.PreReceiveOrders", "Available", c => c.Int());
            AddColumn("dbo.PreReceiveOrders", "ActualReceived", c => c.Int());
            DropColumn("dbo.PreReceiveOrders", "ActualReceivedCtns");
        }
    }
}
