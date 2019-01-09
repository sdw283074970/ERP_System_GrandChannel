namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedFBAPalletModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPallets", "DoesAppliedLabel", c => c.Boolean(nullable: false));
            AddColumn("dbo.FBAPallets", "HasSortingMarking", c => c.Boolean(nullable: false));
            AddColumn("dbo.FBAPallets", "IsOverSizeOrOverwidth", c => c.Boolean(nullable: false));
            AddColumn("dbo.FBAPallets", "ActualPallets", c => c.Int(nullable: false));
            AddColumn("dbo.FBAPallets", "ComsumedPallets", c => c.Int(nullable: false));
            DropColumn("dbo.FBAPallets", "OriginalPallets");
            DropColumn("dbo.FBAPallets", "AvailablePalltes");
            DropColumn("dbo.FBAPallets", "PickingPallets");
            DropColumn("dbo.FBAPallets", "ShippedPallets");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAPallets", "ShippedPallets", c => c.Int(nullable: false));
            AddColumn("dbo.FBAPallets", "PickingPallets", c => c.Int(nullable: false));
            AddColumn("dbo.FBAPallets", "AvailablePalltes", c => c.Int(nullable: false));
            AddColumn("dbo.FBAPallets", "OriginalPallets", c => c.Int(nullable: false));
            DropColumn("dbo.FBAPallets", "ComsumedPallets");
            DropColumn("dbo.FBAPallets", "ActualPallets");
            DropColumn("dbo.FBAPallets", "IsOverSizeOrOverwidth");
            DropColumn("dbo.FBAPallets", "HasSortingMarking");
            DropColumn("dbo.FBAPallets", "DoesAppliedLabel");
        }
    }
}
