namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommenedOutPropertiesInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "CreatDate", c => c.DateTime());
            DropColumn("dbo.SilkIconPreReceiveOrders", "PurchaseOrderNumber");
            DropColumn("dbo.SilkIconPreReceiveOrders", "PickTicketNumber");
            DropColumn("dbo.SilkIconPreReceiveOrders", "ToWhom");
            DropColumn("dbo.SilkIconPreReceiveOrders", "StartDate");
            DropColumn("dbo.SilkIconPreReceiveOrders", "CancelDate");
            DropColumn("dbo.SilkIconPreReceiveOrders", "ActualShippingDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "ActualShippingDate", c => c.DateTime());
            AddColumn("dbo.SilkIconPreReceiveOrders", "CancelDate", c => c.DateTime());
            AddColumn("dbo.SilkIconPreReceiveOrders", "StartDate", c => c.DateTime());
            AddColumn("dbo.SilkIconPreReceiveOrders", "ToWhom", c => c.String());
            AddColumn("dbo.SilkIconPreReceiveOrders", "PickTicketNumber", c => c.String());
            AddColumn("dbo.SilkIconPreReceiveOrders", "PurchaseOrderNumber", c => c.String());
            DropColumn("dbo.SilkIconPreReceiveOrders", "CreatDate");
        }
    }
}
