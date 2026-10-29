using System;
using System.Collections.Generic;
using System.Security.Principal;
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

        //Lista punktów dźwiękowych na mapie
        public List<SoundLandmark> Landmarks { get; set; } = new List<SoundLandmark>();

        //Lista ścian (przeszkody blokujące ruch i tłumiące dźwięk)
        public List<Wall> Walls { get; set; } = new List<Wall>();
    }
}
