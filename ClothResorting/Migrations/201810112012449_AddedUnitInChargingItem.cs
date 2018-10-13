namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUnitInChargingItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItems", "Unit", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItems", "Unit");
        }
    }
}
