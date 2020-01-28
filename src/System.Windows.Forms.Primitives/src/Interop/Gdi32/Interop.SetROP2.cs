﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static partial class Interop
{
    internal static partial class Gdi32
    {
        [DllImport(Libraries.Gdi32, ExactSpelling = true)]
        public static extern Gdi32.R2 SetROP2(IntPtr hdc, R2 rop2);

        public static Gdi32.R2 SetROP2(IHandle hdc, R2 rop2)
        {
            Gdi32.R2 result = SetROP2(hdc.Handle, rop2);
            GC.KeepAlive(hdc);
            return result;
        }
    }
}
