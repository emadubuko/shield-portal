﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DQA.DAL.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class shield_dmpEntities : DbContext
    {
        public shield_dmpEntities()
            : base("name=shield_dmpEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<dqa_facility_level> dqa_facility_level { get; set; }
        public virtual DbSet<dqa_facility_type> dqa_facility_type { get; set; }
        public virtual DbSet<dqa_funder> dqa_funder { get; set; }
        public virtual DbSet<dqa_indicator> dqa_indicator { get; set; }
        public virtual DbSet<dqa_lnk_version_indicators> dqa_lnk_version_indicators { get; set; }
        public virtual DbSet<dqa_report_value> dqa_report_value { get; set; }
        public virtual DbSet<dqa_summary_indicators> dqa_summary_indicators { get; set; }
        public virtual DbSet<dqa_summary_value> dqa_summary_value { get; set; }
        public virtual DbSet<dqa_versions> dqa_versions { get; set; }
        public virtual DbSet<HealthFacility> HealthFacilities { get; set; }
        public virtual DbSet<ImplementingPartner> ImplementingPartners { get; set; }
        public virtual DbSet<lga> lgas { get; set; }
        public virtual DbSet<state> states { get; set; }
        public virtual DbSet<dqa_dimensions> dqa_dimensions { get; set; }
        public virtual DbSet<dqa_comparison> dqa_comparison { get; set; }
        public virtual DbSet<dqa_pivot_table_upload> dqa_pivot_table_upload { get; set; }
        public virtual DbSet<dqa_pivot_table> dqa_pivot_table { get; set; }
        public virtual DbSet<dqa_FY17Q1_Analysyis> dqa_FY17Q1_Analysyis { get; set; }
        public virtual DbSet<dqa_report_metadata> dqa_report_metadata { get; set; }
        public virtual DbSet<dqi_report_value> dqi_report_value { get; set; }
    }
}
