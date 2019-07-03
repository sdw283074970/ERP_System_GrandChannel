namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNameCrossReferencesTableAndRemovedUselessTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NameCrossReferences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NameType = c.String(),
                        NameInSystem = c.String(),
                        Synonym = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.CartonInsides");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CartonInsides",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.NameCrossReferences");
        }
    }
}
