﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal static partial class Interop
{
    internal static partial class Ole32
    {
        public struct TYPEATTR
        {
            public Guid guid;
            public uint lcid;
            public uint dwReserved;
            public DispatchID memidConstructor;
            public DispatchID memidDestructor;
            public IntPtr lpstrSchema;
            public uint cbSizeInstance;
            public TYPEKIND typekind;
            public short cFuncs;
            public short cVars;
            public short cImplTypes;
            public short cbSizeVft;
            public short cbAlignment;
            public short wTypeFlags;
            public short wMajorVerNum;
            public short wMinorVerNum;
            public TYPEDESC tdescAlias;
            public IDLDESC idldescType;
        }
    }
}
