namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInboundDateInPrereceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "InboundDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PreReceiveOrders", "InboundDate");
        }
    }
}
