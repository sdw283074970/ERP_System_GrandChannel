namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedPOSummaryNameItself : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.RegularCartonDetails", name: "POSumary_Id", newName: "POSummary_Id");
            RenameIndex(table: "dbo.RegularCartonDetails", name: "IX_POSumary_Id", newName: "IX_POSummary_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.RegularCartonDetails", name: "IX_POSummary_Id", newName: "IX_POSumary_Id");
            RenameColumn(table: "dbo.RegularCartonDetails", name: "POSummary_Id", newName: "POSumary_Id");
        }
    }
}
