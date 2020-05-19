namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAuthAppInfo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthAppInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppKey = c.String(),
                        SecretKey = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AuthAppInfoes");
        }
    }
}
