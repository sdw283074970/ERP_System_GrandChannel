namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedRelationshipBetweenRetrieveRecordAndPackingList : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RetrievingRecords", "CartonDetail_Id", "dbo.SilkIconCartonDetails");
            DropIndex("dbo.RetrievingRecords", new[] { "CartonDetail_Id" });
            AddColumn("dbo.RetrievingRecords", "PackingList_Id", c => c.Int());
            CreateIndex("dbo.RetrievingRecords", "PackingList_Id");
            AddForeignKey("dbo.RetrievingRecords", "PackingList_Id", "dbo.SilkIconPackingLists", "Id");
            DropColumn("dbo.RetrievingRecords", "CartonDetail_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RetrievingRecords", "CartonDetail_Id", c => c.Int());
            DropForeignKey("dbo.RetrievingRecords", "PackingList_Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.RetrievingRecords", new[] { "PackingList_Id" });
            DropColumn("dbo.RetrievingRecords", "PackingList_Id");
            CreateIndex("dbo.RetrievingRecords", "CartonDetail_Id");
            AddForeignKey("dbo.RetrievingRecords", "CartonDetail_Id", "dbo.SilkIconCartonDetails", "Id");
        }
    }
}
