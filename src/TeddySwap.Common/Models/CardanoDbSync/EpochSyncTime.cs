﻿using System;
using System.Collections.Generic;

namespace TeddySwap.Common.Models.CardanoDbSync;

public partial class EpochSyncTime
{
    public long Id { get; set; }

    public long No { get; set; }

    public long Seconds { get; set; }
}
