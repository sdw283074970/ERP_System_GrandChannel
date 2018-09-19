namespace ClothResorting.Migrations.FBA
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedChargeMethods : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargeMethods", "From", c => c.Int(nullable: false));
            AddColumn("dbo.ChargeMethods", "To", c => c.Int(nullable: false));
            AddColumn("dbo.ChargeMethods", "Duration", c => c.Int(nullable: false));
            DropColumn("dbo.ChargeMethods", "Period");
            DropColumn("dbo.ChargeMethods", "WeekNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChargeMethods", "WeekNumber", c => c.Int(nullable: false));
            AddColumn("dbo.ChargeMethods", "Period", c => c.String());
            DropColumn("dbo.ChargeMethods", "Duration");
            DropColumn("dbo.ChargeMethods", "To");
            DropColumn("dbo.ChargeMethods", "From");
        }
    }
}
