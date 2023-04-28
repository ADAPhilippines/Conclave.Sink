using System;
using System.Collections.Generic;

namespace TeddySwap.Common.Models.CardanoDbSync;

public partial class DelistedPool
{
    public long Id { get; set; }

    public byte[] HashRaw { get; set; } = null!;
}
