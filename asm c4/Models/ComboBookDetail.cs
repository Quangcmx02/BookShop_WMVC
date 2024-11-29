using System;
using System.Collections.Generic;

namespace asm_c4.Models;

public partial class ComboBookDetail
{
    public int ComboBookDetailsId { get; set; }

    public int QuantityBookInCombo { get; set; }

    public int ComboBooksId { get; set; }

    public virtual ComboBook ComboBooks { get; set; } = null!;
}
