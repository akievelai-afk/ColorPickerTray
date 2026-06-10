using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ColorPickerTray
{
    public readonly struct HotKeyDefinition
    {
        public ModifierKeys Modifiers { get; init; }
        public Keys Key { get; init; }
    }

    public static class HotKeyParser
    {
        public static bool TryParse(string input, out HotKeyDefinition definition)
        {
            definition = default;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var parts = input.Split(new[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var modifiers = ModifierKeys.None;
            Keys key = Keys.None;

            foreach (var rawPart in parts)
            {
                var part = rawPart.Trim();
                if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= ModifierKeys.Control;
                }
                else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= ModifierKeys.Alt;
                }
                else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= ModifierKeys.Shift;
                }
                else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase) || part.Equals("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= ModifierKeys.Windows;
                }
                else if (Enum.TryParse(part, true, out Keys parsedKey))
                {
                    key = parsedKey;
                }
                else
                {
                    return false;
                }
            }

            if (key == Keys.None)
            {
                return false;
            }

            definition = new HotKeyDefinition { Modifiers = modifiers, Key = key };
            return true;
        }
    }
}
