﻿using System;
using System.Collections.Generic;

namespace TeddySwap.Common.Models.CardanoDbSync;

public partial class StakeDeregistration
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public int CertIndex { get; set; }

    public int EpochNo { get; set; }

    public long TxId { get; set; }

    public long? RedeemerId { get; set; }

    public virtual StakeAddress Addr { get; set; } = null!;

    public virtual Redeemer? Redeemer { get; set; }

    public virtual Tx Tx { get; set; } = null!;
}
