namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectInvoiceDetailWithFBAShipOrderAndFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "FBAShipOrder_Id", c => c.Int());
            AddColumn("dbo.InvoiceDetails", "FBAMasterOrder_Id", c => c.Int());
            CreateIndex("dbo.InvoiceDetails", "FBAShipOrder_Id");
            CreateIndex("dbo.InvoiceDetails", "FBAMasterOrder_Id");
            AddForeignKey("dbo.InvoiceDetails", "FBAShipOrder_Id", "dbo.FBAShipOrders", "Id");
            AddForeignKey("dbo.InvoiceDetails", "FBAMasterOrder_Id", "dbo.FBAMasterOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvoiceDetails", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropForeignKey("dbo.InvoiceDetails", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropIndex("dbo.InvoiceDetails", new[] { "FBAMasterOrder_Id" });
            DropIndex("dbo.InvoiceDetails", new[] { "FBAShipOrder_Id" });
            DropColumn("dbo.InvoiceDetails", "FBAMasterOrder_Id");
            DropColumn("dbo.InvoiceDetails", "FBAShipOrder_Id");
        }
    }
}
