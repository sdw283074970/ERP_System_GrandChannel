namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPropertiesDistributionCenterNameInSilkIconCartionDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "DistrubutionCenterName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "DistrubutionCenterName");
        }
    }
}
