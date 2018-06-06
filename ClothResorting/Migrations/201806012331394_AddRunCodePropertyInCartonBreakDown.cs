namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRunCodePropertyInCartonBreakDown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "RunCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartonBreakDowns", "RunCode");
        }
    }
}
