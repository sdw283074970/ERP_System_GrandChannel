namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInvoiceStatusInFBAMasterOrderAnFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "InboundType", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "InvoiceStatus", c => c.String());
            AddColumn("dbo.FBAShipOrders", "InvoiceStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "InvoiceStatus");
            DropColumn("dbo.FBAMasterOrders", "InvoiceStatus");
            DropColumn("dbo.FBAMasterOrders", "InboundType");
        }
    }
}
