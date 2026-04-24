using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

[Index("AnnouncementId", "UserId", Name = "UQ_Announcement_User", IsUnique = true)]
public partial class AnnouncementRead
{
    [Key]
    public int Id { get; set; }

    public int AnnouncementId { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ReadAt { get; set; }

    [ForeignKey("AnnouncementId")]
    [InverseProperty("AnnouncementReads")]
    public virtual Announcement Announcement { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("AnnouncementReads")]
    public virtual User User { get; set; } = null!;
}
