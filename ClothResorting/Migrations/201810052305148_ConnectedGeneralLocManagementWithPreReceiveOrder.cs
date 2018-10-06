namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedGeneralLocManagementWithPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GeneralLocationSummaries", "PreReceiveOrder_Id", c => c.Int());
            CreateIndex("dbo.GeneralLocationSummaries", "PreReceiveOrder_Id");
            AddForeignKey("dbo.GeneralLocationSummaries", "PreReceiveOrder_Id", "dbo.PreReceiveOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GeneralLocationSummaries", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropIndex("dbo.GeneralLocationSummaries", new[] { "PreReceiveOrder_Id" });
            DropColumn("dbo.GeneralLocationSummaries", "PreReceiveOrder_Id");
        }
    }
}
