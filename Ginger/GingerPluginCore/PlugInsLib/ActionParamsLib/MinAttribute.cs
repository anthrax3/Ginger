﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinAttribute : Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Min";

        public int MinValue { get; set; }

        public MinAttribute(int min)
        {
            MinValue = min; 
        }

        public MinAttribute()
        {
        }
    }
}
