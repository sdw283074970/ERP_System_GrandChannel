namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedPickDetailProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickDetails", "PcsPerCarton", c => c.Int(nullable: false));
            DropColumn("dbo.PickDetails", "Cartons");
            DropColumn("dbo.PickDetails", "Quantity");
            DropColumn("dbo.PickDetails", "PcsPerCaron");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PickDetails", "PcsPerCaron", c => c.Int(nullable: false));
            AddColumn("dbo.PickDetails", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.PickDetails", "Cartons", c => c.Int(nullable: false));
            DropColumn("dbo.PickDetails", "PcsPerCarton");
        }
    }
}
