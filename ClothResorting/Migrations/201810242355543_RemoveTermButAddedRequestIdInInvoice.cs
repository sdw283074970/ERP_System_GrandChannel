namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTermButAddedRequestIdInInvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "RequestId", c => c.String());
            DropColumn("dbo.Invoices", "Terms");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Invoices", "Terms", c => c.String());
            DropColumn("dbo.Invoices", "RequestId");
        }
    }
}
