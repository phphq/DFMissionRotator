using System;

namespace DFMissionRotator.Classes
{
    public class ConsoleSpiner
    {
        private int Counter;

        public ConsoleSpiner()
        {
            Counter = 0;
        }

        public void Turn()
        {
            Counter++;

            switch (Counter % 4)
            {
                case 0: Console.Write("/"); break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }

    }

}