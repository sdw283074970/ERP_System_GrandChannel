namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedLotsOfFieldTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "GrossWeightPerCartons", c => c.Double(nullable: false));
            AddColumn("dbo.SilkIconCartonDetails", "NetWeightPerCartons", c => c.Double(nullable: false));
            AddColumn("dbo.SilkIconPackingLists", "CFT", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "SumOfCarton", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Long", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Width", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Height", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "S", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "M", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "L", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "XL", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "XXL", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "XXXL", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "PcsPerCartons", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "TotalPcs", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalCartons", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalGrossWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalNetWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "Quantity", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "Cartons", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "NetWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconPackingLists", "GrossWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.SilKIconPODetails", "TotalCartons", c => c.Int(nullable: false));
            DropColumn("dbo.SilkIconCartonDetails", "GrossWeight");
            DropColumn("dbo.SilkIconCartonDetails", "NetWeight");
            DropColumn("dbo.SilkIconPackingLists", "CBM");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPackingLists", "CBM", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "NetWeight", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "GrossWeight", c => c.String());
            AlterColumn("dbo.SilKIconPODetails", "TotalCartons", c => c.String());
            AlterColumn("dbo.SilkIconPackingLists", "GrossWeight", c => c.String());
            AlterColumn("dbo.SilkIconPackingLists", "NetWeight", c => c.String());
            AlterColumn("dbo.SilkIconPackingLists", "Cartons", c => c.String());
            AlterColumn("dbo.SilkIconPackingLists", "Quantity", c => c.String());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalNetWeight", c => c.String());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalGrossWeight", c => c.String());
            AlterColumn("dbo.SilkIconPackingListOverViews", "TotalCartons", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "TotalPcs", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "PcsPerCartons", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "XXXL", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "XXL", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "XL", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "L", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "M", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "S", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "Height", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "Width", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "Long", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "SumOfCarton", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo", c => c.String());
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom", c => c.String());
            DropColumn("dbo.SilkIconPackingLists", "CFT");
            DropColumn("dbo.SilkIconCartonDetails", "NetWeightPerCartons");
            DropColumn("dbo.SilkIconCartonDetails", "GrossWeightPerCartons");
        }
    }
}
