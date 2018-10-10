namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInvoiceAndInvoiceDetailsAndChargingItemsDbSets : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChargingItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChargingType = c.String(),
                        Name = c.String(),
                        Rate = c.Int(nullable: false),
                        Description = c.String(),
                        Vendor_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UpperVendors", t => t.Vendor_Id)
                .Index(t => t.Vendor_Id);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceNumber = c.String(),
                        InvoiceType = c.String(),
                        TotalDue = c.Int(nullable: false),
                        BillTo = c.String(),
                        Terms = c.String(),
                        Enclosed = c.String(),
                        ShipTo = c.String(),
                        ShipVia = c.String(),
                        Currency = c.String(),
                        PurchaseOrder = c.String(),
                        InvoiceDate = c.DateTime(),
                        DueDate = c.DateTime(),
                        ShipDate = c.DateTime(),
                        Venodr_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UpperVendors", t => t.Venodr_Id)
                .Index(t => t.Venodr_Id);
            
            CreateTable(
                "dbo.InvoiceDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Activity = c.String(),
                        ChargingType = c.String(),
                        Unit = c.String(),
                        Rate = c.String(),
                        Amount = c.String(),
                        Invoice_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Invoices", t => t.Invoice_Id)
                .Index(t => t.Invoice_Id);
            
            AddColumn("dbo.ShipOrders", "Department", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invoices", "Venodr_Id", "dbo.UpperVendors");
            DropForeignKey("dbo.InvoiceDetails", "Invoice_Id", "dbo.Invoices");
            DropForeignKey("dbo.ChargingItems", "Vendor_Id", "dbo.UpperVendors");
            DropIndex("dbo.InvoiceDetails", new[] { "Invoice_Id" });
            DropIndex("dbo.Invoices", new[] { "Venodr_Id" });
            DropIndex("dbo.ChargingItems", new[] { "Vendor_Id" });
            DropColumn("dbo.ShipOrders", "Department");
            DropTable("dbo.InvoiceDetails");
            DropTable("dbo.Invoices");
            DropTable("dbo.ChargingItems");
        }
    }
}
