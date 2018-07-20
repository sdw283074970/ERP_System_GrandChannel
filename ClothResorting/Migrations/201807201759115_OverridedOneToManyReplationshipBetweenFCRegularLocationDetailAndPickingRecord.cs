namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OverridedOneToManyReplationshipBetweenFCRegularLocationDetailAndPickingRecord : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PickingRecords", "Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.PickingRecords", new[] { "Id" });
            DropPrimaryKey("dbo.PickingRecords");
            AddColumn("dbo.PickingRecords", "FCRegularLocationDetail_Id", c => c.Int());
            AlterColumn("dbo.PickingRecords", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.PickingRecords", "Id");
            CreateIndex("dbo.PickingRecords", "FCRegularLocationDetail_Id");
            AddForeignKey("dbo.PickingRecords", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickingRecords", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.PickingRecords", new[] { "FCRegularLocationDetail_Id" });
            DropPrimaryKey("dbo.PickingRecords");
            AlterColumn("dbo.PickingRecords", "Id", c => c.Int(nullable: false));
            DropColumn("dbo.PickingRecords", "FCRegularLocationDetail_Id");
            AddPrimaryKey("dbo.PickingRecords", "Id");
            CreateIndex("dbo.PickingRecords", "Id");
            AddForeignKey("dbo.PickingRecords", "Id", "dbo.FCRegularLocationDetails", "Id");
        }
    }
}
