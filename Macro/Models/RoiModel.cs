﻿using System;
using Utils.Infrastructure;

namespace Macro.Models
{
    [Serializable]
    public class RoiModel
    {
        public Rectangle RoiRect { get; set; }
        public MonitorInfo MonitorInfo { get; set; }
    }
}
