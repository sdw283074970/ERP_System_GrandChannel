namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTwoPropertiesInPoSummary : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.POSummaries", "ActualPcs", c => c.Int(nullable: false));
            AddColumn("dbo.POSummaries", "ActualCtns", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.POSummaries", "ActualCtns");
            DropColumn("dbo.POSummaries", "ActualPcs");
        }
    }
}
