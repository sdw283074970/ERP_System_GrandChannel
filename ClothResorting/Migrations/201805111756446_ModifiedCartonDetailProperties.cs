namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedCartonDetailProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "CartonNumberRange");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconCartonDetails", "CartonNumberRange", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeTo");
            DropColumn("dbo.SilkIconCartonDetails", "CartonNumberRangeFrom");
        }
    }
}
