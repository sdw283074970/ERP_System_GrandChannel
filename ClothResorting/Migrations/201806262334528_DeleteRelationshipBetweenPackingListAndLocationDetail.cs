namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteRelationshipBetweenPackingListAndLocationDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationDetails", "PackingList_Id", "dbo.PackingLists");
            DropIndex("dbo.LocationDetails", new[] { "PackingList_Id" });
            DropColumn("dbo.LocationDetails", "PackingList_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LocationDetails", "PackingList_Id", c => c.Int());
            CreateIndex("dbo.LocationDetails", "PackingList_Id");
            AddForeignKey("dbo.LocationDetails", "PackingList_Id", "dbo.PackingLists", "Id");
        }
    }
}
