﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FluidicsSDK
{
    public interface IRenderable
    {
        void Render(Graphics g, int alpha, float scale);
    }
}
