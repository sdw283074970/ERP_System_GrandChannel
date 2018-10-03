namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWorkOrderTypeInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "WorkOrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PreReceiveOrders", "WorkOrderType");
        }
    }
}
