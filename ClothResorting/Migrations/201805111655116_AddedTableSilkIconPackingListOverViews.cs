namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableSilkIconPackingListOverViews : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SilkIconPackingListOverViews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceNumber = c.String(),
                        Date = c.DateTime(),
                        TotalCartons = c.String(),
                        Vol = c.String(),
                        TotalGrossWeight = c.String(),
                        TotalNetWeight = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", c => c.Int());
            CreateIndex("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id");
            AddForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropIndex("dbo.SilkIconPackingLists", new[] { "SilkIconPackingListOverView_Id" });
            DropColumn("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id");
            DropTable("dbo.SilkIconPackingListOverViews");
        }
    }
}
