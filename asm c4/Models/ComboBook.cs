using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class ComboBook
{
    public int ComboBooksId { get; set; }

    public int ComboId { get; set; }

    public int SachId { get; set; }

    public virtual Combo Combo { get; set; } = null!;

    public virtual ICollection<ComboBookDetail> ComboBookDetails { get; set; } = new List<ComboBookDetail>();

    public virtual Sach Sach { get; set; } = null!;
}
