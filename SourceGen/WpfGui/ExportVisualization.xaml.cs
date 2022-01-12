/*
 * Copyright 2020 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using CommonUtil;
using CommonWPF;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Export an image from the visualization editor.
    /// </summary>
    public partial class ExportVisualization : Window, INotifyPropertyChanged {
        private Visualization mVis;
        private WireframeObject mWireObj;
        private string mFileNameBase;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsBitmap {
            get { return mIsBitmap; }
            set { mIsBitmap = value; OnPropertyChanged(); }
        }
        public bool IsWireframe {
            get { return !mIsBitmap; }
        }
        private bool mIsBitmap;

        /// <summary>
        /// Item for output size combo box.
        /// </summary>
        public class OutputSize {
            public int Width { get; private set; }
            public int Height { get; private set; }
            public override string ToString() {
                return Width + "x" + Height;
            }

            public OutputSize(int width, int height) {
                Width = width;
                Height = height;
            }
        }

        /// <summary>
        /// List of output sizes, for combo box.
        /// </summary>
        public List<OutputSize> OutputSizeList { get; private set; }


        public ExportVisualization(Window owner, Visualization vis, WireframeObject wireObj,
                string fileNameBase) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mVis = vis;
            mWireObj = wireObj;
            mFileNameBase = fileNameBase;

            OutputSizeList = new List<OutputSize>();

            // Normally, bitmap and wireframe visualizations don't really differ, because
            // we're just working off the cached image rendering.  It matters for us though,
            // so we need to see if a wireframe-only parameter exists.
            bool isWireframe = (vis is VisWireframeAnimation) ||
                vis.VisGenParams.ContainsKey(VisWireframeAnimation.P_IS_ANIMATED);
            IsBitmap = !isWireframe;

            if (isWireframe) {
                int dim = 64;
                while (dim <= 1024) {
                    OutputSizeList.Add(new OutputSize(dim, dim));
                    dim *= 2;
                }
            } else {
                int baseWidth = (int)vis.CachedImage.Width;
                int baseHeight = (int)vis.CachedImage.Height;
                // ensure there's at least one entry, then add other options
                OutputSizeList.Add(new OutputSize(baseWidth, baseHeight));
                int mult = 2;
                while (baseWidth * mult < 2048 && baseHeight * mult < 2048) {
                    OutputSizeList.Add(new OutputSize(baseWidth * mult, baseHeight * mult));
                    mult *= 2;
                }
            }

            sizeComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = Res.Strings.FILE_FILTER_GIF + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,
                FileName = mFileNameBase + ".gif"
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);
            Debug.WriteLine("Save path: " + pathName);

            try {
                OutputSize item = (OutputSize)sizeComboBox.SelectedItem;
                if (mVis is VisWireframeAnimation) {
                    Debug.Assert(item.Width == item.Height);
                    AnimatedGifEncoder encoder = new AnimatedGifEncoder();
                    ((VisWireframeAnimation)mVis).EncodeGif(encoder, item.Width);

                    using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                        encoder.Save(stream, out int dispWidth, out int dispHeight);
                    }
                } else {
                    BitmapSource outImage;
                    if (IsBitmap) {
                        int scale = item.Width / (int)mVis.CachedImage.Width;
                        Debug.Assert(scale >= 1);
                        if (scale == 1) {
                            outImage = mVis.CachedImage;
                        } else {
                            outImage = mVis.CachedImage.CreateScaledCopy(scale);
                        }
                    } else {
                        Debug.Assert(item.Width == item.Height);
                        outImage = Visualization.GenerateWireframeImage(mWireObj,
                            item.Width, mVis.VisGenParams);
                    }

                    GifBitmapEncoder encoder = new GifBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(outImage));

#if false
                    // try feeding the GIF into our GIF unpacker
                    using (MemoryStream ms = new MemoryStream()) {
                        encoder.Save(ms);
                        Debug.WriteLine("TESTING");
                        UnpackedGif anim = UnpackedGif.Create(ms.GetBuffer());
                        anim.DebugDump();
                    }
#else
                    using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                        encoder.Save(stream);
                    }
#endif
                }
            } catch (Exception ex) {
                // Error handling is a little sloppy, but this shouldn't fail often.
                MessageBox.Show(ex.Message, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // After successful save, close dialog box.
            DialogResult = true;
        }
    }
}
