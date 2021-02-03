namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLocationStatusToInventoryRelatedModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "LocationStatus", c => c.String());
            AddColumn("dbo.FBAPallets", "LocationStatus", c => c.String());
            AddColumn("dbo.FBAPalletLocations", "LocationStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "LocationStatus");
            DropColumn("dbo.FBAPallets", "LocationStatus");
            DropColumn("dbo.FBACartonLocations", "LocationStatus");
        }
    }
}
