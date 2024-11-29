using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class GioHang
{
    public int GioHangId { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();

    public virtual User User { get; set; } = null!;
}
