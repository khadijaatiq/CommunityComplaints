using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CommunityComplaints.Models;

[Index("Name", Name = "UQ__Departme__737584F62F67EDAE", IsUnique = true)]
public partial class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [InverseProperty("Department")]
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
