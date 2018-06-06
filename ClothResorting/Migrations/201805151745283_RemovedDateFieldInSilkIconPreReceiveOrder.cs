namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedDateFieldInSilkIconPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "Date", c => c.DateTime());
        }
    }
}
