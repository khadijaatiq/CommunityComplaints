using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

[Index("ComplaintId", Name = "UQ__Ratings__740D898E9829F971", IsUnique = true)]
public partial class Rating
{
    [Key]
    public int RatingId { get; set; }

    public int ComplaintId { get; set; }

    public int ResidentId { get; set; }

    public int Stars { get; set; }

    [StringLength(500)]
    public string? Feedback { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ComplaintId")]
    [InverseProperty("Rating")]
    public virtual Complaint Complaint { get; set; } = null!;

    [ForeignKey("ResidentId")]
    [InverseProperty("Ratings")]
    public virtual User Resident { get; set; } = null!;
}
