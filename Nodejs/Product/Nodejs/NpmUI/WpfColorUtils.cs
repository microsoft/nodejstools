/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Windows.Media;

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Useful functionality for handling and manipulating colors.
    /// </summary>
    public sealed class WpfColorUtils {
        private static readonly Random _sRandom = new Random();

        private WpfColorUtils() { }

        /// <summary>
        /// Creates a color that is a mixture of the two specified colors in the
        /// proportions specified.
        /// </summary>
        /// <param name="color1">First color.</param>
        /// <param name="color2">Second color.</param>
        /// <param name="parts1">Number of parts of first color to be mixed.</param>
        /// <param name="parts2">Number of parts of second color to be mixed.</param>
        /// <returns>Mixed color.</returns>
        public static Color Mix(
            Color color1,
            Color color2,
            int parts1,
            int parts2) {
            if (parts1 < 0) {
                throw new ArgumentOutOfRangeException(
                    "parts1",
                    parts1,
                    string.Format(
                        "{0} is not a valid value for parts1.",
                        parts1));
            } else if (parts2 < 0) {
                throw new ArgumentOutOfRangeException(
                    "parts2",
                    parts2,
                    string.Format(
                        "{0} is not a valid value for parts2.",
                        parts2));
            }
            int total = parts1 + parts2;
            if (0 == total) {   //  Should never happen
                total = 2;
            }
            int alpha = (color1.A * parts1 + color2.A * parts2) / total;
            if (alpha < 0) {
                alpha = 0;
            }
            return Color.FromArgb(
                (byte) alpha,
                (byte) Math.Min((color1.R * parts1 + color2.R * parts2) / total, 255),
                (byte) Math.Min((color1.G * parts1 + color2.G * parts2) / total, 255),
                (byte) Math.Min((color1.B * parts1 + color2.B * parts2) / total, 255));
        }

        public static Color GetRandomColor() {
            return Color.FromArgb(
                255,
                (byte) Math.Min(_sRandom.Next(256), 255),
                (byte) Math.Min(_sRandom.Next(256), 255),
                (byte) Math.Min(_sRandom.Next(256), 255));
        }

        public static Color GetRandomNormalisedColor() {
            return Normalise(GetRandomColor());
        }

        public static Color Normalise(Color source) {
            int max = Math.Max(Math.Max(source.R, source.G), source.B);
            float factor = 255F / max;
            return Color.FromArgb(
                source.A,
                (byte) Math.Min((int)(source.R * factor), 255),
                (byte) Math.Min((int)(source.G * factor), 255),
                (byte) Math.Min((int)(source.B * factor), 255));
        }

        /// <summary>
        /// Creates a color that is a mixture of the two specified colors in the
        /// proportions specified.
        /// </summary>
        /// <param name="color1">First color.</param>
        /// <param name="color2">Second color.</param>
        /// <param name="parts1">Number of parts of first color to be mixed.</param>
        /// <param name="parts2">Number of parts of second color to be mixed.</param>
        /// <returns>Mixed color.</returns>
        public static Color MixAndNormalise(
            Color color1,
            Color color2,
            int parts1,
            int parts2) {
            return Normalise(Mix(color1, color2, parts1, parts2));
        }

        /// <summary>
        /// Gets a color that is represents an equal mixture of the A, R, G,
        /// and B components of the supplied color.
        /// </summary>
        /// <param name="color1">First color.</param>
        /// <param name="color2">Second color.</param>
        /// <returns>Color halfway between the two supplied color.</returns>
        public static Color MidPoint(Color color1, Color color2) {
            return Color.FromArgb(
                (byte) Math.Min((color1.A + color2.A) / 2, 255),
                (byte) Math.Min((color1.R + color2.R) / 2, 255),
                (byte) Math.Min((color1.G + color2.G) / 2, 255),
                (byte) Math.Min((color1.B + color2.B) / 2, 255));
        }

        /// <summary>
        /// Gets a color that is halfway between the supplied color and white.
        /// Preserves any alpha value in supplied color.
        /// </summary>
        /// <param name="source">Source color.</param>
        /// <returns>Color halfway between supplied color and white.</returns>
        public static Color Lighter(Color source) {
            return MidPoint(
                source,
                Color.FromArgb(
                    source.A,
                    Colors.White.R,
                    Colors.White.G,
                    Colors.White.B));
        }

        /// <summary>
        /// Gets a color that is halfway between the supplied color and black.
        /// Preserves any alpha value in supplied color.
        /// </summary>
        /// <param name="source">Source color.</param>
        /// <returns>Color halfway between supplied color and white.</returns>
        public static Color Darker(Color source) {
            return MidPoint(
                source,
                Color.FromArgb(
                    source.A,
                    Colors.Black.R,
                    Colors.Black.G,
                    Colors.Black.B));
        }
    }
}