namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVendorPropertyInShipOrderDbSetAndDeletedLotsOfUselessDbSets : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PickingRecords", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails");
            DropForeignKey("dbo.PickingRecords", "PickingList_Id", "dbo.PickingLists");
            DropForeignKey("dbo.PickingLists", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropForeignKey("dbo.PickingRecords", "ShipOrder_Id", "dbo.ShipOrders");
            DropForeignKey("dbo.PickDetails", "PullSheet_Id", "dbo.PullSheets");
            DropForeignKey("dbo.PullSheetDiagnostics", "PullSheet_Id", "dbo.PullSheets");
            DropIndex("dbo.PickingRecords", new[] { "FCRegularLocationDetail_Id" });
            DropIndex("dbo.PickingRecords", new[] { "PickingList_Id" });
            DropIndex("dbo.PickingRecords", new[] { "ShipOrder_Id" });
            DropIndex("dbo.PickingLists", new[] { "PreReceiveOrder_Id" });
            DropIndex("dbo.PickDetails", new[] { "PullSheet_Id" });
            DropIndex("dbo.PullSheetDiagnostics", new[] { "PullSheet_Id" });
            AddColumn("dbo.ShipOrders", "Address", c => c.String());
            AddColumn("dbo.ShipOrders", "CreateDate", c => c.String());
            AddColumn("dbo.ShipOrders", "PickDate", c => c.String());
            AddColumn("dbo.ShipOrders", "PickingMan", c => c.String());
            AddColumn("dbo.ShipOrders", "Operator", c => c.String());
            AddColumn("dbo.ShipOrders", "ShippingMan", c => c.String());
            AddColumn("dbo.ShipOrders", "Vendor", c => c.String());
            AddColumn("dbo.PickDetails", "ShipOrder_Id", c => c.Int());
            AddColumn("dbo.PullSheetDiagnostics", "ShipOrder_Id", c => c.Int());
            CreateIndex("dbo.PickDetails", "ShipOrder_Id");
            CreateIndex("dbo.PullSheetDiagnostics", "ShipOrder_Id");
            AddForeignKey("dbo.PickDetails", "ShipOrder_Id", "dbo.ShipOrders", "Id");
            AddForeignKey("dbo.PullSheetDiagnostics", "ShipOrder_Id", "dbo.ShipOrders", "Id");
            DropColumn("dbo.ShipOrders", "Address_1");
            DropColumn("dbo.ShipOrders", "Address_2");
            DropColumn("dbo.ShipOrders", "ShipDate");
            DropColumn("dbo.PickDetails", "PullSheet_Id");
            DropColumn("dbo.PullSheetDiagnostics", "PullSheet_Id");
            DropTable("dbo.PickingRecords");
            DropTable("dbo.PickingLists");
            DropTable("dbo.PullSheets");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PullSheets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderPurchaseOrder = c.String(),
                        Customer = c.String(),
                        Address = c.String(),
                        PickTicketsRange = c.String(),
                        Status = c.String(),
                        CreateDate = c.String(),
                        PickDate = c.String(),
                        PickingMan = c.String(),
                        Operator = c.String(),
                        ShippingMan = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PickingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderPurchaseOrder = c.String(),
                        CreateDate = c.DateTime(),
                        PickTicketsRange = c.String(),
                        Status = c.String(),
                        PreReceiveOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PickingRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderPurchaseOrder = c.String(),
                        Container = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        CustomerCode = c.String(),
                        SizeBundle = c.String(),
                        PcsBundle = c.String(),
                        Cartons = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        PcsPerCarton = c.Int(nullable: false),
                        Location = c.String(),
                        PickingDate = c.DateTime(),
                        FCRegularLocationDetail_Id = c.Int(),
                        PickingList_Id = c.Int(),
                        ShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PullSheetDiagnostics", "PullSheet_Id", c => c.Int());
            AddColumn("dbo.PickDetails", "PullSheet_Id", c => c.Int());
            AddColumn("dbo.ShipOrders", "ShipDate", c => c.String());
            AddColumn("dbo.ShipOrders", "Address_2", c => c.String());
            AddColumn("dbo.ShipOrders", "Address_1", c => c.String());
            DropForeignKey("dbo.PullSheetDiagnostics", "ShipOrder_Id", "dbo.ShipOrders");
            DropForeignKey("dbo.PickDetails", "ShipOrder_Id", "dbo.ShipOrders");
            DropIndex("dbo.PullSheetDiagnostics", new[] { "ShipOrder_Id" });
            DropIndex("dbo.PickDetails", new[] { "ShipOrder_Id" });
            DropColumn("dbo.PullSheetDiagnostics", "ShipOrder_Id");
            DropColumn("dbo.PickDetails", "ShipOrder_Id");
            DropColumn("dbo.ShipOrders", "Vendor");
            DropColumn("dbo.ShipOrders", "ShippingMan");
            DropColumn("dbo.ShipOrders", "Operator");
            DropColumn("dbo.ShipOrders", "PickingMan");
            DropColumn("dbo.ShipOrders", "PickDate");
            DropColumn("dbo.ShipOrders", "CreateDate");
            DropColumn("dbo.ShipOrders", "Address");
            CreateIndex("dbo.PullSheetDiagnostics", "PullSheet_Id");
            CreateIndex("dbo.PickDetails", "PullSheet_Id");
            CreateIndex("dbo.PickingLists", "PreReceiveOrder_Id");
            CreateIndex("dbo.PickingRecords", "ShipOrder_Id");
            CreateIndex("dbo.PickingRecords", "PickingList_Id");
            CreateIndex("dbo.PickingRecords", "FCRegularLocationDetail_Id");
            AddForeignKey("dbo.PullSheetDiagnostics", "PullSheet_Id", "dbo.PullSheets", "Id");
            AddForeignKey("dbo.PickDetails", "PullSheet_Id", "dbo.PullSheets", "Id");
            AddForeignKey("dbo.PickingRecords", "ShipOrder_Id", "dbo.ShipOrders", "Id");
            AddForeignKey("dbo.PickingLists", "PreReceiveOrder_Id", "dbo.PreReceiveOrders", "Id");
            AddForeignKey("dbo.PickingRecords", "PickingList_Id", "dbo.PickingLists", "Id");
            AddForeignKey("dbo.PickingRecords", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails", "Id");
        }
    }
}
