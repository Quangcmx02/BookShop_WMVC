using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class Combo
{
    public int ComboId { get; set; }

    public string TenCombo { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal Gia { get; set; }

    public bool TrangThai { get; set; }

    public string? LinkImages { get; set; }

    public int? Quantity { get; set; }

    public virtual ICollection<ComboBook> ComboBooks { get; set; } = new List<ComboBook>();

    public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

    public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();
}
