namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "Status");
        }
    }
}
