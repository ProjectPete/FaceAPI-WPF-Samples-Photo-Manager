//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClientLibrary.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class PictureFileGroupLookup
    {
        public int PictureFileId { get; set; }
        public string LargePersonGroupId { get; set; }
        public int ProcessingState { get; set; }
    
        public virtual PictureFile PictureFile { get; set; }
    }
}
