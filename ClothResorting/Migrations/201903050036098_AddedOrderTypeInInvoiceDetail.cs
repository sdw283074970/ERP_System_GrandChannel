namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderTypeInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "OrderType");
        }
    }
}
