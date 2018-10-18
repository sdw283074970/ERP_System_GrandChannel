namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusInInvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "Status");
        }
    }
}
