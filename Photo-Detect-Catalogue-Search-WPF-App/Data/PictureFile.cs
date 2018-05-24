namespace Photo_Detect_Catalogue_Search_WPF_App.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PictureFile")]
    public partial class PictureFile
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime? DateTaken { get; set; }

        public bool IsConfirmed { get; set; }

        public virtual ICollection<PicturePerson> PicturePersons { get; set; }

        public virtual ICollection<PictureFileGroupLookup> PictureFileGroupLookups { get; set; }

    }
}
