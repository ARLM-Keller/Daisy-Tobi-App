﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;

namespace Tobi.Common.UI
{
    public class KeyGestureStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                KeyGesture keyG = Convert((string)value);
                return keyG;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return Convert((KeyGesture)value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public static string Convert(Key key, ModifierKeys modKeys, bool useFriendlyKeyDisplayString)
        {
            StringBuilder strBuilder = new StringBuilder("[ ");

            bool hasModKey = false;
            if ((modKeys & ModifierKeys.Shift) != ModifierKeys.None)
            {
                strBuilder.Append("SHIFT ");
                hasModKey = true;
            }
            if ((modKeys & ModifierKeys.Control) != ModifierKeys.None)
            {
                strBuilder.Append("CTRL ");
                hasModKey = true;
            }
            if ((modKeys & ModifierKeys.Alt) != ModifierKeys.None)
            {
                strBuilder.Append("ALT ");
                hasModKey = true;
            }
            if ((modKeys & ModifierKeys.Windows) != ModifierKeys.None)
            {
                strBuilder.Append("WIN ");
                hasModKey = true;
            }
            if (!hasModKey)
            {
                strBuilder.Append("NONE ");
            }

            strBuilder.Append("] ");
            strBuilder.Append(key);

            char ch = '\0';
            if (useFriendlyKeyDisplayString
                && (ch = KeyGestureString.GetDisplayChar(key)) != '\0'
                && (key < Key.A || key > Key.Z)
                && key != Key.Space
                && key != Key.Enter
                && key != Key.Return)
            {
                strBuilder.Append(" (" + ch + ")");
            }

            return strBuilder.ToString();
        }

        public static string Convert(KeyGesture keyG)
        {
            if (keyG == null)
            {
                return null;
            }

            return Convert(keyG.Key, keyG.Modifiers, true);
        }

        public static KeyGestureString Convert(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }

            TypeConverter keyConverter = TypeDescriptor.GetConverter(typeof(Key));
            if (keyConverter == null)
            {
                return null;
            }

            int modStart = str.IndexOf("[", StringComparison.OrdinalIgnoreCase);
            if (modStart < 0 || modStart >= str.Length - 3)
            {
                return null;
            }

            int modEnd = str.IndexOf("]", StringComparison.OrdinalIgnoreCase);
            if (modEnd < 0 || modEnd <= modStart || modEnd >= str.Length - 2)
            {
                return null;
            }

            string modStr = str.Substring(modStart, modEnd - modStart);

            bool isModCtrl = modStr.IndexOf("ctrl", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isModShift = modStr.IndexOf("shift", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isModAlt = modStr.IndexOf("alt", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isModWin = modStr.IndexOf("win", StringComparison.OrdinalIgnoreCase) >= 0;

            string keyStr = str.Substring(modEnd + 1).Trim();

            int spaceIndex = keyStr.IndexOf(" ", StringComparison.OrdinalIgnoreCase);
            if (spaceIndex >= 0)
            {
                keyStr = keyStr.Substring(0, spaceIndex);
            }

            Key key = Key.None;
            try
            {
                key = (Key)keyConverter.ConvertFromString(keyStr);
            }
            catch
            {
                Console.WriteLine(@"!! invalid modifier key string: " + keyStr);
                key = Key.None;
            }
            if (key == Key.None)
            {
                return null;
            }

            ModifierKeys modKey = ModifierKeys.None;
            if (isModShift) modKey |= ModifierKeys.Shift;
            if (isModCtrl) modKey |= ModifierKeys.Control;
            if (isModAlt) modKey |= ModifierKeys.Alt;
            if (isModWin) modKey |= ModifierKeys.Windows;

            if (modKey == ModifierKeys.None)
            {
                //return null; WORKS for SOME gestures, not all (e.g. F1-F12 work fine, but not alpha keys).
            }

            try
            {
                var keyG = new KeyGestureString(key, modKey);
                return keyG;
            }
            catch
            {
                Console.WriteLine(@"!! not a valid KeyGesture: " + str);
                //Debug.Fail(@"Not a valid KeyGesture: " + str);
            }

            return null;
        }
    }
}
