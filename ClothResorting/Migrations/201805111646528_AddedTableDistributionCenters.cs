namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableDistributionCenters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DistributionCenters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Location = c.String(),
                        Address = c.String(),
                        Address2 = c.String(),
                        City_State_Zip = c.String(),
                        CIDNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DistributionCenters");
        }
    }
}
