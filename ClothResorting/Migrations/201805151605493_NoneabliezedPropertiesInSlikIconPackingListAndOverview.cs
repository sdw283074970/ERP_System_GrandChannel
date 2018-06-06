namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoneabliezedPropertiesInSlikIconPackingListAndOverview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "TotalVol", c => c.Double());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalCartons", c => c.Int());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalGrossWeight", c => c.Double());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalNetWeight", c => c.Double());
            AlterColumn("dbo.SilkIconPackingLists", "Quantity", c => c.Int());
            AlterColumn("dbo.SilkIconPackingLists", "Cartons", c => c.Int());
            AlterColumn("dbo.SilkIconPackingLists", "NetWeight", c => c.Double());
            AlterColumn("dbo.SilkIconPackingLists", "GrossWeight", c => c.Double());
            AlterColumn("dbo.SilkIconPackingLists", "CFT", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SilkIconPackingLists", "CFT", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "GrossWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "NetWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "Cartons", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "Quantity", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalNetWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalGrossWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalCartons", c => c.Int(nullable: false));
            DropColumn("dbo.SilkIconPackingListOverViews", "TotalVol");
        }
    }
}
