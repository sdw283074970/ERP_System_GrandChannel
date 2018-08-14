namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderTypeInRegualrCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "OrderType");
        }
    }
}
