namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMeasurementsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Measurements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Record = c.String(),
                        SilkIconPackingList_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconPackingLists", t => t.SilkIconPackingList_Id)
                .Index(t => t.SilkIconPackingList_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Measurements", "SilkIconPackingList_Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.Measurements", new[] { "SilkIconPackingList_Id" });
            DropTable("dbo.Measurements");
        }
    }
}
