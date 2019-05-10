namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFeedbackInChargingItemDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "Comment", c => c.String());
            AddColumn("dbo.ChargingItemDetails", "Result", c => c.String());
            AddColumn("dbo.ChargingItemDetails", "IsHandledFeedback", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItemDetails", "IsHandledFeedback");
            DropColumn("dbo.ChargingItemDetails", "Result");
            DropColumn("dbo.ChargingItemDetails", "Comment");
        }
    }
}
