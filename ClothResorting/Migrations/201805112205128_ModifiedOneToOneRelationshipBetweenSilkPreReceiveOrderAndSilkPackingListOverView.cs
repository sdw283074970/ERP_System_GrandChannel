namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedOneToOneRelationshipBetweenSilkPreReceiveOrderAndSilkPackingListOverView : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "SilkIconPreReceiveOrder_Id" });
            AddColumn("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id", c => c.Int());
            CreateIndex("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id");
            AddForeignKey("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews", "Id");
            DropColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", c => c.Int());
            DropForeignKey("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropIndex("dbo.SilkIconPreReceiveOrders", new[] { "SilkIconPackingListOverView_Id" });
            DropColumn("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id");
            CreateIndex("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
    }
}
