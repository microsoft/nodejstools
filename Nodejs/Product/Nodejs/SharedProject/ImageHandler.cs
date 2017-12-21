// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.VisualStudioTools.Project
{
    public class ImageHandler : IDisposable
    {
        private ImageList imageList;
        private List<IntPtr> iconHandles;
        private static volatile object Mutex;
        private bool isDisposed;

        /// <summary>
        /// Initializes the <see cref="RDTListener"/> class.
        /// </summary>
        static ImageHandler()
        {
            Mutex = new object();
        }

        /// <summary>
        /// Builds an ImageHandler object from a Stream providing the bitmap that
        /// stores the images for the image list.
        /// </summary>
        public ImageHandler(Stream resourceStream)
        {
            Utilities.ArgumentNotNull("resourceStream", resourceStream);
            this.imageList = Utilities.GetImageList(resourceStream);
        }

        /// <summary>
        /// Builds an ImageHandler object from an ImageList object.
        /// </summary>
        public ImageHandler(ImageList list)
        {
            Utilities.ArgumentNotNull("list", list);

            this.imageList = list;
        }

        /// <summary>
        /// Closes the ImageHandler object freeing its resources.
        /// </summary>
        public void Close()
        {
            if (null != this.iconHandles)
            {
                foreach (var hnd in this.iconHandles)
                {
                    if (hnd != IntPtr.Zero)
                    {
                        NativeMethods.DestroyIcon(hnd);
                    }
                }
                this.iconHandles = null;
            }

            if (null != this.imageList)
            {
                this.imageList.Dispose();
                this.imageList = null;
            }
        }

        /// <summary>
        /// Add an image to the ImageHandler.
        /// </summary>
        /// <param name="image">the image object to be added.</param>
        public void AddImage(Image image)
        {
            Utilities.ArgumentNotNull("image", image);
            if (null == this.imageList)
            {
                this.imageList = new ImageList();
            }
            this.imageList.Images.Add(image);
            if (null != this.iconHandles)
            {
                this.iconHandles.Add(IntPtr.Zero);
            }
        }

        /// <summary>
        /// Get or set the ImageList object for this ImageHandler.
        /// </summary>
        public ImageList ImageList
        {
            get { return this.imageList; }
            set
            {
                Close();
                this.imageList = value;
            }
        }

        /// <summary>
        /// Returns the handle to an icon build from the image of index
        /// iconIndex in the image list.
        /// </summary>
        public IntPtr GetIconHandle(int iconIndex)
        {
            Utilities.CheckNotNull(this.imageList);
            // Make sure that the list of handles is initialized.
            if (null == this.iconHandles)
            {
                InitHandlesList();
            }

            // Verify that the index is inside the expected range.
            if ((iconIndex < 0) || (iconIndex >= this.iconHandles.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(iconIndex));
            }

            // Check if the icon is in the cache.
            if (IntPtr.Zero == this.iconHandles[iconIndex])
            {
                var bitmap = this.imageList.Images[iconIndex] as Bitmap;
                // If the image is not a bitmap, then we can not build the icon,
                // so we have to return a null handle.
                if (null == bitmap)
                {
                    return IntPtr.Zero;
                }

                this.iconHandles[iconIndex] = bitmap.GetHicon();
            }

            return this.iconHandles[iconIndex];
        }

        private void InitHandlesList()
        {
            this.iconHandles = new List<IntPtr>(this.imageList.Images.Count);
            for (var i = 0; i < this.imageList.Images.Count; ++i)
            {
                this.iconHandles.Add(IntPtr.Zero);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                lock (Mutex)
                {
                    if (disposing)
                    {
                        this.imageList.Dispose();
                    }

                    this.isDisposed = true;
                }
            }
        }
    }
}
