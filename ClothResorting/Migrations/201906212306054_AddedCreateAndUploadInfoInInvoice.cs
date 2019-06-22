namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreateAndUploadInfoInInvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "CreatedBy", c => c.String());
            AddColumn("dbo.Invoices", "UploadedBy", c => c.String());
            AddColumn("dbo.Invoices", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Invoices", "UploadedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "UploadedDate");
            DropColumn("dbo.Invoices", "CreatedDate");
            DropColumn("dbo.Invoices", "UploadedBy");
            DropColumn("dbo.Invoices", "CreatedBy");
        }
    }
}
