// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace Microsoft.NodejsTools.Repl
{
    internal sealed class OutputBuffer : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private int _maxSize;
        private readonly object _lock;
        private readonly List<OutputEntry> _outputEntries = new List<OutputEntry>();
        private int _bufferLength;
        private long _lastFlush;
        private static readonly Stopwatch _stopwatch;
        private readonly ReplWindow _window;
        private const int _initialMaxSize = 1024;

        private InteractiveWindowColor _outColor = InteractiveWindowColor.Foreground;
        private InteractiveWindowColor _errColor = InteractiveWindowColor.Error;

        static OutputBuffer()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public OutputBuffer(ReplWindow window)
        {
            _maxSize = _initialMaxSize;
            _lock = new object();
            _timer = new DispatcherTimer();
            _timer.Tick += (sender, args) => Flush();
            _timer.Interval = TimeSpan.FromMilliseconds(400);
            _window = window;

            ResetColors();
        }

        public void ResetColors()
        {
            _outColor = InteractiveWindowColor.Foreground;
            _errColor = InteractiveWindowColor.Error;
        }

        public void Write(string text, bool isError = false)
        {
            bool needsFlush = false;
            lock (_lock)
            {
                int escape;
                if (ProcessAnsiEscapes && (escape = text.IndexOf('\x1b')) != -1)
                {
                    AppendEscapedText(text, isError, escape);
                }
                else
                {
                    AppendText(text, isError ? OutputEntryKind.StdErr : OutputEntryKind.StdOut, isError ? _errColor : _outColor);
                }

                _bufferLength += text.Length;
                needsFlush = (_bufferLength > _maxSize);
                if (!needsFlush && !_timer.IsEnabled)
                {
                    _timer.IsEnabled = true;
                }
            }
            if (needsFlush)
            {
                Flush();
            }
        }

        private void AppendEscapedText(string text, bool isError, int escape)
        {
            OutputEntryKind kind = isError ? OutputEntryKind.StdErr : OutputEntryKind.StdOut;
            InteractiveWindowColor color = isError ? _errColor : _outColor;

            // http://en.wikipedia.org/wiki/ANSI_escape_code
            // process any ansi color sequences...

            int start = 0;
            List<int> codes = new List<int>();
            do
            {
                if (escape != start)
                {
                    // add unescaped text
                    AppendText(text.Substring(start, escape - start), kind, color);
                }

                // process the escape sequence                
                if (escape < text.Length - 1 && text[escape + 1] == '[')
                {
                    // We have the Control Sequence Introducer (CSI) - ESC [

                    codes.Clear();
                    int? value = 0;

                    for (int i = escape + 2; i < text.Length; i++)
                    { // skip esc + [
                        if (text[i] >= '0' && text[i] <= '9')
                        {
                            // continue parsing the integer...
                            if (value == null)
                            {
                                value = 0;
                            }
                            value = 10 * value.Value + (text[i] - '0');
                        }
                        else if (text[i] == ';')
                        {
                            if (value != null)
                            {
                                codes.Add(value.Value);
                                value = null;
                            }
                            else
                            {
                                // CSI ; - invalid or CSI ### ;;, both invalid
                                break;
                            }
                        }
                        else if (text[i] == 'm')
                        {
                            if (value != null)
                            {
                                codes.Add(value.Value);
                            }

                            // parsed a valid code
                            start = i + 1;
                            if (codes.Count == 0)
                            {
                                // reset
                                color = isError ? _errColor : _outColor;
                            }
                            else
                            {
                                for (int j = 0; j < codes.Count; j++)
                                {
                                    switch (codes[j])
                                    {
                                        case 0: color = InteractiveWindowColor.White; break;
                                        case 1: // bright/bold
                                            color |= InteractiveWindowColor.DarkGray;
                                            break;
                                        case 2: // faint

                                        case 3: // italic
                                        case 4: // single underline
                                            break;
                                        case 5: // blink slow
                                        case 6: // blink fast
                                            break;
                                        case 7: // negative
                                        case 8: // conceal
                                        case 9: // crossed out
                                        case 10: // primary font
                                        case 11: // 11-19, n-th alternate font
                                            break;
                                        case 21: // bright/bold off 
                                        case 22: // normal intensity
                                            color &= ~InteractiveWindowColor.DarkGray;
                                            break;
                                        case 24: // underline off
                                            break;
                                        case 25: // blink off
                                            break;
                                        case 27: // image - postive
                                        case 28: // reveal
                                        case 29: // not crossed out
                                        case 30: color = InteractiveWindowColor.Black | (color & InteractiveWindowColor.DarkGray); break;
                                        case 31: color = InteractiveWindowColor.DarkRed | (color & InteractiveWindowColor.DarkGray); break;
                                        case 32: color = InteractiveWindowColor.DarkGreen | (color & InteractiveWindowColor.DarkGray); break;
                                        case 33: color = InteractiveWindowColor.DarkYellow | (color & InteractiveWindowColor.DarkGray); break;
                                        case 34: color = InteractiveWindowColor.DarkBlue | (color & InteractiveWindowColor.DarkGray); break;
                                        case 35: color = InteractiveWindowColor.DarkMagenta | (color & InteractiveWindowColor.DarkGray); break;
                                        case 36: color = InteractiveWindowColor.DarkCyan | (color & InteractiveWindowColor.DarkGray); break;
                                        case 37: color = InteractiveWindowColor.Gray | (color & InteractiveWindowColor.DarkGray); break;
                                        case 38: // xterm 286 background color
                                        case 39: // default text color
                                            color = _outColor;
                                            break;
                                        case 40: // background colors
                                        case 41:
                                        case 42:
                                        case 43:
                                        case 44:
                                        case 45:
                                        case 46:
                                        case 47: break;
                                        case 90: color = InteractiveWindowColor.DarkGray; break;
                                        case 91: color = InteractiveWindowColor.Red; break;
                                        case 92: color = InteractiveWindowColor.Green; break;
                                        case 93: color = InteractiveWindowColor.Yellow; break;
                                        case 94: color = InteractiveWindowColor.Blue; break;
                                        case 95: color = InteractiveWindowColor.Magenta; break;
                                        case 96: color = InteractiveWindowColor.Cyan; break;
                                        case 97: color = InteractiveWindowColor.White; break;
                                    }
                                }
                            }
                            break;
                        }
                        else
                        {
                            // unknown char, invalid escape
                            break;
                        }
                    }

                    escape = text.IndexOf('\x1b', escape + 1);
                }// else not an escape sequence, process as text
            } while (escape != -1);

            if (start != text.Length)
            {
                AppendText(text.Substring(start), kind, color);
            }
        }

        private void AppendText(string text, OutputEntryKind kind, InteractiveWindowColor color)
        {
            var newProps = new OutputEntryProperties(kind, color);
            if (_outputEntries.Count == 0 || _outputEntries[_outputEntries.Count - 1].Properties != newProps)
            {
                _outputEntries.Add(new OutputEntry(newProps));
            }
            var buffer = _outputEntries[_outputEntries.Count - 1].Buffer;
            buffer.Append(text);
        }

        public bool ProcessAnsiEscapes { get; set; }

        /// <summary>
        /// Flushes the buffer, should always be called from the UI thread.
        /// </summary>
        public void Flush()
        {
            // if we're rapidly outputting grow the output buffer.
            long curTime = _stopwatch.ElapsedMilliseconds;
            if (curTime - _lastFlush < 1000)
            {
                if (_maxSize < 1024 * 1024)
                {
                    _maxSize *= 2;
                }
            }
            _lastFlush = _stopwatch.ElapsedMilliseconds;

            OutputEntry[] entries;
            lock (_lock)
            {
                entries = _outputEntries.ToArray();

                _outputEntries.Clear();
                _bufferLength = 0;
                _timer.IsEnabled = false;
            }

            if (entries.Length > 0)
            {
                _window.AppendOutput(entries);
                _window.TextView.Caret.EnsureVisible();
            }
        }

        public void Dispose()
        {
            _timer.IsEnabled = false;
        }

        public struct OutputEntry
        {
            public readonly StringBuilder Buffer;
            public readonly OutputEntryProperties Properties;

            public OutputEntry(OutputEntryProperties properties)
            {
                Properties = properties;
                Buffer = new StringBuilder();
            }
        }

        /// <summary>
        /// Properties for a run of text - includes destination (stdout/stderr) and color
        /// </summary>
        internal struct OutputEntryProperties
        {
            public readonly OutputEntryKind Kind;
            public readonly InteractiveWindowColor Color;

            public OutputEntryProperties(OutputEntryKind kind, InteractiveWindowColor color)
            {
                Kind = kind;
                Color = color;
            }

            public static bool operator ==(OutputEntryProperties l, OutputEntryProperties r)
            {
                return l.Kind == r.Kind && l.Color == r.Color;
            }

            public static bool operator !=(OutputEntryProperties l, OutputEntryProperties r)
            {
                return l.Kind != r.Kind || l.Color != r.Color;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is OutputEntryProperties))
                {
                    return false;
                }

                var other = (OutputEntryProperties)obj;
                return other == this;
            }

            public override int GetHashCode()
            {
                return (int)Kind ^ ((int)Color) << 1;
            }
        }

        internal enum OutputEntryKind
        {
            StdOut,
            StdErr
        }
    }
}
