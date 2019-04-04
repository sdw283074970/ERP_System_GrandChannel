namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectDiagnosticWithFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PullSheetDiagnostics", "FBAShipOrder_Id", c => c.Int());
            CreateIndex("dbo.PullSheetDiagnostics", "FBAShipOrder_Id");
            AddForeignKey("dbo.PullSheetDiagnostics", "FBAShipOrder_Id", "dbo.FBAShipOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PullSheetDiagnostics", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropIndex("dbo.PullSheetDiagnostics", new[] { "FBAShipOrder_Id" });
            DropColumn("dbo.PullSheetDiagnostics", "FBAShipOrder_Id");
        }
    }
}
