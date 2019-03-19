namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCostInInvoiceDetail1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "Cost", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "Cost");
        }
    }
}
