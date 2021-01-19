namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWarehouseLocation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WarehouseLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WarehouseName = c.String(),
                        WarehouseCode = c.String(),
                        Address = c.String(),
                        ContactPerson = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WarehouseLocations");
        }
    }
}
