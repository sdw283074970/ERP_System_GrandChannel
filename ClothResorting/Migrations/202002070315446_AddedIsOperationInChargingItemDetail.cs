namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsOperationInChargingItemDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "IsOperation", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItemDetails", "IsOperation");
        }
    }
}
