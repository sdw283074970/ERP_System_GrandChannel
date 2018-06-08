namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameStatusToContainerNumberInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "ContainerNumber", c => c.String());
            DropColumn("dbo.SilkIconPreReceiveOrders", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "Status", c => c.String());
            DropColumn("dbo.SilkIconPreReceiveOrders", "ContainerNumber");
        }
    }
}
