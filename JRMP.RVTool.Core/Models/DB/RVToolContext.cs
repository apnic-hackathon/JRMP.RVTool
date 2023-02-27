using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;

namespace JRMP.RVTool.Core.Models.DB
{
    public class RVToolContext : DbContext
    {
        private readonly string _dbConnectionString;

        public RVToolContext()
        {
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings["RVTool"]?.ConnectionString))
                this._dbConnectionString = ConfigurationManager.ConnectionStrings["RVTool"].ConnectionString;
        }

        public RVToolContext(string dbConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dbConnectionString))
                throw new ArgumentException("dbConnectionString");

            this._dbConnectionString = dbConnectionString;
        }

        public RVToolContext(DbContextOptions<RVToolContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(this._dbConnectionString))
                optionsBuilder.UseSqlServer(this._dbConnectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

        }

        public virtual DbSet<AddressDelegation> AddressDelegations { get; set; }
        public virtual DbSet<AddressFamily> AddressFamilies { get; set; }
        public virtual DbSet<BGPPrefix> BGPPrefixes { get; set; }
        public virtual DbSet<BGPSource> BGPSources { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<DelegationType> DelegationTypes { get; set; }
        public virtual DbSet<RIR> RIRs { get; set; }
    }
}
