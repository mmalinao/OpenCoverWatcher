using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Console
{
    public static class ConsoleArgs
    {
        [Required]
        public static readonly string TargetDir = "-targetdir";

        [Optional]
        public static readonly string OutputDir = "-outputdir";

        [Optional]
        public static readonly string BuildConfig = "-buildconfig";

        [Optional]
        public static readonly string Help = "-?";

        public static IEnumerable<string> All()
        {
            return typeof(ConsoleArgs)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null));
        }

        public static IEnumerable<string> AllRequired()
        {
            return typeof(ConsoleArgs)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string)
                    && f.GetCustomAttributes(typeof(Required), false).Length > 0)
                .Select(f => (string)f.GetValue(null));
        }

        public static IEnumerable<string> AllOptional()
        {
            return typeof(ConsoleArgs)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string)
                    && f.GetCustomAttributes(typeof(Optional), false).Length > 0)
                .Select(f => (string)f.GetValue(null));
        }

    }

    #region Custom Attributes
    public class Required : Attribute
    {
    }

    public class Optional : Attribute
    {
    }
    #endregion
}
