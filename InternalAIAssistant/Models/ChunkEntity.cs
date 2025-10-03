using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalAIAssistant.Models;

public class ChunkEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int FileId { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public int ChunkIndex { get; set; }
    
    public int PageNumber { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Foreign key
    [ForeignKey(nameof(FileId))]
    public virtual FileEntity File { get; set; } = null!;
}