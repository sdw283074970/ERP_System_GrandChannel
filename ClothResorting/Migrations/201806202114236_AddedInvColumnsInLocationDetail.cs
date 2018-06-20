namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInvColumnsInLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LocationDetails", "OrgNumberOfCartons", c => c.Int(nullable: false));
            AddColumn("dbo.LocationDetails", "InvNumberOfCartons", c => c.Int(nullable: false));
            AddColumn("dbo.LocationDetails", "OrgPcs", c => c.Int(nullable: false));
            AddColumn("dbo.LocationDetails", "InvPcs", c => c.Int(nullable: false));
            DropColumn("dbo.LocationDetails", "NumberOfCartons");
            DropColumn("dbo.LocationDetails", "Pcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LocationDetails", "Pcs", c => c.Int(nullable: false));
            AddColumn("dbo.LocationDetails", "NumberOfCartons", c => c.Int(nullable: false));
            DropColumn("dbo.LocationDetails", "InvPcs");
            DropColumn("dbo.LocationDetails", "OrgPcs");
            DropColumn("dbo.LocationDetails", "InvNumberOfCartons");
            DropColumn("dbo.LocationDetails", "OrgNumberOfCartons");
        }
    }
}
