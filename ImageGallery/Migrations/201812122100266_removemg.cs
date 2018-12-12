namespace ImageGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removemg : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Photos", "ImagePath", c => c.String());
            AddColumn("dbo.Photos", "ThumbPath", c => c.String());
            DropColumn("dbo.Photos", "Data");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Photos", "Data", c => c.Binary());
            DropColumn("dbo.Photos", "ThumbPath");
            DropColumn("dbo.Photos", "ImagePath");
        }
    }
}
