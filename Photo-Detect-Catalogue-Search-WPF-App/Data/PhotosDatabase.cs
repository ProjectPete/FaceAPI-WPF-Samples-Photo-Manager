namespace Photo_Detect_Catalogue_Search_WPF_App.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PhotosDatabase : DbContext
    {
        public PhotosDatabase()
            : base("name=PhotosDatabase")
        {
        }

        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PictureFile> PictureFiles { get; set; }
        public virtual DbSet<PictureFileGroupLookup> PictureFileGroupLookups { get; set; }
        public virtual DbSet<PicturePerson> PicturePersons { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PictureFileGroupLookup>()
                .Property(e => e.LargePersonGroupId)
                .IsUnicode(false);
        }
    }
}
