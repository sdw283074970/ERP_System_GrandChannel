namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedNameCrossReferenceFiledsName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NameCrossReferences", "StringType", c => c.String());
            AddColumn("dbo.NameCrossReferences", "OriginalString", c => c.String());
            DropColumn("dbo.NameCrossReferences", "NameType");
            DropColumn("dbo.NameCrossReferences", "NameInSystem");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NameCrossReferences", "NameInSystem", c => c.String());
            AddColumn("dbo.NameCrossReferences", "NameType", c => c.String());
            DropColumn("dbo.NameCrossReferences", "OriginalString");
            DropColumn("dbo.NameCrossReferences", "StringType");
        }
    }
}
