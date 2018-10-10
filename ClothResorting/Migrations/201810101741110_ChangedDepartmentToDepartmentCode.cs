namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedDepartmentToDepartmentCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShipOrders", "DepartmentCode", c => c.String());
            AddColumn("dbo.UpperVendors", "DepartmentCode", c => c.String());
            DropColumn("dbo.ShipOrders", "Department");
            DropColumn("dbo.UpperVendors", "Department");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UpperVendors", "Department", c => c.String());
            AddColumn("dbo.ShipOrders", "Department", c => c.String());
            DropColumn("dbo.UpperVendors", "DepartmentCode");
            DropColumn("dbo.ShipOrders", "DepartmentCode");
        }
    }
}
