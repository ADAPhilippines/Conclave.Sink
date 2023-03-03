﻿using System;
using System.Collections.Generic;

namespace TeddySwap.Common.Models.CardanoDbSync;

public partial class ReservedPoolTicker
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] PoolHash { get; set; } = null!;
}
