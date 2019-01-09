namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedFBACartonLocationModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "GrossWeightPerCtn", c => c.Single(nullable: false));
            AddColumn("dbo.FBACartonLocations", "CBMPerCtn", c => c.Single(nullable: false));
            AddColumn("dbo.FBACartonLocations", "CtnsPerPlt", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBACartonLocations", "CtnsPerPlt");
            DropColumn("dbo.FBACartonLocations", "CBMPerCtn");
            DropColumn("dbo.FBACartonLocations", "GrossWeightPerCtn");
        }
    }
}
