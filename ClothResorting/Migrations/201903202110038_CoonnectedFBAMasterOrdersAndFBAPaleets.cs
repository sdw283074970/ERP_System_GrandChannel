namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CoonnectedFBAMasterOrdersAndFBAPaleets : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPallets", "FBAMasterOrder_Id", c => c.Int());
            CreateIndex("dbo.FBAPallets", "FBAMasterOrder_Id");
            AddForeignKey("dbo.FBAPallets", "FBAMasterOrder_Id", "dbo.FBAMasterOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAPallets", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.FBAPallets", new[] { "FBAMasterOrder_Id" });
            DropColumn("dbo.FBAPallets", "FBAMasterOrder_Id");
        }
    }
}
