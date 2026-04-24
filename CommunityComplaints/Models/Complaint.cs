using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class Complaint
{
    [Key]
    public int ComplaintId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string Category { get; set; } = null!;

    [StringLength(20)]
    public string Urgency { get; set; } = null!;

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public int ResidentId { get; set; }

    public int? DepartmentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ResolvedAt { get; set; }

    [InverseProperty("Complaint")]
    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    [InverseProperty("Complaint")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Complaints")]
    public virtual Department? Department { get; set; }

    [InverseProperty("Complaint")]
    public virtual Rating? Rating { get; set; }

    [ForeignKey("ResidentId")]
    [InverseProperty("Complaints")]
    public virtual User Resident { get; set; } = null!;

    [InverseProperty("Complaint")]
    public virtual ICollection<ResolutionStage> ResolutionStages { get; set; } = new List<ResolutionStage>();
}
