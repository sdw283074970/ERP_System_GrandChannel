namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedContainerInFCRegularLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "Container", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FCRegularLocationDetails", "Container");
        }
    }
}
