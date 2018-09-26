namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperatorInReplenishmentLocationDetailDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReplenishmentLocationDetails", "Operator", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReplenishmentLocationDetails", "Operator");
        }
    }
}
