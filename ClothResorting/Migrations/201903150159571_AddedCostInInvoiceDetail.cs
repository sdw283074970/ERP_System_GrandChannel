namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCostInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "Cost", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "Cost");
        }
    }
}
