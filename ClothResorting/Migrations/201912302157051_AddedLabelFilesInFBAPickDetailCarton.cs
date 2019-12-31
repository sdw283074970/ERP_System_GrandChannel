namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLabelFilesInFBAPickDetailCarton : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetailCartons", "LabelFiles", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetailCartons", "LabelFiles");
        }
    }
}
