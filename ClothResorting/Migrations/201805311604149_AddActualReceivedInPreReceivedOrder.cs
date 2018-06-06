namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActualReceivedInPreReceivedOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "ActualReceived", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "ActualReceived");
        }
    }
}
