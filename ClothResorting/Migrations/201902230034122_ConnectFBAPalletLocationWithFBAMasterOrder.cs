namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectFBAPalletLocationWithFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPalletLocations", "FBAMasterOrder_Id", c => c.Int());
            CreateIndex("dbo.FBAPalletLocations", "FBAMasterOrder_Id");
            AddForeignKey("dbo.FBAPalletLocations", "FBAMasterOrder_Id", "dbo.FBAMasterOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAPalletLocations", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.FBAPalletLocations", new[] { "FBAMasterOrder_Id" });
            DropColumn("dbo.FBAPalletLocations", "FBAMasterOrder_Id");
        }
    }
}
