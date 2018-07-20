namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OverridedOneToOneReplationshipBetweenFCRegularLocationDetailAndPickingRecord : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.PickingRecords");
            AlterColumn("dbo.PickingRecords", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PickingRecords", "Id");
            CreateIndex("dbo.PickingRecords", "Id");
            AddForeignKey("dbo.PickingRecords", "Id", "dbo.FCRegularLocationDetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickingRecords", "Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.PickingRecords", new[] { "Id" });
            DropPrimaryKey("dbo.PickingRecords");
            AlterColumn("dbo.PickingRecords", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.PickingRecords", "Id");
        }
    }
}
