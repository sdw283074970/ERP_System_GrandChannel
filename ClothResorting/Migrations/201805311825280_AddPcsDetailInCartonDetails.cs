namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPcsDetailInCartonDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "ActualReceivedPcs", c => c.Int());
            AddColumn("dbo.SilkIconCartonDetails", "AvaliablePcs", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "AvaliablePcs");
            DropColumn("dbo.SilkIconCartonDetails", "ActualReceivedPcs");
        }
    }
}
