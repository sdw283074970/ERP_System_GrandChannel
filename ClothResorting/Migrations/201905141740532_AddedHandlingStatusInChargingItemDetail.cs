namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHandlingStatusInChargingItemDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "HandlingStatus", c => c.String());
            AddColumn("dbo.ChargingItemDetails", "ConfirmedBy", c => c.String());
            DropColumn("dbo.ChargingItemDetails", "IsHandledFeedback");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChargingItemDetails", "IsHandledFeedback", c => c.Boolean(nullable: false));
            DropColumn("dbo.ChargingItemDetails", "ConfirmedBy");
            DropColumn("dbo.ChargingItemDetails", "HandlingStatus");
        }
    }
}
