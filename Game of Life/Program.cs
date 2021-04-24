using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Game_of_Life
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }
}
