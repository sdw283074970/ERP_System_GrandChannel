namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedMeasurementModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Measurements", "PurchaseOrderNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Measurements", "PurchaseOrderNumber");
        }
    }
}
