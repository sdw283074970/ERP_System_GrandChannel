namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OverridedOneToOneRelationshipBetweenSilkIconPackingListAndSilkIconPODetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropPrimaryKey("dbo.SilKIconPODetails");
            AlterColumn("dbo.SilKIconPODetails", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.SilKIconPODetails", "Id");
            CreateIndex("dbo.SilKIconPODetails", "Id");
            AddForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists", "Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.SilKIconPODetails", new[] { "Id" });
            DropPrimaryKey("dbo.SilKIconPODetails");
            AlterColumn("dbo.SilKIconPODetails", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.SilKIconPODetails", "Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails", "Id");
        }
    }
}
