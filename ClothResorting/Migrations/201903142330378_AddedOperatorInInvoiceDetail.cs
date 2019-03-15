namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperatorInInvoiceDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "Operator", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "Operator");
        }
    }
}
