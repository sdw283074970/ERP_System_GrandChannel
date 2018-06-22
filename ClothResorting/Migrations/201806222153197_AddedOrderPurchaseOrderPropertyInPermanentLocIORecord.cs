namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderPurchaseOrderPropertyInPermanentLocIORecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PermanentLocIORecords", "OrderPurchaseOrder", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PermanentLocIORecords", "OrderPurchaseOrder");
        }
    }
}
