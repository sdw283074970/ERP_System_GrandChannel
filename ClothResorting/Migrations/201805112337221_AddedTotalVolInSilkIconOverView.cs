namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTotalVolInSilkIconOverView : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "TotalVol", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPackingListOverViews", "TotalVol");
        }
    }
}
