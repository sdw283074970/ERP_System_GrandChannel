namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPickingListsAndPickingRecordsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PickingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreateDate = c.DateTime(),
                        PickTicketsRange = c.String(),
                        PreReceiveOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreReceiveOrders", t => t.PreReceiveOrder_Id)
                .Index(t => t.PreReceiveOrder_Id);
            
            CreateTable(
                "dbo.PickingRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Container = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        CustomerCode = c.String(),
                        SizeBundle = c.String(),
                        PcsBundle = c.String(),
                        Cartons = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        PcsPerCaron = c.Int(nullable: false),
                        Location = c.String(),
                        PickingDate = c.DateTime(),
                        PickingList_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PickingLists", t => t.PickingList_Id)
                .Index(t => t.PickingList_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickingLists", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropForeignKey("dbo.PickingRecords", "PickingList_Id", "dbo.PickingLists");
            DropIndex("dbo.PickingRecords", new[] { "PickingList_Id" });
            DropIndex("dbo.PickingLists", new[] { "PreReceiveOrder_Id" });
            DropTable("dbo.PickingRecords");
            DropTable("dbo.PickingLists");
        }
    }
}
