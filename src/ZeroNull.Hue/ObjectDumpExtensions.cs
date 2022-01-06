using Newtonsoft.Json.Linq;

namespace ZeroNull.Hue
{
    /// <summary>
    /// Dumps objects to the console in a (perhaps) pleasant manner
    /// </summary>
    internal static class ObjectDumpExtensions
    {

        public static void DumpHeading(this string heading, ConsoleColor color = ConsoleColor.Green)
        {
            Console.WriteLine();
            Console.ForegroundColor = color;
            Console.WriteLine(heading);
            Console.ForegroundColor = color;
            Console.WriteLine(string.Empty.PadLeft(heading.Length, '='));
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Dump(this object obj, string label = null, int indentLevel = 0, ConsoleColor color = ConsoleColor.Cyan)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                Console.Write(string.Empty.PadLeft(indentLevel, ' '));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(label);
                Console.Write(": ");
            }

            var token = JToken.FromObject(obj);
            DumpToken(indentLevel - 1, token, color);
            Console.WriteLine();
        }

        public static void DumpValue(this object value, string label = null, int indentLevel = 0, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.Write(string.Empty.PadLeft(indentLevel, ' '));
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!string.IsNullOrEmpty(label))
            {
                Console.Write(label);
                Console.Write(": ");
            }
            Console.ForegroundColor = color;
            Console.Write(value.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        private static void DumpToken(int indentLevel, JToken token, ConsoleColor color = ConsoleColor.Cyan)
        {
            switch (token)
            {
                case JObject obj:
                    // Output all properties of the object
                    foreach (var prop in obj.Properties())
                    {
                        DumpToken(indentLevel + 1, prop);
                    }
                    break;

                case JProperty prop:
                    Console.Write(string.Empty.PadLeft(indentLevel, ' '));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(prop.Name);
                    Console.Write(": ");

                    // Are we going to write a value, or something more complex?
                    if (prop.Value is JValue)
                    {
                        RenderValue(prop.Value as JValue, color);
                        Console.WriteLine();
                    }
                    else
                    {
                        // Next line...
                        Console.WriteLine();
                        DumpToken(indentLevel + 1, prop.Value, color);
                    }
                    break;

                case JArray array:
                    int index = 0;
                    foreach (var item in array)
                    {
                        Console.Write(string.Empty.PadLeft(indentLevel + 1, ' '));
                        Console.Write($"[{index}]: ");
                        // If the token is anything other than a value, step into it
                        if (item is JValue)
                        {
                            RenderValue(item as JValue, color);
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine();
                            DumpToken(indentLevel + 2, item, color);
                        }
                        index++;
                    }
                    break;

                case JValue value:
                    RenderValue(value, color);
                    break;
            }
        }

        public static void RenderValue(this string value, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.Write(value.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void RenderValue(this JValue value, ConsoleColor color = ConsoleColor.Cyan)
        {
            RenderValue(value.ToString(), color);
        }
    }
}
