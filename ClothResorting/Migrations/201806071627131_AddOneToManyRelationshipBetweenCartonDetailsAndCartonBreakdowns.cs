namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOneToManyRelationshipBetweenCartonDetailsAndCartonBreakdowns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "SilkIconCartonDetail_Id", c => c.Int());
            CreateIndex("dbo.CartonBreakDowns", "SilkIconCartonDetail_Id");
            AddForeignKey("dbo.CartonBreakDowns", "SilkIconCartonDetail_Id", "dbo.SilkIconCartonDetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartonBreakDowns", "SilkIconCartonDetail_Id", "dbo.SilkIconCartonDetails");
            DropIndex("dbo.CartonBreakDowns", new[] { "SilkIconCartonDetail_Id" });
            DropColumn("dbo.CartonBreakDowns", "SilkIconCartonDetail_Id");
        }
    }
}
