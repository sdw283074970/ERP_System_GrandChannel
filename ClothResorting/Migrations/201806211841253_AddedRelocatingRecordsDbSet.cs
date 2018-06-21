namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRelocatingRecordsDbSet : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PermanentLocations", t => t.PermanentLocation_Id)
                .Index(t => t.PermanentLocation_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RelocatingRecords", "PermanentLocation_Id", "dbo.PermanentLocations");
            DropIndex("dbo.RelocatingRecords", new[] { "PermanentLocation_Id" });
            DropTable("dbo.RelocatingRecords");
        }
    }
}
