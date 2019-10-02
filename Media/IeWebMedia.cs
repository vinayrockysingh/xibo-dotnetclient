﻿/**
 * Copyright (C) 2019 Xibo Signage Ltd
 *
 * Xibo - Digital Signage - http://www.xibo.org.uk
 *
 * This file is part of Xibo.
 *
 * Xibo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * Xibo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with Xibo.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XiboClient
{
    class IeWebMedia : WebMedia
    {
        private bool _disposed;

        private WebBrowser _webBrowser;

        public IeWebMedia(RegionOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Render Media
        /// </summary>
        public override void RenderMedia()
        {
            // Create the web view we will use
            _webBrowser = new WebBrowser();
            _webBrowser.DocumentCompleted += _webBrowser_DocumentCompleted;
            _webBrowser.Size = Size;
            _webBrowser.ScrollBarsEnabled = false;
            _webBrowser.ScriptErrorsSuppressed = true;
            _webBrowser.Visible = false;

            HtmlUpdatedEvent += IeWebMedia_HtmlUpdatedEvent;

            if (IsNativeOpen())
            {
                // Navigate directly
                _webBrowser.Navigate(_filePath);
            }
            else if (HtmlReady())
            {
                // Write to temporary file
                ReadControlMeta();

                // Navigate to temp file
                _webBrowser.Navigate(_localWebPath);
            }
            else
            {
                Debug.WriteLine("HTML Resource is not ready to be shown (meaning the file doesn't exist at all) - wait for the download the occur and then show");
            }

            Controls.Add(_webBrowser);

            // Render media shows the controls and starts timers, etc
            base.RenderMedia();
        }

        /// <summary>
        /// Web Browser finished loading document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            DocumentCompleted();

            if (!IsDisposed)
            {
                // Show the browser
                _webBrowser.Visible = true;
            }
        }

        private void IeWebMedia_HtmlUpdatedEvent(string url)
        {
            if (_webBrowser != null)
            {
                _webBrowser.Navigate(url);
            }
        }

        /// <summary>
        /// Dispose of this text item
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Debug.WriteLine("Disposing of " + _filePath, "IeWebMedia - Dispose");

            if (disposing)
            {
                // Remove the webbrowser control
                try
                {
                    // Remove the web browser control
                    Controls.Remove(_webBrowser);

                    // Workaround to remove COM object
                    PerformLayout();

                    // Detatch event and remove
                    if (_webBrowser != null && !_disposed)
                    {
                        _webBrowser.DocumentCompleted -= _webBrowser_DocumentCompleted;
                        _webBrowser.Navigate("about:blank");
                        _webBrowser.Dispose();

                        _disposed = true;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(new LogMessage("IeWebMedia - Dispose", "Cannot dispose of web browser. E = " + e.Message), LogType.Info.ToString());
                }
            }

            base.Dispose(disposing);
        }
    }
}
