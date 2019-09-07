namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSubCustomerInFBASystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "SubCustomer", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "SubCustomer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "SubCustomer");
            DropColumn("dbo.FBAShipOrders", "SubCustomer");
        }
    }
}
