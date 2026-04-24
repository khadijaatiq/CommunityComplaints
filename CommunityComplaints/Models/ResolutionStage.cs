using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class ResolutionStage
{
    [Key]
    public int StageId { get; set; }

    public int ComplaintId { get; set; }

    public int StaffId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ComplaintId")]
    [InverseProperty("ResolutionStages")]
    public virtual Complaint Complaint { get; set; } = null!;

    [ForeignKey("StaffId")]
    [InverseProperty("ResolutionStages")]
    public virtual User Staff { get; set; } = null!;
}
