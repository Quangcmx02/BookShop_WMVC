using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class DanhMuc
{
    public int DanhMucId { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();
}
