namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReTypedRelationshipInSilkIconPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "SilkIconPreReceiveOrder_Id" });
            DropColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
        }
    }
}
