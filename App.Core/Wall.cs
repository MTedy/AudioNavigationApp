using System;
using System.Collections.Generic;
using System.Text;

namespace App.Core
{
    public class Wall
    {
        //Współrzędne początku i końca ściany (odcinek kolizji)
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
    }
}
