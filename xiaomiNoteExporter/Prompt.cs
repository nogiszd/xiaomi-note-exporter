using System.Drawing;
using Pastel;

namespace xiaomiNoteExporter
{
    class Prompt
    {
        private readonly string _message;

        public Prompt(string message)
        {
            _message= message;
        }

       private static string InsertAfterSpace(string str, string insertion, Color? color = null)
        {
            var spaceIndex = str.IndexOf(' ');

            if (spaceIndex > 0)
            {
                return str.Insert(spaceIndex, $" {insertion.Pastel(color ?? Color.Red)}");
            }
            else
            {
                return str;
            }
        }

        public string Ask()
        {
            Console.WriteLine(_message);

            while(true)
            {
                var result = Console.ReadLine();

                if (string.IsNullOrEmpty(result))
                {
                    Console.Clear();
                    Console.WriteLine($"{InsertAfterSpace(_message, "valid")}");
                }
                else
                {
                    return result;
                }
            }
        }
    }
}
