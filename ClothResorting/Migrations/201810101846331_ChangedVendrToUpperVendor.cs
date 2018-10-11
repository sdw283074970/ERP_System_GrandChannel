namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedVendrToUpperVendor : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Invoices", name: "Venodr_Id", newName: "UpperVendor_Id");
            RenameIndex(table: "dbo.Invoices", name: "IX_Venodr_Id", newName: "IX_UpperVendor_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Invoices", name: "IX_UpperVendor_Id", newName: "IX_Venodr_Id");
            RenameColumn(table: "dbo.Invoices", name: "UpperVendor_Id", newName: "Venodr_Id");
        }
    }
}
