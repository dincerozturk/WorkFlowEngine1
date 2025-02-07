﻿using System;
using System.Collections.Generic;

namespace WFE.Core.Migration.EFCore.Models;

public partial class JobQueue
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public string Queue { get; set; }

    public DateTime? FetchedAt { get; set; }
}
