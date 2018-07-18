namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedFCRegularLocationDetailWithPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "PreReceiveOrder_Id", c => c.Int());
            CreateIndex("dbo.FCRegularLocationDetails", "PreReceiveOrder_Id");
            AddForeignKey("dbo.FCRegularLocationDetails", "PreReceiveOrder_Id", "dbo.PreReceiveOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FCRegularLocationDetails", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropIndex("dbo.FCRegularLocationDetails", new[] { "PreReceiveOrder_Id" });
            DropColumn("dbo.FCRegularLocationDetails", "PreReceiveOrder_Id");
        }
    }
}
