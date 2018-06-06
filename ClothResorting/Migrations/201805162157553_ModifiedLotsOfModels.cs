namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedLotsOfModels : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters");
            DropIndex("dbo.SilkIconCartonDetails", new[] { "DistributionCenter_Id" });
            CreateTable(
                "dbo.SizeRatios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SizeName = c.String(),
                        Count = c.Int(),
                        SilkIconCartonDetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconCartonDetails", t => t.SilkIconCartonDetail_Id)
                .Index(t => t.SilkIconCartonDetail_Id);
            
            AddColumn("dbo.SilkIconCartonDetails", "Style", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "RunCode", c => c.String());
            AddColumn("dbo.SilkIconPackingLists", "Date", c => c.DateTime());
            AddColumn("dbo.SilkIconPackingLists", "PackedCartons", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "NumberOfSizeRatio", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "NumberOfDemension", c => c.Int());
            DropColumn("dbo.SilkIconCartonDetails", "DistrubutionCenterName");
            DropColumn("dbo.SilkIconCartonDetails", "S");
            DropColumn("dbo.SilkIconCartonDetails", "M");
            DropColumn("dbo.SilkIconCartonDetails", "L");
            DropColumn("dbo.SilkIconCartonDetails", "XL");
            DropColumn("dbo.SilkIconCartonDetails", "XXL");
            DropColumn("dbo.SilkIconCartonDetails", "XXXL");
            DropColumn("dbo.SilkIconCartonDetails", "DistributionCenter_Id");
            DropColumn("dbo.SilkIconPackingLists", "Quantity");
            DropColumn("dbo.SilkIconPackingLists", "Cartons");
            DropColumn("dbo.SilkIconPreReceiveOrders", "InvoiceNumber");
            DropTable("dbo.DistributionCenters");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DistributionCenters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SilkIconPreReceiveOrders", "InvoiceNumber", c => c.String());
            AddColumn("dbo.SilkIconPackingLists", "Cartons", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "Quantity", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "DistributionCenter_Id", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "XXXL", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "XXL", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "XL", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "L", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "M", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "S", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "DistrubutionCenterName", c => c.String());
            DropForeignKey("dbo.SizeRatios", "SilkIconCartonDetail_Id", "dbo.SilkIconCartonDetails");
            DropIndex("dbo.SizeRatios", new[] { "SilkIconCartonDetail_Id" });
            DropColumn("dbo.SilkIconPackingLists", "NumberOfDemension");
            DropColumn("dbo.SilkIconPackingLists", "NumberOfSizeRatio");
            DropColumn("dbo.SilkIconPackingLists", "PackedCartons");
            DropColumn("dbo.SilkIconPackingLists", "Date");
            DropColumn("dbo.SilkIconCartonDetails", "RunCode");
            DropColumn("dbo.SilkIconCartonDetails", "Style");
            DropTable("dbo.SizeRatios");
            CreateIndex("dbo.SilkIconCartonDetails", "DistributionCenter_Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters", "Id");
        }
    }
}
