namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedPermanentLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PermanentLocations", "Color", c => c.String());
            AddColumn("dbo.PermanentLocIORecords", "PurchaseOrder", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PermanentLocIORecords", "PurchaseOrder");
            DropColumn("dbo.PermanentLocations", "Color");
        }
    }
}
