namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedSilkIconPreReceiveOrderDbSet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders");
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "SilkIconPreReceiveOrder_Id" });
            DropColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            DropTable("dbo.SilkIconPreReceiveOrders");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SilkIconPreReceiveOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                        CreatDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", c => c.Int());
            CreateIndex("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id");
            AddForeignKey("dbo.SilkIconPackingListOverViews", "SilkIconPreReceiveOrder_Id", "dbo.SilkIconPreReceiveOrders", "Id");
        }
    }
}
