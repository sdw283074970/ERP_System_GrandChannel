namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DisconnectedFCRegularLocationWithPrereceiveOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FCRegularLocations", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropIndex("dbo.FCRegularLocations", new[] { "PreReceiveOrder_Id" });
            DropColumn("dbo.FCRegularLocations", "PreReceiveOrder_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FCRegularLocations", "PreReceiveOrder_Id", c => c.Int());
            CreateIndex("dbo.FCRegularLocations", "PreReceiveOrder_Id");
            AddForeignKey("dbo.FCRegularLocations", "PreReceiveOrder_Id", "dbo.PreReceiveOrders", "Id");
        }
    }
}
