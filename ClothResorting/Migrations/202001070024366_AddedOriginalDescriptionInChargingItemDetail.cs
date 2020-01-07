namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOriginalDescriptionInChargingItemDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargingItemDetails", "OriginalDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargingItemDetails", "OriginalDescription");
        }
    }
}
