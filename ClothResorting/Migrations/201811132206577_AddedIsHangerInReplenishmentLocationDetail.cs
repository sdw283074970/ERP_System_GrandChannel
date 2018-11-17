namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsHangerInReplenishmentLocationDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PermanentLocIORecords", "PermanentLocation_Id", "dbo.PermanentLocations");
            DropIndex("dbo.PermanentLocIORecords", new[] { "PermanentLocation_Id" });
            AddColumn("dbo.ReplenishmentLocationDetails", "IsHanger", c => c.Boolean(nullable: false));
            DropTable("dbo.PermanentLocations");
            DropTable("dbo.PermanentLocIORecords");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PermanentLocIORecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PermanentLoc = c.String(),
                        OrderPurchaseOrder = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        TargetPcs = c.Int(nullable: false),
                        InvBefore = c.Int(nullable: false),
                        InvChange = c.Int(nullable: false),
                        InvAfter = c.Int(nullable: false),
                        FromLocation = c.String(),
                        TargetBalance = c.Int(nullable: false),
                        OperationDate = c.DateTime(nullable: false),
                        PermanentLocation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PermanentLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Vender = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        Location = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.ReplenishmentLocationDetails", "IsHanger");
            CreateIndex("dbo.PermanentLocIORecords", "PermanentLocation_Id");
            AddForeignKey("dbo.PermanentLocIORecords", "PermanentLocation_Id", "dbo.PermanentLocations", "Id");
        }
    }
}
