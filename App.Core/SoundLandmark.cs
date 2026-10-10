using System;
using System.Collections.Generic;
using System.Text;

namespace App.Core
{
    public class SoundLandmark
    {
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public string AudioFile { get; set; } = string.Empty;
    }
}
