namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedUnitInChargingItem : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ChargingItems", "Unit");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChargingItems", "Unit", c => c.Int(nullable: false));
        }
    }
}
