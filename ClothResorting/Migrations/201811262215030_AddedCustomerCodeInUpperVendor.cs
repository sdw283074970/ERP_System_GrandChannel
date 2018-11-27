namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCustomerCodeInUpperVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "CustomerCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpperVendors", "CustomerCode");
        }
    }
}
