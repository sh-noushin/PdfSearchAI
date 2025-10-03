using Microsoft.EntityFrameworkCore;
using PdfChunkService.Models;

namespace PdfChunkService.Data
{
    public class PdfChunkDbContext : DbContext
    {
        public PdfChunkDbContext(DbContextOptions<PdfChunkDbContext> options) : base(options)
        {
        }

        public DbSet<FileEntity> Files { get; set; }
        public DbSet<ChunkEntity> Chunks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure FileEntity
            modelBuilder.Entity<FileEntity>(entity =>
            {
                entity.HasIndex(e => e.FilePath).IsUnique();
                entity.HasIndex(e => e.FileHash);
            });

            // Configure ChunkEntity
            modelBuilder.Entity<ChunkEntity>(entity =>
            {
                entity.HasOne(c => c.File)
                      .WithMany(f => f.Chunks)
                      .HasForeignKey(c => c.FileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
