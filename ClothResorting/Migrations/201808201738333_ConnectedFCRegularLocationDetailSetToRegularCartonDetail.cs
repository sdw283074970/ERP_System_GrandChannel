namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedFCRegularLocationDetailSetToRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "RegularCaronDetail_Id", c => c.Int());
            CreateIndex("dbo.FCRegularLocationDetails", "RegularCaronDetail_Id");
            AddForeignKey("dbo.FCRegularLocationDetails", "RegularCaronDetail_Id", "dbo.RegularCartonDetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FCRegularLocationDetails", "RegularCaronDetail_Id", "dbo.RegularCartonDetails");
            DropIndex("dbo.FCRegularLocationDetails", new[] { "RegularCaronDetail_Id" });
            DropColumn("dbo.FCRegularLocationDetails", "RegularCaronDetail_Id");
        }
    }
}
