namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusInInstructionTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InstructionTemplates", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InstructionTemplates", "Status");
        }
    }
}
