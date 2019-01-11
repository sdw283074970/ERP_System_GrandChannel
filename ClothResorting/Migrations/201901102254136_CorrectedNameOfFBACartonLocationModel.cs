namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectedNameOfFBACartonLocationModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "AvailableCtns", c => c.Int(nullable: false));
            DropColumn("dbo.FBACartonLocations", "AvaliableCtns");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBACartonLocations", "AvaliableCtns", c => c.Int(nullable: false));
            DropColumn("dbo.FBACartonLocations", "AvailableCtns");
        }
    }
}
