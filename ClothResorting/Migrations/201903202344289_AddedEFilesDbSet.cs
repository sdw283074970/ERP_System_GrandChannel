namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEFilesDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        Path = c.String(),
                        FBAMasterOrder_Id = c.Int(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAMasterOrders", t => t.FBAMasterOrder_Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBAMasterOrder_Id)
                .Index(t => t.FBAShipOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EFiles", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropForeignKey("dbo.EFiles", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.EFiles", new[] { "FBAShipOrder_Id" });
            DropIndex("dbo.EFiles", new[] { "FBAMasterOrder_Id" });
            DropTable("dbo.EFiles");
        }
    }
}
