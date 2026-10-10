using System;
using System.Collections.Generic;
using System.Text;

namespace App.Core
{
    public class Map
    {
        public string MapName { get; set; } = string.Empty;

        //Granice mapy
        public double Width { get; set; }
        public double Height { get; set; }

        //Pozycja początkowa
        public double PlayerStartX { get; set; }
        public double PlayerStartY { get; set; }

        public List<SoundLandmark> Landmarks { get; set; } = new List<SoundLandmark>();
    }
}
