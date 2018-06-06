namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableSilkIconCartonDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SilkIconCartonDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonNumberRange = c.String(),
                        Long = c.String(),
                        Width = c.String(),
                        Height = c.String(),
                        GrossWeight = c.String(),
                        NetWeight = c.String(),
                        S = c.String(),
                        M = c.String(),
                        L = c.String(),
                        XL = c.String(),
                        XXL = c.String(),
                        XXXL = c.String(),
                        Total = c.String(),
                        DistributionCenter_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DistributionCenters", t => t.DistributionCenter_Id)
                .Index(t => t.DistributionCenter_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters");
            DropIndex("dbo.SilkIconCartonDetails", new[] { "DistributionCenter_Id" });
            DropTable("dbo.SilkIconCartonDetails");
        }
    }
}
