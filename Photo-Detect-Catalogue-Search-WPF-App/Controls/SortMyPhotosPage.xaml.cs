﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Face-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// SCENARIO: Photo Manager. Identify and tag photos automatically.
//
// This scenario combines several Face API features into a fully working example
// of a photo store management tool. With this tool, you can catalog all your
// photos and holiday snaps. Once the AI service is sufficiently trained, the
// hope is that it can identify and tag up to 80% of images. The tool also 
// enables you to show all photos containing a certain person.
// 

namespace Photo_Detect_Catalogue_Search_WPF_App.Controls
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SortMyPhotosPage.xaml
    /// </summary>
    public partial class SortMyPhotosPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortMyPhotosPage"/> class.
        /// </summary>
        public SortMyPhotosPage()
        {
            InitializeComponent();
        }
    }
}
