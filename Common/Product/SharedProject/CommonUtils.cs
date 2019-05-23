// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.VisualStudioTools
{
    internal static class CommonUtils
    {
        private static readonly char[] InvalidPathChars = GetInvalidPathChars();

        private static readonly char[] DirectorySeparators = new[] {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        private static char[] GetInvalidPathChars()
        {
            return Path.GetInvalidPathChars().Concat(new[] { '*', '?' }).ToArray();
        }

        internal static bool TryMakeUri(string path, bool isDirectory, UriKind kind, out Uri uri)
        {
            if (isDirectory && !string.IsNullOrEmpty(path) && !HasEndSeparator(path))
            {
                path += Path.DirectorySeparatorChar;
            }

            return Uri.TryCreate(path, kind, out uri);
        }

        internal static Uri MakeUri(string path, bool isDirectory, UriKind kind, string throwParameterName = "path")
        {
            try
            {
                if (isDirectory && !string.IsNullOrEmpty(path) && !HasEndSeparator(path))
                {
                    path += Path.DirectorySeparatorChar;
                }

                return new Uri(path, kind);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Path was invalid", throwParameterName, ex);
            }
        }

        /// <summary>
        /// Normalizes and returns the provided path.
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            const string MdhaPrefix = "mdha:";

            // webkit debugger prepends with 'mdha'
            if (path.StartsWith(MdhaPrefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(MdhaPrefix.Length);
            }

            return !HasStartSeparator(path) && Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Normalizes and returns the provided directory path, always
        /// ending with '/'.
        /// </summary>
        public static string NormalizeDirectoryPath(string path)
        {
            var normalizedPath = NormalizePath(path);
            return EnsureEndSeparator(normalizedPath);
        }

        /// <summary>
        /// Return true if both paths represent the same directory.
        /// </summary>
        public static bool IsSameDirectory(string path1, string path2)
        {
            if (StringComparer.Ordinal.Equals(path1, path2))
            {
                // Quick return, but will only work where the paths are already normalized and
                // have matching case.
                return true;
            }

            return
                TryMakeUri(path1, true, UriKind.Absolute, out Uri uri1) &&
                TryMakeUri(path2, true, UriKind.Absolute, out Uri uri2) &&
                uri1 == uri2;
        }

        /// <summary>
        /// Return true if both paths represent the same location.
        /// </summary>
        public static bool IsSamePath(string file1, string file2)
        {
            if (StringComparer.Ordinal.Equals(file1, file2))
            {
                // Quick return, but will only work where the paths are already normalized and
                // have matching case.
                return true;
            }

            return
                TryMakeUri(file1, false, UriKind.Absolute, out Uri uri1) &&
                TryMakeUri(file2, false, UriKind.Absolute, out Uri uri2) &&
                uri1 == uri2;
        }

        /// <summary>
        /// Return true if the path represents a file or directory contained in
        /// root or a subdirectory of root.
        /// </summary>
        public static bool IsSubpathOf(string root, string path)
        {
            if (HasEndSeparator(root) && !path.Contains("..") && path.StartsWith(root, StringComparison.Ordinal))
            {
                // Quick return, but only where the paths are already normalized and
                // have matching case.
                return true;
            }

            var uriRoot = MakeUri(root, true, UriKind.Absolute, "root");
            var uriPath = MakeUri(path, false, UriKind.Absolute, "path");

            if (uriRoot.Equals(uriPath) || uriRoot.IsBaseOf(uriPath))
            {
                return true;
            }

            // Special case where root and path are the same, but path was provided
            // without a terminating separator.
            var uriDirectoryPath = MakeUri(path, true, UriKind.Absolute, "path");
            if (uriRoot.Equals(uriDirectoryPath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a normalized directory path created by joining relativePath to root.
        /// The result is guaranteed to end with a backslash.
        /// </summary>
        /// <exception cref="ArgumentException">root is not an absolute path, or
        /// either path is invalid.</exception>
        /// <exception cref="InvalidOperationException">An absolute path cannot be
        /// created.</exception>
        public static string GetAbsoluteDirectoryPath(string root, string relativePath)
        {
            var absolutePath = GetAbsoluteFilePath(root, relativePath);
            return EnsureEndSeparator(absolutePath);
        }

        /// <summary>
        /// Returns a normalized file path created by joining relativePath to root.
        /// The result is not guaranteed to end with a backslash.
        /// </summary>
        /// <exception cref="ArgumentException">root is not an absolute path, or
        /// either path is invalid.</exception>
        public static string GetAbsoluteFilePath(string root, string relativePath)
        {
            var absolutePath = HasStartSeparator(relativePath)
                ? Path.GetFullPath(relativePath)
                : Path.Combine(root, relativePath);

            var split = absolutePath.Split(DirectorySeparators);
            var segments = new LinkedList<string>();

            for (var i = split.Length - 1; i >= 0; i--)
            {
                var segment = split[i];

                if (segment == "..")
                {
                    i--;
                }
                else if (segment != ".")
                {
                    segments.AddFirst(segment);
                }
            }

            return Path.Combine(segments.ToArray());
        }

        /// <summary>
        /// Returns a normalized file path created by joining relativePath to root.
        /// The result is not guaranteed to end with a backslash.
        /// </summary>
        /// <returns>True, if the absolute path is returned successfully; otherwise false, 
        /// if root is not an absolute path, or either path is invalid.</returns>
        public static bool TryGetAbsoluteFilePath(string root, string relativePath, out string absoluteFilePath)
        {
            try
            {
                absoluteFilePath = GetAbsoluteFilePath(root, relativePath);
                return true;
            }
            catch
            {
                absoluteFilePath = null;
                return false;
            }
        }

        /// <summary>
        /// Returns a relative path from the base path to the other path. This is
        /// intended for serialization rather than UI. See CreateFriendlyDirectoryPath
        /// for UI strings.
        /// </summary>
        /// <exception cref="ArgumentException">Either parameter was an invalid or a
        /// relative path.</exception>
        public static string GetRelativeDirectoryPath(string fromDirectory, string toDirectory)
        {
            var relativePath = GetRelativeFilePath(fromDirectory, toDirectory);
            return EnsureEndSeparator(relativePath);
        }

        /// <summary>
        /// Returns a relative path from the base path to the file. This is
        /// intended for serialization rather than UI. See CreateFriendlyFilePath
        /// for UI strings.
        /// Retunrs the file fullpath if the roots are different.
        /// </summary>
        public static string GetRelativeFilePath(string fromDirectory, string toFile)
        {
            var dirFullPath = Path.GetFullPath(TrimEndSeparator(fromDirectory));
            var fileFullPath = Path.GetFullPath(toFile);

            // If the root paths doesn't match return the file full path.
            if (!string.Equals(Path.GetPathRoot(dirFullPath), Path.GetPathRoot(fileFullPath), StringComparison.OrdinalIgnoreCase))
            {
                return fileFullPath;
            }

            var splitDirectory = dirFullPath.Split(DirectorySeparators);
            var splitFile = fileFullPath.Split(DirectorySeparators);

            var relativePath = new List<string>();
            var dirIndex = 0;

            var minLegth = Math.Min(splitDirectory.Length, splitFile.Length);

            while (dirIndex < minLegth
                && string.Equals(splitDirectory[dirIndex], splitFile[dirIndex], StringComparison.OrdinalIgnoreCase))
            {
                dirIndex++;
            }

            for (var i = splitDirectory.Length; i > dirIndex; i--)
            {
                relativePath.Add("..");
            }

            for (var i = dirIndex; i < splitFile.Length; i++)
            {
                relativePath.Add(splitFile[i]);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativePath);
        }

        /// <summary>
        /// Tries to create a friendly directory path: '.' if the same as base path,
        /// relative path if short, absolute path otherwise.
        /// </summary>
        public static string CreateFriendlyDirectoryPath(string basePath, string path)
        {
            var relativePath = GetRelativeDirectoryPath(basePath, path);

            if (relativePath.Length > 1)
            {
                relativePath = TrimEndSeparator(relativePath);
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = ".";
            }

            return relativePath;
        }

        /// <summary>
        /// Tries to create a friendly file path.
        /// </summary>
        public static string CreateFriendlyFilePath(string basePath, string path)
        {
            return GetRelativeFilePath(basePath, path);
        }

        /// <summary>
        /// Returns the last directory segment of a path. The last segment is
        /// assumed to be the string between the second-last and last directory
        /// separator characters in the path. If there is no suitable substring,
        /// the empty string is returned.
        /// 
        /// The first segment of the path is only returned if it does not
        /// contain a colon. Segments equal to "." are ignored and the preceding
        /// segment is used.
        /// </summary>
        /// <remarks>
        /// This should be used in place of:
        /// <c>Path.GetFileName(CommonUtils.TrimEndSeparator(Path.GetDirectoryName(path)))</c>
        /// </remarks>
        public static string GetLastDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var last = path.LastIndexOfAny(DirectorySeparators);

            var result = string.Empty;
            while (last > 1)
            {
                var first = path.LastIndexOfAny(DirectorySeparators, last - 1);
                if (first < 0)
                {
                    if (path.IndexOf(':') < last)
                    {
                        // Don't want to return scheme/drive as a directory
                        return string.Empty;
                    }
                    first = -1;
                }
                if (first == 1 && path[0] == path[1])
                {
                    // Don't return computer name in UNC path
                    return string.Empty;
                }

                result = path.Substring(first + 1, last - (first + 1));
                if (!string.IsNullOrEmpty(result) && result != ".")
                {
                    // Result is valid
                    break;
                }

                last = first;
            }

            return result;
        }

        /// <summary>
        /// Returns the path to the parent directory segment of a path. If the
        /// last character of the path is a directory separator, the segment
        /// prior to that character is removed. Otherwise, the segment following
        /// the last directory separator is removed.
        /// </summary>
        /// <remarks>
        /// This should be used in place of:
        /// <c>Path.GetDirectoryName(CommonUtils.TrimEndSeparator(path)) + Path.DirectorySeparatorChar</c>
        /// </remarks>
        public static string GetParent(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var last = path.Length - 1;
            if (DirectorySeparators.Contains(path[last]))
            {
                last -= 1;
            }

            if (last <= 0)
            {
                return string.Empty;
            }

            last = path.LastIndexOfAny(DirectorySeparators, last);

            if (last < 0)
            {
                return string.Empty;
            }

            return path.Remove(last + 1);
        }

        /// <summary>
        /// Returns the last segment of the path. If the last character is a
        /// directory separator, this will be the segment preceding the
        /// separator. Otherwise, it will be the segment following the last
        /// separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileOrDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var last = path.Length - 1;
            if (DirectorySeparators.Contains(path[last]))
            {
                last -= 1;
            }

            if (last < 0)
            {
                return string.Empty;
            }

            var start = path.LastIndexOfAny(DirectorySeparators, last);

            return path.Substring(start + 1, last - start);
        }

        /// <summary>
        /// Returns true if the path has a directory separator character at the end.
        /// </summary>
        public static bool HasEndSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[path.Length - 1]);
        }

        public static bool HasStartSeparator(string path)
        {
            return !string.IsNullOrEmpty(path) && DirectorySeparators.Contains(path[0]);
        }

        /// <summary>
        /// Removes up to one directory separator character from the end of path.
        /// </summary>
        public static string TrimEndSeparator(string path)
        {
            if (HasEndSeparator(path))
            {
                if (path.Length > 2 && path[path.Length - 2] == ':')
                {
                    // The slash at the end of a drive specifier is not actually
                    // a separator.
                    return path;
                }
                else if (path.Length > 3 && path[path.Length - 2] == path[path.Length - 1] && path[path.Length - 3] == ':')
                {
                    // The double slash at the end of a schema is not actually a
                    // separator.
                    return path;
                }
                return path.Remove(path.Length - 1);
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// Adds a directory separator character to the end of path if required.
        /// </summary>
        public static string EnsureEndSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            else if (!HasEndSeparator(path))
            {
                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// Removes leading @"..\" segments from a path.
        /// </summary>
        private static string TrimUpPaths(string path)
        {
            var actualStart = 0;
            while (actualStart + 2 < path.Length)
            {
                if (path[actualStart] == '.' && path[actualStart + 1] == '.' &&
                    (path[actualStart + 2] == Path.DirectorySeparatorChar || path[actualStart + 2] == Path.AltDirectorySeparatorChar))
                {
                    actualStart += 3;
                }
                else
                {
                    break;
                }
            }

            return (actualStart > 0) ? path.Substring(actualStart) : path;
        }

        /// <summary>
        /// Remove first and last quotes from path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string UnquotePath(string path)
        {
            if (path.StartsWith("\"") && path.EndsWith("\""))
            {
                return path.Substring(1, path.Length - 2);
            }
            return path;
        }

        /// <summary>
        /// Returns true if the path is a valid path, regardless of whether the
        /// file exists or not.
        /// </summary>
        public static bool IsValidPath(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                path.IndexOfAny(InvalidPathChars) < 0;
        }

        /// <summary>
        /// Gets a filename in the specified location with the specified name and extension.
        /// If the file already exist it will calculate a name with a number in it.
        /// </summary>
        public static string GetAvailableFilename(string location, string basename, string extension)
        {
            var newPath = Path.Combine(location, basename);
            var index = 0;
            if (File.Exists(newPath + extension))
            {
                string candidateNewPath;
                do
                {
                    candidateNewPath = string.Format("{0}{1}", newPath, ++index);
                } while (File.Exists(candidateNewPath + extension));
                newPath = candidateNewPath;
            }
            var final = newPath + extension;
            return final;
        }
    }
}
