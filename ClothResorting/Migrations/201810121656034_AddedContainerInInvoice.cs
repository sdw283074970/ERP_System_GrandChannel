namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedContainerInInvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "Container", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "Container");
        }
    }
}
