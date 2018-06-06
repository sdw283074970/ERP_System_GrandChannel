namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationInSilkIconCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Location", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "Location");
        }
    }
}
