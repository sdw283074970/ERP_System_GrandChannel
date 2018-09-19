namespace ClothResorting.Migrations.FBA
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedChargeMethodsAndChargeTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargeMethods", "TimeUnit", c => c.String());
            AddColumn("dbo.ChargeTemplates", "TimeUnit", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargeTemplates", "TimeUnit");
            DropColumn("dbo.ChargeMethods", "TimeUnit");
        }
    }
}
