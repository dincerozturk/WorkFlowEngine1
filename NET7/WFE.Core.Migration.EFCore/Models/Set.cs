﻿using System;
using System.Collections.Generic;

namespace WFE.Core.Migration.EFCore.Models;

public partial class Set
{
    public int Id { get; set; }

    public string Key { get; set; }

    public double Score { get; set; }

    public string Value { get; set; }

    public DateTime? ExpireAt { get; set; }
}
