using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class DonHang
{
    public int DonHangId { get; set; }

    public int UserId { get; set; }

    public DateTime NgayDat { get; set; }

    public DateTime? NgayGiao { get; set; }

    public decimal TongTien { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

    public virtual User User { get; set; } = null!;
}
