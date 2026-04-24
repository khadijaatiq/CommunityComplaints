using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

public partial class Announcement
{
    [Key]
    public int AnnouncementId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(2000)]
    public string Body { get; set; } = null!;

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Announcement")]
    public virtual ICollection<AnnouncementRead> AnnouncementReads { get; set; } = new List<AnnouncementRead>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Announcements")]
    public virtual User CreatedByNavigation { get; set; } = null!;
}
