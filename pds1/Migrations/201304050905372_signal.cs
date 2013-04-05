namespace pds1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class signal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Measures", "signal", c => c.Int(nullable: false));
            AlterColumn("dbo.Measures", "strenght", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Measures", "strenght", c => c.String());
            AlterColumn("dbo.Measures", "signal", c => c.String());
        }
    }
}
