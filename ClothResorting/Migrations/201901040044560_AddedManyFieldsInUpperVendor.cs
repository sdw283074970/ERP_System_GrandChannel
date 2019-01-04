namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedManyFieldsInUpperVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "FirstAddressLine", c => c.String());
            AddColumn("dbo.UpperVendors", "SecondAddressLine", c => c.String());
            AddColumn("dbo.UpperVendors", "TelNumber", c => c.String());
            AddColumn("dbo.UpperVendors", "EmailAddress", c => c.String());
            AddColumn("dbo.UpperVendors", "ContactPerson", c => c.String());
            AddColumn("dbo.UpperVendors", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpperVendors", "Status");
            DropColumn("dbo.UpperVendors", "ContactPerson");
            DropColumn("dbo.UpperVendors", "EmailAddress");
            DropColumn("dbo.UpperVendors", "TelNumber");
            DropColumn("dbo.UpperVendors", "SecondAddressLine");
            DropColumn("dbo.UpperVendors", "FirstAddressLine");
        }
    }
}
