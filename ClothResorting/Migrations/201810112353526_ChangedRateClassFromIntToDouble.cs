namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedRateClassFromIntToDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ChargingItems", "Rate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ChargingItems", "Rate", c => c.Int(nullable: false));
        }
    }
}
