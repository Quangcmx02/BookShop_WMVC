using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public string? Email { get; set; }

    public int? Phone { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
