namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAvailablePickingShippedPropertiesInFCRegularCartonLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "AvailableCtns", c => c.Int(nullable: false));
            AddColumn("dbo.FCRegularLocationDetails", "PickingCtns", c => c.Int(nullable: false));
            AddColumn("dbo.FCRegularLocationDetails", "ShippedCtns", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FCRegularLocationDetails", "ShippedCtns");
            DropColumn("dbo.FCRegularLocationDetails", "PickingCtns");
            DropColumn("dbo.FCRegularLocationDetails", "AvailableCtns");
        }
    }
}
