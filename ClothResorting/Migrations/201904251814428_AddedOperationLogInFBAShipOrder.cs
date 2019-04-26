namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperationLogInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "OperationLog", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "OperationLog");
        }
    }
}
