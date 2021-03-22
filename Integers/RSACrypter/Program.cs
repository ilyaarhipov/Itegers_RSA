using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Integers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Enter command");
            var line = Console.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                switch (line.Trim())
                {
                    case "encode":
                        Encode();
                        break;
                    case "decode":
                        Decode();
                        break;
                    default:
                        Console.WriteLine("Available commands is {decode} and {encode}");
                        break;
                }

                Console.WriteLine($"Enter command");
                line = Console.ReadLine();
            }
        }

        private static void Encode()
        {
            Console.Write("Enter first prime number: ");
            var p = new Integer(Console.ReadLine());
            Console.Write("Enter second prime number: ");
            var q = new Integer(Console.ReadLine());
            var module = p * q;
            Console.WriteLine($"Your module is {module}");
            var phi = (p - Integer.One) * (q - Integer.One);
            var publicExponent = RSA.CalculatePublicExponent(phi);
            var secretExponent = RSA.CalculateSecretExponent(publicExponent, phi);
            Console.WriteLine("Enter your message:");
            var message = Encoding.ASCII
                                  .GetBytes(Console.ReadLine() ?? string.Empty)
                                  .Select(x => (int) x)
                                  .ToArray();
            var encode = RSA.Encode(message, publicExponent, module);
            Console.WriteLine("Your crypted data:");
            foreach (var integer in encode)
                Console.WriteLine(integer);
            Console.WriteLine($"Your secret exponent : {secretExponent}");
        }

        private static void Decode()
        {
            Console.WriteLine("Enter your secret exponent");
            var secretExponent = new Integer(Console.ReadLine());
            Console.WriteLine("Enter your module");
            var module = new Integer(Console.ReadLine());
            Console.WriteLine("Enter your crypted data per line");
            var crypted = new List<Integer>();
            var line = Console.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                crypted.Add(new Integer(line));
                line = Console.ReadLine();
            }

            var chars = RSA.Decode(crypted, secretExponent, module);
            var result = Encoding.ASCII.GetString(chars.Select(x => (byte) x).ToArray());
            Console.WriteLine($"Your message was {result}");
        }
    }
}