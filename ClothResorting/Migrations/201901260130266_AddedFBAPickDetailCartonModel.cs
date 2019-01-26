namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAPickDetailCartonModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAPickDetailCartons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PickCtns = c.Int(nullable: false),
                        FBACartonLocation_Id = c.Int(),
                        FBAPickDetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBACartonLocations", t => t.FBACartonLocation_Id)
                .ForeignKey("dbo.FBAPickDetails", t => t.FBAPickDetail_Id)
                .Index(t => t.FBACartonLocation_Id)
                .Index(t => t.FBAPickDetail_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAPickDetailCartons", "FBAPickDetail_Id", "dbo.FBAPickDetails");
            DropForeignKey("dbo.FBAPickDetailCartons", "FBACartonLocation_Id", "dbo.FBACartonLocations");
            DropIndex("dbo.FBAPickDetailCartons", new[] { "FBAPickDetail_Id" });
            DropIndex("dbo.FBAPickDetailCartons", new[] { "FBACartonLocation_Id" });
            DropTable("dbo.FBAPickDetailCartons");
        }
    }
}
