using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class Sach
{
    public int SachId { get; set; }

    public string TenSach { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public decimal GiaTien { get; set; }

    public bool TrangThai { get; set; }

    public int? DanhMucId { get; set; }

    public int SoLuong { get; set; }

    public virtual ICollection<ComboBook> ComboBooks { get; set; } = new List<ComboBook>();

    public virtual DanhMuc? DanhMuc { get; set; }

    public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

    public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();
}
