namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPcsPerCartonInCartonBreakdown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "PcsPerCartons", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartonBreakDowns", "PcsPerCartons");
        }
    }
}
