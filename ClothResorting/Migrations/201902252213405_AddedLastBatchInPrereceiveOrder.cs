namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLastBatchInPrereceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "LastBatch", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PreReceiveOrders", "LastBatch");
        }
    }
}
