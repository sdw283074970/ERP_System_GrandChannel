namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedVendorToUpperVendor : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ChargingItems", name: "Vendor_Id", newName: "UpperVendor_Id");
            RenameIndex(table: "dbo.ChargingItems", name: "IX_Vendor_Id", newName: "IX_UpperVendor_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ChargingItems", name: "IX_UpperVendor_Id", newName: "IX_Vendor_Id");
            RenameColumn(table: "dbo.ChargingItems", name: "UpperVendor_Id", newName: "Vendor_Id");
        }
    }
}
