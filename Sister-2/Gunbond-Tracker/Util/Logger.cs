using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gunbond_Tracker.Util
{
    public static class Logger
    {
        private static bool _Active = false;
        public static bool Active
        {
            get { return _Active; }
            set { _Active = value; }
        }

        //
        // Summary:
        //     Writes the text representation of the specified Boolean value to the standard
        //     output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(bool value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the specified Unicode character value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(char value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the specified array of Unicode characters to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     A Unicode character array.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(char[] buffer)
        {
            if (Active) Console.Write(buffer);
        }

        //
        // Summary:
        //     Writes the text representation of the specified System.Decimal value to the
        //     standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(decimal value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified double-precision floating-point
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(double value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified single-precision floating-point
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(float value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 32-bit signed integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(int value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 64-bit signed integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(long value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified object to the standard output
        //     stream.
        //
        // Parameters:
        //   value:
        //     The value to write, or null.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(object value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the specified string value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(string value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 32-bit unsigned integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(uint value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 64-bit unsigned integer value
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(ulong value)
        {
            if (Active) Console.Write(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified object to the standard output
        //     stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     An object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void Write(string format, object arg0)
        {
            if (Active) Console.Write(format, arg0);
        }

        //
        // Summary:
        //     Writes the text representation of the specified array of objects to the standard
        //     output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format or arg is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void Write(string format, params object[] arg)
        {
            if (Active) Console.Write(format, arg);
        }

        //
        // Summary:
        //     Writes the specified subarray of Unicode characters to the standard output
        //     stream.
        //
        // Parameters:
        //   buffer:
        //     An array of Unicode characters.
        //
        //   index:
        //     The starting position in buffer.
        //
        //   count:
        //     The number of characters to write.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is less than zero.
        //
        //   System.ArgumentException:
        //     index plus count specify a position that is not within buffer.
        //
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void Write(char[] buffer, int index, int count)
        {
            if (Active) Console.Write(buffer, index, count);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects, followed by the
        //     current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void Write(string format, object arg0, object arg1)
        {
            if (Active) Console.Write(format, arg0, arg1);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects to the standard output
        //     stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void Write(string format, object arg0, object arg1, object arg2)
        {
            if (Active) Console.Write(format, arg0, arg1, arg2);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects and variable-length
        //     parameter list to the standard output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        //   arg3:
        //     The fourth object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void Write(string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (Active) Console.Write(format, arg0, arg1, arg2, arg3);
        }


        //
        // Summary:
        //     Writes the current line terminator to the standard output stream.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine()
        {
            if (Active) Console.WriteLine();
        }

        //
        // Summary:
        //     Writes the text representation of the specified Boolean value, followed by
        //     the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(bool value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the specified Unicode character, followed by the current line terminator,
        //     value to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(char value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the specified array of Unicode characters, followed by the current
        //     line terminator, to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     A Unicode character array.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(char[] buffer)
        {
            if (Active) Console.WriteLine(buffer);
        }

        //
        // Summary:
        //     Writes the text representation of the specified System.Decimal value, followed
        //     by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(decimal value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified double-precision floating-point
        //     value, followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(double value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified single-precision floating-point
        //     value, followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(float value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 32-bit signed integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(int value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 64-bit signed integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(long value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified object, followed by the current
        //     line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(object value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator,
        //     to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(string value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 32-bit unsigned integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(uint value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified 64-bit unsigned integer value,
        //     followed by the current line terminator, to the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(ulong value)
        {
            if (Active) Console.WriteLine(value);
        }

        //
        // Summary:
        //     Writes the text representation of the specified object, followed by the current
        //     line terminator, to the standard output stream using the specified format
        //     information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     An object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLine(string format, object arg0)
        {
            if (Active) Console.WriteLine(format, arg0);
        }

        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed
        //     by the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format or arg is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLine(string format, params object[] arg)
        {
            if (Active) Console.WriteLine(format, arg);
        }

        //
        // Summary:
        //     Writes the specified subarray of Unicode characters, followed by the current
        //     line terminator, to the standard output stream.
        //
        // Parameters:
        //   buffer:
        //     An array of Unicode characters.
        //
        //   index:
        //     The starting position in buffer.
        //
        //   count:
        //     The number of characters to write.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is less than zero.
        //
        //   System.ArgumentException:
        //     index plus count specify a position that is not within buffer.
        //
        //   System.IO.IOException:
        //     An I/O error occurred.
        public static void WriteLine(char[] buffer, int index, int count)
        {
            if (Active) Console.WriteLine(buffer, index, count);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects, followed by the
        //     current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLine(string format, object arg0, object arg1)
        {
            if (Active) Console.WriteLine(format, arg0, arg1);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects, followed by the
        //     current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (Active) Console.WriteLine(format, arg0, arg1, arg2);
        }

        //
        // Summary:
        //     Writes the text representation of the specified objects and variable-length
        //     parameter list, followed by the current line terminator, to the standard
        //     output stream using the specified format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg0:
        //     The first object to write using format.
        //
        //   arg1:
        //     The second object to write using format.
        //
        //   arg2:
        //     The third object to write using format.
        //
        //   arg3:
        //     The fourth object to write using format.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        //
        //   System.ArgumentNullException:
        //     format is null.
        //
        //   System.FormatException:
        //     The format specification in format is invalid.
        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (Active) Console.WriteLine(format, arg0, arg1, arg2, arg3);
        }

    }
}
