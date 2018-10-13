namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUnitInChargingItem1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItems", "Unit", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItems", "Unit");
        }
    }
}
