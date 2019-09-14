namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHoldCtns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "HoldCtns", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBACartonLocations", "HoldCtns");
        }
    }
}
