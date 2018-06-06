namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableSilkIconPackingLists : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SilkIconPackingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrderNumber = c.String(),
                        StyleNumber = c.String(),
                        Quantity = c.String(),
                        Cartons = c.String(),
                        NetWeight = c.String(),
                        GrossWeight = c.String(),
                        CBM = c.String(),
                        Vol = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SilkIconPackingLists");
        }
    }
}
