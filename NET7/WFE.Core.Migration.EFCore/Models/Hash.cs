﻿using System;
using System.Collections.Generic;

namespace WFE.Core.Migration.EFCore.Models;

public partial class Hash
{
    public int Id { get; set; }

    public string Key { get; set; }

    public string Field { get; set; }

    public string Value { get; set; }

    public DateTime? ExpireAt { get; set; }
}
