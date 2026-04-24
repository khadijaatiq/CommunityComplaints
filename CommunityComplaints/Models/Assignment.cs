using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    public int ComplaintId { get; set; }

    public int StaffId { get; set; }

    public int AssignedBy { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AssignedAt { get; set; }

    [ForeignKey("AssignedBy")]
    [InverseProperty("AssignmentAssignedByNavigations")]
    public virtual User AssignedByNavigation { get; set; } = null!;

    [ForeignKey("ComplaintId")]
    [InverseProperty("Assignments")]
    public virtual Complaint Complaint { get; set; } = null!;

    [ForeignKey("StaffId")]
    [InverseProperty("AssignmentStaffs")]
    public virtual User Staff { get; set; } = null!;
}
