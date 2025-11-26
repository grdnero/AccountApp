using System;
using System.Linq;

namespace AuthKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Auth Key Generator ===");
            while (true)
            {
                Console.WriteLine("\n1. Generate auth key");
                Console.WriteLine("2. Exit");
                Console.Write("Choice: ");

                var choice = Console.ReadLine();
                if (choice == "2") break;

                if (choice == "1")
                {
                    Console.Write("Enter recovery words (separated by commas): ");
                    var input = Console.ReadLine() ?? "";
                    var words = input.Split(',').Select(w => w.Trim()).Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
                    
                    if (!words.Any())
                    {
                        Console.WriteLine("No valid words provided!");
                        continue;
                    }

                    Console.Write("Enter login counter (0-63): ");
                    if (!int.TryParse(Console.ReadLine(), out int loginCounter) || loginCounter < 0 || loginCounter > 63)
                    {
                        Console.WriteLine("Invalid login counter!");
                        continue;
                    }

                    // Step 1: Join words
                    string combined = string.Join("", words);

                    // Step 2-4: Process ASCII values
                    var nums = combined.Select(c =>
                    {
                        int val = (int)c;
                        // Step 3: Adjust values
                        if (val >= 65 && val <= 90) val += 20;
                        else if (val >= 97 && val <= 122) val -= 64;
                        
                        // Step 4: Flip first 7 bits and add 16
                        val = (~val) & 0x7F;
                        return val + 16;
                    }).ToList();

                    // Step 5: Sum values and add login counter
                    int total = nums.Sum() + loginCounter;

                    // Step 6: Convert to 4-digit pin
                    string pin = string.Join("", total.ToString().Take(4));
                    while (pin.Length < 4) pin += "0";

                    Console.WriteLine($"Auth Key: {pin}");
                }
            }
        }
    }
}