namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RestoredSilkPreRecerveOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "SilkIconPreReceiveOrder_Id" });
            DropPrimaryKey("dbo.SilkIconPreReceiveOrders");
            AlterColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", c => c.Int());
            AlterColumn("dbo.SilkIconPreReceiveOrders", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.SilkIconPreReceiveOrders", "Id");
            CreateIndex("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "SilkIconPreReceiveOrder_Id" });
            DropPrimaryKey("dbo.SilkIconPreReceiveOrders");
            AlterColumn("dbo.SilkIconPreReceiveOrders", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.SilkIconPreReceiveOrders", "Id");
            CreateIndex("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
    }
}
