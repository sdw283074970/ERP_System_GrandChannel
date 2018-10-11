namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedNullableDateTimetoString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Invoices", "InvoiceDate", c => c.String());
            AlterColumn("dbo.Invoices", "DueDate", c => c.String());
            AlterColumn("dbo.Invoices", "ShipDate", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Invoices", "ShipDate", c => c.DateTime());
            AlterColumn("dbo.Invoices", "DueDate", c => c.DateTime());
            AlterColumn("dbo.Invoices", "InvoiceDate", c => c.DateTime());
        }
    }
}
