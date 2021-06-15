// original source: https://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser

using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CommandLine
{
    public static class Arguments
    {

        public static bool TryGetOptions(string[] args, bool inConsole, out string mode, out ushort port, out bool https)
        {
            var arguments = Parse(args);
            var validArgs = true;

            mode = arguments["m"] ?? arguments["mode"] ?? "start";
            https = arguments["http"] == null && arguments["https"] != null;
            var portString = arguments["p"] ?? arguments["port"] ?? "8080";

            if (mode != "start" && mode != "attach" && mode != "kill")
            {
                if (inConsole)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid mode; Allowed values are start | attach | kill");
                    Console.ResetColor();
                }
                validArgs = false;
            }

            if (!ushort.TryParse(portString, out port) || port < 80)
            {
                if (inConsole)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid port number specified.");
                    Console.ResetColor();
                }
                validArgs = false;
            }

            if (arguments["h"] != null || arguments["help"] != null) validArgs = false;

            if (inConsole && !validArgs)
            {
                Console.WriteLine();
                Console.WriteLine("  Mode Argument Options (Defaults to start)");
                Console.WriteLine("     -m | --mode start -> Start the SPA server and proxy to that.");
                Console.WriteLine("     -m | --mode attach -> Attach to existing SPA server");
                Console.WriteLine("     -m | --mode kill -> Shutdown any existing SPA server on the specified port (used after debugging in VS Code)");
                Console.WriteLine();
                Console.WriteLine("  Port Argument (Defaults to 8080)");
                Console.WriteLine("     -p | --port 8080 -> Specify what port to start or attach to, minimum of 80");
                Console.WriteLine();
                Console.WriteLine("  HTTPS (Defaults to false)");
                Console.WriteLine("     -https -> Uses HTTPS");
                Console.WriteLine();

            }

            return validArgs;

        }

        public static StringDictionary Parse(string[] args)
        {
            var parameters = new StringDictionary();
            Regex splitter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples:
            // -param1 value1 --param2 /param3:"Test-:-work"
            //   /param4=happy -param5 '--=nice=--'
            foreach (string txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = splitter.Split(txt, 3);

                switch (parts.Length)
                {
                    // Found a value (for the last parameter
                    // found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parts[0] =
                                    remover.Replace(parts[0], "$1");

                                parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting.
                        // With no value, set it to true.
                        if (parameter != null && !parameters.ContainsKey(parameter))
                            parameters.Add(parameter, "true");

                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting.
                        // With no value, set it to true.
                        if (parameter != null && !parameters.ContainsKey(parameter))
                            parameters.Add(parameter, "true");

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!parameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            parameters.Add(parameter, parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null && !parameters.ContainsKey(parameter))
                parameters.Add(parameter, "true");

            return parameters;
        }
    }
}