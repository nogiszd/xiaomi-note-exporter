﻿using System.Drawing;
using Pastel;

namespace xiaomiNoteExporter
{
    class Prompt
    {
        private readonly string _message;
        private readonly string _defaultValue;

        public Prompt(string message, string? defaultValue = "")
        {
            _message = message;
            _defaultValue = defaultValue!;
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

        public string Ask(bool isToggle = false)
        {
            Console.WriteLine(_message);

            if (isToggle)
            {
                Console.ReadKey();
                return _defaultValue;
            } else
            {
                while (true)
                {
                    var result = Console.ReadLine();

                    if (string.IsNullOrEmpty(result))
                    {
                        if (!string.IsNullOrEmpty(_defaultValue))
                        {
                            Console.WriteLine(_defaultValue.Pastel(Color.DimGray));
                            return _defaultValue;
                        } else
                        {
                            Console.Clear();
                            Console.WriteLine($"{InsertAfterSpace(_message, "valid")}");
                        }
                    } else
                    {
                        return result;
                    }
                }
            }
        }
    }
}
