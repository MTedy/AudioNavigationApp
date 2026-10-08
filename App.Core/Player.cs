using System;
using System.Collections.Generic;
using System.Text;

namespace App.Core
{
    public class Player
    {

        //Pozycja X i Y (środek mapy 200, 200
        public double X { get; set; } = 200;
        public double Y { get; set; } = 200;

        //Kąt obrotu w stopniach
        public double Rotation { get; set; } = 0;
    }
}
