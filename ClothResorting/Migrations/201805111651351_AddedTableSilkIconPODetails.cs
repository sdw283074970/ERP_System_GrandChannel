namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableSilkIconPODetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SilKIconPODetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrderNumber = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Total = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", c => c.Int());
            CreateIndex("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropIndex("dbo.SilkIconCartonDetails", new[] { "SilKIconPODetail_Id" });
            DropColumn("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id");
            DropTable("dbo.SilKIconPODetails");
        }
    }
}
