namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMoreStatusInCharginItemDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "IsInstruction", c => c.Boolean(nullable: false));
            AddColumn("dbo.ChargingItemDetails", "IsCharging", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItemDetails", "IsCharging");
            DropColumn("dbo.ChargingItemDetails", "IsInstruction");
        }
    }
}
