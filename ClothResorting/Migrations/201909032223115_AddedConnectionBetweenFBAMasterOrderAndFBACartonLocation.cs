namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedConnectionBetweenFBAMasterOrderAndFBACartonLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "FBAMasterOrder_Id", c => c.Int());
            CreateIndex("dbo.FBACartonLocations", "FBAMasterOrder_Id");
            AddForeignKey("dbo.FBACartonLocations", "FBAMasterOrder_Id", "dbo.FBAMasterOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBACartonLocations", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.FBACartonLocations", new[] { "FBAMasterOrder_Id" });
            DropColumn("dbo.FBACartonLocations", "FBAMasterOrder_Id");
        }
    }
}
