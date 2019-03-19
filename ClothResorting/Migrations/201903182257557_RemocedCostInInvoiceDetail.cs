namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemocedCostInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.InvoiceDetails", "Cost");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvoiceDetails", "Cost", c => c.String());
        }
    }
}
