namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedBaseFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "ETA", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Carrier", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Vessel", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Voy", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "ETD", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "ETAPort", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "PlaceOfReceipt", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "PortOfLoading", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "PortOfDischarge", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "PlaceOfDelivery", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Container", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "SealNumber", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "ContainerSize", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "ContainerSize");
            DropColumn("dbo.FBAMasterOrders", "SealNumber");
            DropColumn("dbo.FBAMasterOrders", "Container");
            DropColumn("dbo.FBAMasterOrders", "PlaceOfDelivery");
            DropColumn("dbo.FBAMasterOrders", "PortOfDischarge");
            DropColumn("dbo.FBAMasterOrders", "PortOfLoading");
            DropColumn("dbo.FBAMasterOrders", "PlaceOfReceipt");
            DropColumn("dbo.FBAMasterOrders", "ETAPort");
            DropColumn("dbo.FBAMasterOrders", "ETD");
            DropColumn("dbo.FBAMasterOrders", "Voy");
            DropColumn("dbo.FBAMasterOrders", "Vessel");
            DropColumn("dbo.FBAMasterOrders", "Carrier");
            DropColumn("dbo.FBAMasterOrders", "ETA");
        }
    }
}
