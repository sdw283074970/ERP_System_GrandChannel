namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedInvoiceTotalDueFromIntToDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Invoices", "TotalDue", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Invoices", "TotalDue", c => c.Int(nullable: false));
        }
    }
}
