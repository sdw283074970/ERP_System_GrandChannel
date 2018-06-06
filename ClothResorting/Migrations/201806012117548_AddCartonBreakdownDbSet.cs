namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCartonBreakdownDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartonBreakDowns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseNumber = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        CartonNumberRangeFrom = c.Int(),
                        CartonNumberRangeTo = c.Int(),
                        Size = c.String(),
                        ForecastPcs = c.Int(),
                        ActualPcs = c.Int(),
                        AvailablePcs = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CartonBreakDowns");
        }
    }
}
