namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsApplyToShipOrderAndMasterOrderInInstructionTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InstructionTemplates", "IsApplyToShipOrder", c => c.Boolean(nullable: false));
            AddColumn("dbo.InstructionTemplates", "IsApplyToMasterOrder", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InstructionTemplates", "IsApplyToMasterOrder");
            DropColumn("dbo.InstructionTemplates", "IsApplyToShipOrder");
        }
    }
}
