namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedTotalVolColumnInSilkIconOverView : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SilkIconPackingListOverViews", "TotalVol");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "TotalVol", c => c.String());
        }
    }
}
