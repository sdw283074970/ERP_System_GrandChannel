namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReAddedDbset : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropForeignKey("dbo.SilkIconPackingListOverViews", "Id", "dbo.SilkIconPreReceiveOrders");
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.SilkIconPackingLists", new[] { "SilkIconPackingListOverView_Id" });
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "Id" });
            DropIndex("dbo.SilKIconPODetails", new[] { "Id" });
            DropIndex("dbo.SilkIconCartonDetails", new[] { "SilKIconPODetail_Id" });
            AddColumn("dbo.SilkIconPackingLists", "PurchaseOrder_StyleNumber", c => c.String());
            AddColumn("dbo.SilkIconPackingLists", "SilkIconPreReceiveOrder_Id", c => c.Int());
            AddColumn("dbo.SilkIconPreReceiveOrders", "InvoiceNumber", c => c.String());
            AddColumn("dbo.SilkIconPreReceiveOrders", "Date", c => c.DateTime());
            AddColumn("dbo.SilkIconPreReceiveOrders", "TotalCartons", c => c.Int());
            AddColumn("dbo.SilkIconPreReceiveOrders", "TotalGrossWeight", c => c.Double());
            AddColumn("dbo.SilkIconPreReceiveOrders", "TotalNetWeight", c => c.Double());
            AddColumn("dbo.SilkIconPreReceiveOrders", "TotalVol", c => c.Double());
            AddColumn("dbo.SilkIconCartonDetails", "SilkIconPackingList_Id", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "SumOfCarton", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "Long", c => c.Double());
            AlterColumn("dbo.SilkIconCartonDetails", "Width", c => c.Double());
            AlterColumn("dbo.SilkIconCartonDetails", "Height", c => c.Double());
            AlterColumn("dbo.SilkIconCartonDetails", "GrossWeightPerCartons", c => c.Double());
            AlterColumn("dbo.SilkIconCartonDetails", "NetWeightPerCartons", c => c.Double());
            AlterColumn("dbo.SilkIconCartonDetails", "PcsPerCartons", c => c.Int());
            AlterColumn("dbo.SilkIconCartonDetails", "TotalPcs", c => c.Int());
            CreateIndex("dbo.SilkIconCartonDetails", "SilkIconPackingList_Id");
            CreateIndex("dbo.SilkIconPackingLists", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilkIconPackingList_Id", "dbo.SilkIconPackingLists", "Id");
            AddForeignKey("dbo.SilkIconPackingLists", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
            DropColumn("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id");
            DropColumn("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id");
            DropTable("dbo.SilkIconPackingListOverViews");
            DropTable("dbo.SilKIconPODetails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SilKIconPODetails",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PurchaseOrder_StyleNumber = c.String(),
                        Color = c.String(),
                        TotalCartons = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconPackingListOverViews",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        InvoiceNumber = c.String(),
                        Date = c.DateTime(),
                        TotalCartons = c.Int(),
                        TotalGrossWeight = c.Double(),
                        TotalNetWeight = c.Double(),
                        TotalVol = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", c => c.Int());
            DropForeignKey("dbo.SilkIconPackingLists", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropForeignKey("dbo.SilkIconCartonDetails", "SilkIconPackingList_Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.SilkIconPackingLists", new[] { "SilkIconPreReceiveOrder_Id" });
            DropIndex("dbo.SilkIconCartonDetails", new[] { "SilkIconPackingList_Id" });
            AlterColumn("dbo.SilkIconCartonDetails", "TotalPcs", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "PcsPerCartons", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "NetWeightPerCartons", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "GrossWeightPerCartons", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Height", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Width", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "Long", c => c.Double(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "SumOfCarton", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo", c => c.Int(nullable: false));
            AlterColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom", c => c.Int(nullable: false));
            DropColumn("dbo.SilkIconCartonDetails", "SilkIconPackingList_Id");
            DropColumn("dbo.SilkIconPreReceiveOrders", "TotalVol");
            DropColumn("dbo.SilkIconPreReceiveOrders", "TotalNetWeight");
            DropColumn("dbo.SilkIconPreReceiveOrders", "TotalGrossWeight");
            DropColumn("dbo.SilkIconPreReceiveOrders", "TotalCartons");
            DropColumn("dbo.SilkIconPreReceiveOrders", "Date");
            DropColumn("dbo.SilkIconPreReceiveOrders", "InvoiceNumber");
            DropColumn("dbo.SilkIconPackingLists", "SilkIconPreReceiveOrder_Id");
            DropColumn("dbo.SilkIconPackingLists", "PurchaseOrder_StyleNumber");
            CreateIndex("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id");
            CreateIndex("dbo.SilKIconPODetails", "Id");
            CreateIndex("dbo.SilkIconPackingListOverViews", "Id");
            CreateIndex("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id");
            AddForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists", "Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails", "Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "Id", "dbo.SilkIconPreReceiveOrders", "Id");
            AddForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews", "Id");
        }
    }
}
