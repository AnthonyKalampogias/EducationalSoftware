﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SoftwareEduEntities : DbContext
    {
        public SoftwareEduEntities()
            : base("name=SoftwareEduEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Tests> Tests { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<FinalsScores> FinalsScores { get; set; }
        public virtual DbSet<Scores> Scores { get; set; }
        public virtual DbSet<Chapters> Chapters { get; set; }
        public virtual DbSet<Content> Content { get; set; }
        public virtual DbSet<UserVisits> UserVisits { get; set; }
    }
}
