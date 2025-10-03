using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PdfChunkService.Models
{
    public class FileEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastModified { get; set; }
        
        public long FileSize { get; set; }
        
        public string FileHash { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<ChunkEntity> Chunks { get; set; } = new List<ChunkEntity>();
    }
}
