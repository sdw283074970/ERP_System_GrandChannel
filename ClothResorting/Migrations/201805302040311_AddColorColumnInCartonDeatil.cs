namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColorColumnInCartonDeatil : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Color", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "Color");
        }
    }
}
