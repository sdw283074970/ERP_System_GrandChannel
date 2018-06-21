namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedRelocationRecordsDbSet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RelocatingRecords", "PermanentLocation_Id", "dbo.PermanentLocations");
            DropIndex("dbo.RelocatingRecords", new[] { "PermanentLocation_Id" });
            AddColumn("dbo.PermanentLocIORecords", "PermanentLoc", c => c.String());
            AddColumn("dbo.PermanentLocIORecords", "PermanentLocation_Id", c => c.Int());
            CreateIndex("dbo.PermanentLocIORecords", "PermanentLocation_Id");
            AddForeignKey("dbo.PermanentLocIORecords", "PermanentLocation_Id", "dbo.PermanentLocations", "Id");
            DropColumn("dbo.PermanentLocIORecords", "PermanentLocation");
            DropTable("dbo.RelocatingRecords");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RelocatingRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        FromLoc = c.String(),
                        ToPermenentLoc = c.String(),
                        RelocatingDate = c.DateTime(nullable: false),
                        PermanentLocation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PermanentLocIORecords", "PermanentLocation", c => c.String());
            DropForeignKey("dbo.PermanentLocIORecords", "PermanentLocation_Id", "dbo.PermanentLocations");
            DropIndex("dbo.PermanentLocIORecords", new[] { "PermanentLocation_Id" });
            DropColumn("dbo.PermanentLocIORecords", "PermanentLocation_Id");
            DropColumn("dbo.PermanentLocIORecords", "PermanentLoc");
            CreateIndex("dbo.RelocatingRecords", "PermanentLocation_Id");
            AddForeignKey("dbo.RelocatingRecords", "PermanentLocation_Id", "dbo.PermanentLocations", "Id");
        }
    }
}
