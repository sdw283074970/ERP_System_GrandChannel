namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualPltsInFBAPickDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetails", "ActualPlts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "ActualPlts");
        }
    }
}
