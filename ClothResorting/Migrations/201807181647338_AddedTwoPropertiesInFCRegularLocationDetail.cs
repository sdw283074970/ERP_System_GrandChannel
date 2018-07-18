    namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTwoPropertiesInFCRegularLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "PcsPerCaron", c => c.Int(nullable: false));
            AddColumn("dbo.FCRegularLocationDetails", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FCRegularLocationDetails", "Status");
            DropColumn("dbo.FCRegularLocationDetails", "PcsPerCaron");
        }
    }
}
