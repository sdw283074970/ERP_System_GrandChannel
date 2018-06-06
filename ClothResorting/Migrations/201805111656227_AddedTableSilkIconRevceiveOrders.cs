namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableSilkIconRevceiveOrders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SilkIconPreReceiveOrders",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PurchaseOrderNumber = c.String(),
                        PickTicketNumber = c.String(),
                        ToWhom = c.String(),
                        StartDate = c.DateTime(),
                        CancelDate = c.DateTime(),
                        ActualShippingDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SilkIconPreReceiveOrders");
        }
    }
}
