namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreateDateAndCreateByInChargingItemDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "CreateDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ChargingItemDetails", "CreateBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItemDetails", "CreateBy");
            DropColumn("dbo.ChargingItemDetails", "CreateDate");
        }
    }
}
