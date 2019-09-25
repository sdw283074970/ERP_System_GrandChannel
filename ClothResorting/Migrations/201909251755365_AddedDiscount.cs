namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDiscount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvoiceDetails", "Discount", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvoiceDetails", "Discount");
        }
    }
}
