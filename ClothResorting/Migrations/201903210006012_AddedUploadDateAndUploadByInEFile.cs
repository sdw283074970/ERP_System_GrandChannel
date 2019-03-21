namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUploadDateAndUploadByInEFile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EFiles", "UploadDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.EFiles", "UploadBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EFiles", "UploadBy");
            DropColumn("dbo.EFiles", "UploadDate");
        }
    }
}
