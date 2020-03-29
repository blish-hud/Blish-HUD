/*
 *  This code is heavily adapted from the Myra UndoRedoRecord (https://github.com/rds1983/Myra/blob/a9dbf7a1ceedc19f9e416c754eaf38e89a89a746/src/Myra/Graphics2D/UI/TextEdit/UndoRedoRecord.cs)
 *
 *  MIT License
 *
 *  Copyright (c) 2017-2020 The Myra Team
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.

 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Resources {
	internal enum OperationType {
        Insert,
        Delete,
        Replace
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UndoRedoRecord {
        public OperationType OperationType;
        public string        Data;
        public int           Index;
        public int           Length;
    }
}
