﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lazarus.Models
{
    public partial class LazarusDbContext : DbContext
    {
        public LazarusDbContext()
        {
        }

        public LazarusDbContext(DbContextOptions<LazarusDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        public virtual DbSet<CuaHang> CuaHang { get; set; }
        public virtual DbSet<HoaDon> HoaDon { get; set; }
        public virtual DbSet<LoaiSanPham> LoaiSanPham { get; set; }
        public virtual DbSet<LoaiTaiKhoan> LoaiTaiKhoan { get; set; }
        public virtual DbSet<SanPham> SanPham { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoan { get; set; }
        public virtual DbSet<ThongTinGiaoHang> ThongTinGiaoHang { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChiTietHoaDon>(entity =>
            {
                entity.HasKey(e => new { e.MaHoaDon, e.MaSanPham });

                entity.Property(e => e.MaHoaDon).IsUnicode(false);

                entity.Property(e => e.MaSanPham).IsUnicode(false);

                entity.HasOne(d => d.MaHoaDonNavigation)
                    .WithMany(p => p.ChiTietHoaDon)
                    .HasForeignKey(d => d.MaHoaDon)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietHoaDon_HoaDon");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.ChiTietHoaDon)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChiTietHoaDon_SanPham");
            });

            modelBuilder.Entity<CuaHang>(entity =>
            {
                entity.Property(e => e.CuaHangId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.Property(e => e.HoaDonId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.MaTaiKhoan).IsUnicode(false);
            });

            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.Property(e => e.LoaiSanPhamId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<LoaiTaiKhoan>(entity =>
            {
                entity.Property(e => e.LoaiTaiKhoanId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.Property(e => e.SanPhamId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.MaCuaHang).IsUnicode(false);

                entity.Property(e => e.MaLoaiSanPham).IsUnicode(false);

                entity.HasOne(d => d.MaCuaHangNavigation)
                    .WithMany(p => p.SanPham)
                    .HasForeignKey(d => d.MaCuaHang)
                    .HasConstraintName("FK_SanPham_CuaHang");

                entity.HasOne(d => d.MaLoaiSanPhamNavigation)
                    .WithMany(p => p.SanPham)
                    .HasForeignKey(d => d.MaLoaiSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SanPham_LoaiSanPham");
            });

            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.Property(e => e.TaiKhoanId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.MaCuaHang).IsUnicode(false);

                entity.Property(e => e.MaLoaiTaiKhoan).IsUnicode(false);

                entity.HasOne(d => d.MaCuaHangNavigation)
                    .WithMany(p => p.TaiKhoan)
                    .HasForeignKey(d => d.MaCuaHang)
                    .HasConstraintName("FK_TaiKhoan_CuaHang");

                entity.HasOne(d => d.MaLoaiTaiKhoanNavigation)
                    .WithMany(p => p.TaiKhoan)
                    .HasForeignKey(d => d.MaLoaiTaiKhoan)
                    .HasConstraintName("FK_TaiKhoan_LoaiTaiKhoan");
            });

            modelBuilder.Entity<ThongTinGiaoHang>(entity =>
            {
                entity.Property(e => e.ThongTinId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.MaCuaHang).IsUnicode(false);

                entity.Property(e => e.MaHoaDon).IsUnicode(false);

                entity.HasOne(d => d.MaHoaDonNavigation)
                    .WithMany(p => p.ThongTinGiaoHang)
                    .HasForeignKey(d => d.MaHoaDon)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ThongTinGiaoHang_HoaDon");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}