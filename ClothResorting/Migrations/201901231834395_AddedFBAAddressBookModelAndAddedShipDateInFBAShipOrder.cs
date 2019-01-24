namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAAddressBookModelAndAddedShipDateInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAAddressBooks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WarehouseCode = c.String(),
                        Address = c.String(),
                        Memo = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FBAShipOrders", "ShipDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "ShipDate");
            DropTable("dbo.FBAAddressBooks");
        }
    }
}
