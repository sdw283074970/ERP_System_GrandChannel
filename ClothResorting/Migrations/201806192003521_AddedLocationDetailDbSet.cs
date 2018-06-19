namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLocationDetailDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocationDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        NumberOfCartons = c.Int(nullable: false),
                        Pcs = c.Int(nullable: false),
                        Location = c.String(),
                        InboundDate = c.DateTime(nullable: false),
                        CartonBreakdown_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CartonBreakDowns", t => t.CartonBreakdown_Id)
                .Index(t => t.CartonBreakdown_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocationDetails", "CartonBreakdown_Id", "dbo.CartonBreakDowns");
            DropIndex("dbo.LocationDetails", new[] { "CartonBreakdown_Id" });
            DropTable("dbo.LocationDetails");
        }
    }
}
