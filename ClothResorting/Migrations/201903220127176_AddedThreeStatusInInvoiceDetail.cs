namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedThreeStatusInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "CostConfirm", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvoiceDetails", "CollectionStatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvoiceDetails", "PaymentStatus", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "PaymentStatus");
            DropColumn("dbo.InvoiceDetails", "CollectionStatus");
            DropColumn("dbo.InvoiceDetails", "CostConfirm");
        }
    }
}
