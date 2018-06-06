namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIdentityMainKeyInSilkIconPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropPrimaryKey("dbo.SilkIconPreReceiveOrders");
            AlterColumn("dbo.SilkIconPreReceiveOrders", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.SilkIconPreReceiveOrders", "Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropPrimaryKey("dbo.SilkIconPreReceiveOrders");
            AlterColumn("dbo.SilkIconPreReceiveOrders", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.SilkIconPreReceiveOrders", "Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
    }
}
