namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusInReplenishmentLocationDetailDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReplenishmentLocationDetails", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReplenishmentLocationDetails", "Status");
        }
    }
}
