namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PreReceiveOrders", "Status");
        }
    }
}
