namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "InvoiceType", c => c.String());
            DropColumn("dbo.InvoiceDetails", "OrderType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvoiceDetails", "OrderType", c => c.String());
            DropColumn("dbo.InvoiceDetails", "InvoiceType");
        }
    }
}
