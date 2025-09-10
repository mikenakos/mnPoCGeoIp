using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using mnPoCGeoIp.Models;

namespace mnPoCGeoIp.DataBase;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<geoip_lookup_batch> geoip_lookup_batches { get; set; }

    public virtual DbSet<geoip_lookup_ip> geoip_lookup_ips { get; set; }

    public virtual DbSet<geoip_lookup_ips_result> geoip_lookup_ips_results { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=mnakosPoCGeoIpDb;Trusted_Connection=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<geoip_lookup_batch>(entity =>
        {
            entity.HasKey(e => e.id).HasName("pk_geoip_lookup_batches");

            entity.HasIndex(e => e.process_ended, "ndx_proc_end");

            entity.Property(e => e.created).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<geoip_lookup_ip>(entity =>
        {
            entity.HasKey(e => new { e.glb_id, e.exec_order }).HasName("pk_geoip_lookup_ips");

            entity.Property(e => e.ip_address).HasMaxLength(45);

            entity.HasOne(d => d.glb).WithMany(p => p.geoip_lookup_ips)
                .HasForeignKey(d => d.glb_id)
                .HasConstraintName("fkd_geoip_lookup_ips_glb_id");
        });

        modelBuilder.Entity<geoip_lookup_ips_result>(entity =>
        {
            entity.HasKey(e => new { e.glb_id, e.exec_order }).HasName("pk_geoip_lookup_ips_results");

            entity.Property(e => e.country_iso3_code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.country_name).HasMaxLength(96);
            entity.Property(e => e.last_error).HasMaxLength(128);
            entity.Property(e => e.lat).HasColumnType("decimal(8, 6)");
            entity.Property(e => e.lon).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.timezone_id).HasMaxLength(64);
            entity.Property(e => e.updated).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.geoip_lookup_ip).WithOne(p => p.geoip_lookup_ips_result)
                .HasForeignKey<geoip_lookup_ips_result>(d => new { d.glb_id, d.exec_order })
                .HasConstraintName("fkd_geoip_lookup_ips_results_glb_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
