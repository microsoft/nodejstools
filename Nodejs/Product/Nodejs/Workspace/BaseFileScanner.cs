// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Indexing;

namespace Microsoft.NodejsTools.Workspace
{
    internal abstract class BaseFileScanner : IFileScanner, IFileScannerUpToDateCheck
    {
        protected readonly IWorkspace workspace;

        protected BaseFileScanner(IWorkspace workspaceContext)
        {
            this.workspace = workspaceContext;
        }

        protected string EnsureRooted(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException($"{nameof(filePath)} should not be null or empty.", nameof(filePath));
            }

            return this.workspace.MakeRooted(filePath);
        }

        public virtual async Task<bool> IsUpToDateAsync(DateTimeOffset? lastScanTimestamp, string filePath, FileScannerType scannerType, CancellationToken cancellationToken)
        {
            if (await this.IsValidFileAsync(filePath))
            {
                try
                {
                    var lastWrite = File.GetLastWriteTimeUtc(filePath);
                    return lastScanTimestamp.HasValue && lastWrite < lastScanTimestamp.Value.UtcDateTime;
                }
                catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
                {
                    // We have already loaded the file in VS,
                    // so any I/O related exceptions are very unlikely
                    // and we def. don't want to crash VS on that.
                }
            }

            return false;
        }

        protected abstract Task<bool> IsValidFileAsync(string filePath);

        public async Task<T> ScanContentAsync<T>(string filePath, CancellationToken cancellationToken) where T : class
        {
            if (await this.IsValidFileAsync(filePath))
            {
                if (typeof(T) == FileScannerTypeConstants.FileReferenceInfoType)
                {
                    return await this.ComputeFileReferencesAsync(filePath, cancellationToken) as T;
                }
                if (typeof(T) == FileScannerTypeConstants.FileDataValuesType)
                {
                    return await this.ComputeFileDataValuesAsync(filePath, cancellationToken) as T;
                }
            }

            return this.GetDefaultScanValue<T>();
        }

        protected abstract Task<List<FileReferenceInfo>> ComputeFileReferencesAsync(string filePath, CancellationToken cancellationToken);

        protected abstract Task<List<FileDataValue>> ComputeFileDataValuesAsync(string filePath, CancellationToken cancellationToken);

        protected T GetDefaultScanValue<T>() where T : class
        {
            // We can't actually return null, this will crash the Index service
            if (typeof(T) == FileScannerTypeConstants.FileDataValuesType)
            {
                return Array.Empty<FileDataValue>() as T;
            }
            if (typeof(T) == FileScannerTypeConstants.FileReferenceInfoType)
            {
                return Array.Empty<FileReferenceInfo>() as T;
            }

            // This situation should never happen, since we set the supported filetypes
            // in the ExportFileScannerAttribute on the DerivedScannerFactory class.
            throw new NotImplementedException($"Unexpected Type requested '{typeof(T)}'.");
        }
    }
}
