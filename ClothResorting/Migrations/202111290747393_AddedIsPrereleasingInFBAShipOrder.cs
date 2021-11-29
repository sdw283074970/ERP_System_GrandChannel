namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsPrereleasingInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "IsPrereleasing", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "IsPrereleasing");
        }
    }
}
