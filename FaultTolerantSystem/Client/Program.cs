using Client.ServiceReference;
using Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var callback = new Callback();
            var instanceContext = new InstanceContext(callback);
            Service1Client _serviceClient = new Service1Client(instanceContext);
            Services.Client client = new Services.Client(_serviceClient);
            callback.SetClient(client);
            client.Start();

            Console.WriteLine("\n╔═══════════════════════════════════════╗");
            Console.WriteLine("║     Secure Messaging Client v1.0      ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.WriteLine("\nType 'commands' to see available options\n");

            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(">> ");
                Console.ResetColor();

                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                var tokens = ParseInput(input);
                if (tokens.Length == 0)
                    continue;

                string cmd = tokens[0].ToLower();

                try
                {
                    switch (cmd)
                    {
                        case "msg":
                        case "message":
                            if (tokens.Length < 3)
                            {
                                ShowError("Usage: msg <recipient-id> <your message here>");
                                break;
                            }

                            if (Guid.TryParse(tokens[1], out Guid recipientId))
                            {
                                string content = string.Join(" ", tokens.Skip(2));
                                client.SendMessage(recipientId, content);
                                ShowSuccess($"Message sent to {recipientId}");
                            }
                            else
                            {
                                ShowError("Invalid recipient ID format");
                            }
                            break;

                        case "commands":
                        case "help":
                        case "?":
                            ShowHelp();
                            break;

                        case "clear":
                        case "cls":
                            Console.Clear();
                            break;

                        case "quit":
                        case "exit":
                        case "bye":
                            Console.WriteLine("\n[!] Disconnecting from server...");
                            _serviceClient.Close();
                            running = false;
                            break;

                        case "info":
                            client.ShowClientInfo();
                            break;

                        default:
                            ShowError($"Unknown command: '{cmd}'. Type 'commands' for help");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Operation failed: {ex.Message}");
                }
            }

            Console.WriteLine("\n[✓] Client terminated successfully\n");
        }

        static string[] ParseInput(string input)
        {
            return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        static void ShowHelp()
        {
            Console.WriteLine("\n┌─────────────────────────────────────────────────────┐");
            Console.WriteLine("│                  AVAILABLE COMMANDS                 │");
            Console.WriteLine("├─────────────────────────────────────────────────────┤");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("│  msg <id> <text>      Send a message                │");
            Console.WriteLine("│  info                 Show client information        │");
            Console.WriteLine("│  clear                Clear the console              │");
            Console.WriteLine("│  commands             Show this help menu            │");
            Console.WriteLine("│  exit                 Disconnect and quit            │");
            Console.ResetColor();
            Console.WriteLine("└─────────────────────────────────────────────────────┘\n");
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[✗] {message}");
            Console.ResetColor();
        }

        static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[✓] {message}");
            Console.ResetColor();
        }

        static void ShowWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] {message}");
            Console.ResetColor();
        }
    }
}