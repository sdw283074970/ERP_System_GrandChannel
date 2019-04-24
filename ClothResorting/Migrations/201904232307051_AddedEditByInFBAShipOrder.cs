namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEditByInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "EditBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "EditBy");
        }
    }
}
