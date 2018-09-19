namespace ClothResorting.Migrations.FBA
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCurrencyInChargeTemplateAndMethod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargeMethods", "Currency", c => c.String());
            AddColumn("dbo.ChargeTemplates", "Currency", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargeTemplates", "Currency");
            DropColumn("dbo.ChargeMethods", "Currency");
        }
    }
}
