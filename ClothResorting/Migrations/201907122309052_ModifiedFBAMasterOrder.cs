namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "UnloadFinishTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "IsDamaged", c => c.String());
            DropColumn("dbo.FBAMasterOrders", "UnloadTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAMasterOrders", "UnloadTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.FBAMasterOrders", "IsDamaged");
            DropColumn("dbo.FBAMasterOrders", "UnloadFinishTime");
        }
    }
}
