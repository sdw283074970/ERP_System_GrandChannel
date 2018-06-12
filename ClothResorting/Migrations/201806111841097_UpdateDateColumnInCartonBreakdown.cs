namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDateColumnInCartonBreakdown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartonBreakDowns", "ReceivedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartonBreakDowns", "ReceivedDate");
        }
    }
}
