namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsActiveInWarehouseLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WarehouseLocations", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WarehouseLocations", "IsActive");
        }
    }
}
