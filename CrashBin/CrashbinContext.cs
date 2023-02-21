using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CrashBin;

public partial class CrashbinContext : DbContext
{
    public CrashbinContext()
    {
    }

    public CrashbinContext(DbContextOptions<CrashbinContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Crash> Crashes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Filename=crashbin.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
