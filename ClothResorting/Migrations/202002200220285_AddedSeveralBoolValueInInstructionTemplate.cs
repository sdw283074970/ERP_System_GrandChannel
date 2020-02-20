namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSeveralBoolValueInInstructionTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InstructionTemplates", "IsInstruction", c => c.Boolean(nullable: false));
            AddColumn("dbo.InstructionTemplates", "IsOperation", c => c.Boolean(nullable: false));
            AddColumn("dbo.InstructionTemplates", "IsCharging", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InstructionTemplates", "IsCharging");
            DropColumn("dbo.InstructionTemplates", "IsOperation");
            DropColumn("dbo.InstructionTemplates", "IsInstruction");
        }
    }
}
