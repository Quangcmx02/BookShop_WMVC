using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace asm_c4.Models;

public partial class QuanLySachContext : DbContext
{
    public QuanLySachContext()
    {
    }

    public QuanLySachContext(DbContextOptions<QuanLySachContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<ComboBook> ComboBooks { get; set; }

    public virtual DbSet<ComboBookDetail> ComboBookDetails { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonHangChiTiet> DonHangChiTiets { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }

    public virtual DbSet<Sach> Saches { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-BLDH9R40;Database=QuanLySach;Trusted_Connection=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.ComboId).HasName("PK__Combos__DD42582E716C493F");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TenCombo).HasMaxLength(255);
        });

        modelBuilder.Entity<ComboBook>(entity =>
        {
            entity.HasKey(e => e.ComboBooksId).HasName("PK__ComboBoo__E02744EFDCC4CDCF");

            entity.Property(e => e.ComboBooksId).HasColumnName("ComboBooksID");
            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.SachId).HasColumnName("SachID");

            entity.HasOne(d => d.Combo).WithMany(p => p.ComboBooks)
                .HasForeignKey(d => d.ComboId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ComboBook__Combo__03F0984C");

            entity.HasOne(d => d.Sach).WithMany(p => p.ComboBooks)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ComboBook__SachI__04E4BC85");
        });

        modelBuilder.Entity<ComboBookDetail>(entity =>
        {
            entity.HasKey(e => e.ComboBookDetailsId).HasName("PK__ComboBoo__E5E4E9100B51F660");

            entity.Property(e => e.ComboBookDetailsId).HasColumnName("ComboBookDetailsID");

            entity.HasOne(d => d.ComboBooks).WithMany(p => p.ComboBookDetails)
                .HasForeignKey(d => d.ComboBooksId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComboBooks_ComboBookDetails");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.DanhMucId).HasName("PK__DanhMuc__1C53BA7B46AFEE20");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.DonHangId).HasName("PK__DonHang__D159F4BED4067BC2");

            entity.ToTable("DonHang");

            entity.Property(e => e.NgayDat).HasColumnType("datetime");
            entity.Property(e => e.NgayGiao).HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__UserId__49C3F6B7");
        });

        modelBuilder.Entity<DonHangChiTiet>(entity =>
        {
            entity.ToTable("DonHangChiTiet");

            entity.Property(e => e.DonHangChiTietId).HasColumnName("DonHangChiTietID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Combo).WithMany(p => p.DonHangChiTiets)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK_DonHangChiTiet_Combos");

            entity.HasOne(d => d.DonHang).WithMany(p => p.DonHangChiTiets)
                .HasForeignKey(d => d.DonHangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHangCh__DonHa__4CA06362");

            entity.HasOne(d => d.Sach).WithMany(p => p.DonHangChiTiets)
                .HasForeignKey(d => d.SachId)
                .HasConstraintName("FK__DonHangCh__SachI__4D94879B");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.GioHangId).HasName("PK__GioHang__4242280D067C5924");

            entity.ToTable("GioHang");

            entity.Property(e => e.GioHangId).HasColumnName("GioHangID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GioHang__UserID__3B75D760");
        });

        modelBuilder.Entity<GioHangChiTiet>(entity =>
        {
            entity.ToTable("GioHangChiTiet");

            entity.Property(e => e.GioHangChiTietId).HasColumnName("GioHangChiTietID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GioHangId).HasColumnName("GioHangID");
            entity.Property(e => e.SachId).HasColumnName("SachID");

            entity.HasOne(d => d.Combo).WithMany(p => p.GioHangChiTiets)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK_GioHangChiTiet_Combos");

            entity.HasOne(d => d.GioHang).WithMany(p => p.GioHangChiTiets)
                .HasForeignKey(d => d.GioHangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GioHangCh__GioHa__3E52440B");

            entity.HasOne(d => d.Sach).WithMany(p => p.GioHangChiTiets)
                .HasForeignKey(d => d.SachId)
                .HasConstraintName("FK__GioHangCh__SachI__3F466844");
        });

        modelBuilder.Entity<Sach>(entity =>
        {
            entity.HasKey(e => e.SachId).HasName("PK__Sach__F3005E3AB0B5438E");

            entity.ToTable("Sach");

            entity.Property(e => e.SachId).HasColumnName("SachID");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenSach).HasMaxLength(100);

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.Saches)
                .HasForeignKey(d => d.DanhMucId)
                .HasConstraintName("FK_Sach_DanhMuc");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC11528375");

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
