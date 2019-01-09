namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHowToDeliverInLocationModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "HowToDeliver", c => c.String());
            AddColumn("dbo.FBAPalletLocations", "HowToDeliver", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "HowToDeliver");
            DropColumn("dbo.FBACartonLocations", "HowToDeliver");
        }
    }
}
