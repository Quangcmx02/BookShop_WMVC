using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class DonHangChiTiet
{
    public int DonHangId { get; set; }

    public int? SachId { get; set; }

    public int? SoLuongSach { get; set; }

    public decimal DonGia { get; set; }

    public decimal ThanhTien { get; set; }

    public int? ComboId { get; set; }

    public int DonHangChiTietId { get; set; }

    public int? SoLuongCombo { get; set; }

    public virtual Combo? Combo { get; set; }

    public virtual DonHang DonHang { get; set; } = null!;

    public virtual Sach? Sach { get; set; }
}
