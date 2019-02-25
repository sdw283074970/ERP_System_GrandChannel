namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLastBatchInPOSummary : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.POSummaries", "Batch", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.POSummaries", "Batch");
        }
    }
}
