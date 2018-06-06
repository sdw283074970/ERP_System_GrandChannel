namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRelationshipBetweenCartonBreakDownAndSilkIconPackingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "SilkIconPackingList_Id", c => c.Int());
            CreateIndex("dbo.CartonBreakDowns", "SilkIconPackingList_Id");
            AddForeignKey("dbo.CartonBreakDowns", "SilkIconPackingList_Id", "dbo.SilkIconPackingLists", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartonBreakDowns", "SilkIconPackingList_Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.CartonBreakDowns", new[] { "SilkIconPackingList_Id" });
            DropColumn("dbo.CartonBreakDowns", "SilkIconPackingList_Id");
        }
    }
}
