using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBContext;

public partial class FaceContext : DbContext
{
    public FaceContext()
    {
    }

    public FaceContext(DbContextOptions<FaceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Datasource=face.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.PkClass);

            entity.ToTable("classes");

            entity.Property(e => e.PkClass).HasColumnName("PK_class");
            entity.Property(e => e.ClassName).HasColumnName("class_name");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.PkImage);

            entity.ToTable("images");

            entity.Property(e => e.PkImage).HasColumnName("PK_image");
            entity.Property(e => e.FkUser).HasColumnName("FK_user");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Images)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.PkUser);

            entity.ToTable("users");

            entity.Property(e => e.PkUser).HasColumnName("PK_user");
            entity.Property(e => e.FkClass).HasColumnName("FK_class");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Surname).HasColumnName("surname");

            entity.HasOne(d => d.FkClassNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkClass)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
