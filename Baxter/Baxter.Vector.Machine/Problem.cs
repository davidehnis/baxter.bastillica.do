﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    public class Problem
    {
        public int L { get; set; }

        public double[] Y { get; set; }

        public Node[][] X { get; set; }
    }
}