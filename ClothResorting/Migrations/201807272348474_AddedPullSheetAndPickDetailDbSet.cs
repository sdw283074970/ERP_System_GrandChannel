namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPullSheetAndPickDetailDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PickDetails",
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
                        Status = c.String(),
                        Location = c.String(),
                        PickCtns = c.Int(nullable: false),
                        PickPcs = c.Int(nullable: false),
                        InboundDate = c.DateTime(),
                        PullSheet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PullSheets", t => t.PullSheet_Id)
                .Index(t => t.PullSheet_Id);
            
            CreateTable(
                "dbo.PullSheets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PickTicketsRange = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FCRegularLocationDetails", "AvailablePcs", c => c.Int(nullable: false));
            AddColumn("dbo.FCRegularLocationDetails", "PickingPcs", c => c.Int(nullable: false));
            AddColumn("dbo.FCRegularLocationDetails", "ShippedPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickDetails", "PullSheet_Id", "dbo.PullSheets");
            DropIndex("dbo.PickDetails", new[] { "PullSheet_Id" });
            DropColumn("dbo.FCRegularLocationDetails", "ShippedPcs");
            DropColumn("dbo.FCRegularLocationDetails", "PickingPcs");
            DropColumn("dbo.FCRegularLocationDetails", "AvailablePcs");
            DropTable("dbo.PullSheets");
            DropTable("dbo.PickDetails");
        }
    }
}
