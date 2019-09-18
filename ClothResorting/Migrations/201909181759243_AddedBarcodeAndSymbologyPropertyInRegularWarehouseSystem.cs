namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBarcodeAndSymbologyPropertyInRegularWarehouseSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAOrderDetails", "Barcode", c => c.String());
            AddColumn("dbo.FBAOrderDetails", "Symbology", c => c.String());
            AddColumn("dbo.FBAPickDetails", "Barcode", c => c.String());
            AddColumn("dbo.FBAPickDetails", "Symbology", c => c.String());
            DropColumn("dbo.FBACartonLocations", "UPCNumber");
            DropColumn("dbo.FBAOrderDetails", "UPCNumber");
            DropColumn("dbo.FBAPickDetails", "UPCNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAPickDetails", "UPCNumber", c => c.String());
            AddColumn("dbo.FBAOrderDetails", "UPCNumber", c => c.String());
            AddColumn("dbo.FBACartonLocations", "UPCNumber", c => c.String());
            DropColumn("dbo.FBAPickDetails", "Symbology");
            DropColumn("dbo.FBAPickDetails", "Barcode");
            DropColumn("dbo.FBAOrderDetails", "Symbology");
            DropColumn("dbo.FBAOrderDetails", "Barcode");
        }
    }
}
