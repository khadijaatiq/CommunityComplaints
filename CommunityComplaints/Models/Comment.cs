using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class Comment
{
    [Key]
    public int CommentId { get; set; }

    public int ComplaintId { get; set; }

    public int UserId { get; set; }

    [StringLength(1000)]
    public string Message { get; set; } = null!;

    public bool IsInternal { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ComplaintId")]
    [InverseProperty("Comments")]
    public virtual Complaint Complaint { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Comments")]
    public virtual User User { get; set; } = null!;
}
