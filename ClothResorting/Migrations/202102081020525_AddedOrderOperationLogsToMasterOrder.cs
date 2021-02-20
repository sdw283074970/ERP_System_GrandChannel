namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderOperationLogsToMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderOperationLogs", "FBAMasterOrder_Id", c => c.Int());
            CreateIndex("dbo.OrderOperationLogs", "FBAMasterOrder_Id");
            AddForeignKey("dbo.OrderOperationLogs", "FBAMasterOrder_Id", "dbo.FBAMasterOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderOperationLogs", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.OrderOperationLogs", new[] { "FBAMasterOrder_Id" });
            DropColumn("dbo.OrderOperationLogs", "FBAMasterOrder_Id");
        }
    }
}
