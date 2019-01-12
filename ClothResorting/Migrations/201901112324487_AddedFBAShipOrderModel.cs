namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAShipOrderModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAShipOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShipOrderNumber = c.String(),
                        CustomerCode = c.String(),
                        Destination = c.String(),
                        PickReference = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        CreateBy = c.String(),
                        PickDate = c.DateTime(nullable: false),
                        PickMan = c.String(),
                        Status = c.String(),
                        ShippedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FBAShipOrders");
        }
    }
}
