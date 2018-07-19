namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCartonInsides : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartonInsides",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        FCRegularLocationDetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FCRegularLocationDetails", t => t.FCRegularLocationDetail_Id)
                .Index(t => t.FCRegularLocationDetail_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartonInsides", "FCRegularLocationDetail_Id", "dbo.FCRegularLocationDetails");
            DropIndex("dbo.CartonInsides", new[] { "FCRegularLocationDetail_Id" });
            DropTable("dbo.CartonInsides");
        }
    }
}
