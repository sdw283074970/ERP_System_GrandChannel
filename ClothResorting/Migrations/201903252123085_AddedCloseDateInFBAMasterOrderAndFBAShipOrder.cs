namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCloseDateInFBAMasterOrderAndFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EFiles", "RootPath", c => c.String());
            DropColumn("dbo.EFiles", "Path");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EFiles", "Path", c => c.String());
            DropColumn("dbo.EFiles", "RootPath");
        }
    }
}
