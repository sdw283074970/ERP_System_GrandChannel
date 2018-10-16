namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMemoAndQuantityInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "Quantity", c => c.Double(nullable: false));
            AddColumn("dbo.InvoiceDetails", "Memo", c => c.String());
            AlterColumn("dbo.InvoiceDetails", "Rate", c => c.Double(nullable: false));
            AlterColumn("dbo.InvoiceDetails", "Amount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvoiceDetails", "Amount", c => c.String());
            AlterColumn("dbo.InvoiceDetails", "Rate", c => c.String());
            DropColumn("dbo.InvoiceDetails", "Memo");
            DropColumn("dbo.InvoiceDetails", "Quantity");
        }
    }
}
