namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCartonBreakdownOutBoundsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartonBreakdownOutbounds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PickPurchaseNumber = c.String(),
                        TimeOfOutbound = c.DateTime(nullable: false),
                        Pcs = c.Int(nullable: false),
                        CartonBreakdown_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CartonBreakDowns", t => t.CartonBreakdown_Id)
                .Index(t => t.CartonBreakdown_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartonBreakdownOutbounds", "CartonBreakdown_Id", "dbo.CartonBreakDowns");
            DropIndex("dbo.CartonBreakdownOutbounds", new[] { "CartonBreakdown_Id" });
            DropTable("dbo.CartonBreakdownOutbounds");
        }
    }
}
