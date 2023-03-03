﻿using System;
using System.Collections.Generic;

namespace TeddySwap.Common.Models.CardanoDbSync;

public partial class TxIn
{
    public long Id { get; set; }

    public long TxInId { get; set; }

    public long TxOutId { get; set; }

    public short TxOutIndex { get; set; }

    public long? RedeemerId { get; set; }

    public virtual Redeemer? Redeemer { get; set; }

    public virtual Tx TxInNavigation { get; set; } = null!;

    public virtual Tx TxOut { get; set; } = null!;
}
