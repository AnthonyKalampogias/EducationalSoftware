//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EducationalSoftware.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tests
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        public string Question { get; set; }
        public string answerA { get; set; }
        public string answerB { get; set; }
        public string answerC { get; set; }
    
        public virtual Content Content { get; set; }
    }
}
