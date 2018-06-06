namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLicationInCartonBreakDown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "Location", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartonBreakDowns", "Location");
        }
    }
}
