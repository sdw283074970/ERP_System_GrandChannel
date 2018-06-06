namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedSomeColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "TotalVol", c => c.String());
            DropColumn("dbo.SilkIconPackingListOverViews", "Vol");
            DropColumn("dbo.SilkIconPackingLists", "Vol");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPackingLists", "Vol", c => c.String());
            AddColumn("dbo.SilkIconPackingListOverViews", "Vol", c => c.String());
            DropColumn("dbo.SilkIconPackingListOverViews", "TotalVol");
        }
    }
}
