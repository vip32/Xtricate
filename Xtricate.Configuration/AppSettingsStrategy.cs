using System;
using System.Linq;

namespace Xtricate.Configuration
{
    public static class AppSettingsStrategy
    {
        public static string CollapseNewLines(string originalSetting)
        {
            if (originalSetting == null) return null;

            var lines = originalSetting.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length > 1
                ? string.Join("", lines.Select(x => x.Trim()))
                : originalSetting;
        }
    }
}