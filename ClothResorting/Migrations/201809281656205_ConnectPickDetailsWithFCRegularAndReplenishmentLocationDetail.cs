namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectPickDetailsWithFCRegularAndReplenishmentLocationDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CartonInsides", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.CartonInsides", new[] { "FCRegularLocationDetail_Id" });
            AddColumn("dbo.PickDetails", "FCRegularLocationDetail_Id", c => c.Int());
            AddColumn("dbo.PickDetails", "ReplenishmentLocationDetail_Id", c => c.Int());
            CreateIndex("dbo.PickDetails", "FCRegularLocationDetail_Id");
            CreateIndex("dbo.PickDetails", "ReplenishmentLocationDetail_Id");
            AddForeignKey("dbo.PickDetails", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails", "Id");
            AddForeignKey("dbo.PickDetails", "ReplenishmentLocationDetail_Id", "dbo.ReplenishmentLocationDetails", "Id");
            DropColumn("dbo.CartonInsides", "FCRegularLocationDetail_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CartonInsides", "FCRegularLocationDetail_Id", c => c.Int());
            DropForeignKey("dbo.PickDetails", "ReplenishmentLocationDetail_Id", "dbo.ReplenishmentLocationDetails");
            DropForeignKey("dbo.PickDetails", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.PickDetails", new[] { "ReplenishmentLocationDetail_Id" });
            DropIndex("dbo.PickDetails", new[] { "FCRegularLocationDetail_Id" });
            DropColumn("dbo.PickDetails", "ReplenishmentLocationDetail_Id");
            DropColumn("dbo.PickDetails", "FCRegularLocationDetail_Id");
            CreateIndex("dbo.CartonInsides", "FCRegularLocationDetail_Id");
            AddForeignKey("dbo.CartonInsides", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails", "Id");
        }
    }
}
