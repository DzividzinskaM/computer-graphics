﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    interface IImageReader
    {
        public Image Read(string filePath);
    }
}
