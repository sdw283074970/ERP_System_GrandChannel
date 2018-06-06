namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommenedOutDistributionCenterProperties : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DistributionCenters", "Location");
            DropColumn("dbo.DistributionCenters", "Address");
            DropColumn("dbo.DistributionCenters", "Address2");
            DropColumn("dbo.DistributionCenters", "City_State_Zip");
            DropColumn("dbo.DistributionCenters", "CIDNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DistributionCenters", "CIDNumber", c => c.String());
            AddColumn("dbo.DistributionCenters", "City_State_Zip", c => c.String());
            AddColumn("dbo.DistributionCenters", "Address2", c => c.String());
            AddColumn("dbo.DistributionCenters", "Address", c => c.String());
            AddColumn("dbo.DistributionCenters", "Location", c => c.String());
        }
    }
}
