﻿//
//  CodeTable.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Xdelta.Instructions;

namespace Xdelta
{
    public class CodeTable
    {
        private Instruction[,] instructions;

        public CodeTable(byte[] table, byte nearSlots, byte sameSlots)
        {
            Cache = new Cache(nearSlots, sameSlots);
            Initialize(table);
        }

        public Cache Cache {
            get;
            private set;
        }

        public static CodeTable Default {
            get { return new CodeTable(DefaultTable, DefaultNearSlots, DefaultSameSlots); }
        }

        public Instruction[] GetInstructions(byte index)
        {
            return new Instruction[] { instructions[index, 0], instructions[index, 1] };
        }

        private void Initialize(byte[] table)
        {
            if (table.Length != 256 * 6)
                throw new FormatException("Invalid code table size");

            instructions = new Instruction[256, 2];
            for (int i = 0, t = 0; i < 256; i++) {
                instructions[i, 0] = CreateInstruction(table[t++], table[t++], table[t++]);
                instructions[i, 1] = CreateInstruction(table[t++], table[t++], table[t++]);
            }
        }

        private Instruction CreateInstruction(byte type, byte size, byte mode)
        {
            switch ((InstructionType)type) {
            case InstructionType.Noop:
                return new Noop();

            case InstructionType.Run:
                return new Run(size);

            case InstructionType.Add:
                return new Add(size);

            case InstructionType.Copy:
                return new Copy(size, mode, Cache);
            }

            return null;
        }

        private const byte DefaultNearSlots = 4;
        private const byte DefaultSameSlots = 3;
        private static readonly byte[] DefaultTable = new byte[1536] {
            //  T1  S1  M1  T2  S2  M2
            02, 00, 00, 00, 00, 00, // 0
            01, 00, 00, 00, 00, 00, // 1
            01, 01, 00, 00, 00, 00, // 2
            01, 02, 00, 00, 00, 00, // 3
            01, 03, 00, 00, 00, 00, // 4
            01, 04, 00, 00, 00, 00, // 5
            01, 05, 00, 00, 00, 00, // 6
            01, 06, 00, 00, 00, 00, // 7
            01, 07, 00, 00, 00, 00, // 8
            01, 08, 00, 00, 00, 00, // 9
            01, 09, 00, 00, 00, 00, // 10
            01, 10, 00, 00, 00, 00, // 11
            01, 11, 00, 00, 00, 00, // 12
            01, 12, 00, 00, 00, 00, // 13
            01, 13, 00, 00, 00, 00, // 14
            01, 14, 00, 00, 00, 00, // 15
            01, 15, 00, 00, 00, 00, // 16
            01, 16, 00, 00, 00, 00, // 17
            01, 17, 00, 00, 00, 00, // 18
            03, 00, 00, 00, 00, 00, // 19
            03, 04, 00, 00, 00, 00, // 20
            03, 05, 00, 00, 00, 00, // 21
            03, 06, 00, 00, 00, 00, // 22
            03, 07, 00, 00, 00, 00, // 23
            03, 08, 00, 00, 00, 00, // 24
            03, 09, 00, 00, 00, 00, // 25
            03, 10, 00, 00, 00, 00, // 26
            03, 11, 00, 00, 00, 00, // 27
            03, 12, 00, 00, 00, 00, // 28
            03, 13, 00, 00, 00, 00, // 29
            03, 14, 00, 00, 00, 00, // 30
            03, 15, 00, 00, 00, 00, // 31
            03, 16, 00, 00, 00, 00, // 32
            03, 17, 00, 00, 00, 00, // 33
            03, 18, 00, 00, 00, 00, // 34
            03, 00, 01, 00, 00, 00, // 35
            03, 04, 01, 00, 00, 00, // 36
            03, 05, 01, 00, 00, 00, // 37
            03, 06, 01, 00, 00, 00, // 38
            03, 07, 01, 00, 00, 00, // 39
            03, 08, 01, 00, 00, 00, // 40
            03, 09, 01, 00, 00, 00, // 41
            03, 10, 01, 00, 00, 00, // 42
            03, 11, 01, 00, 00, 00, // 43
            03, 12, 01, 00, 00, 00, // 44
            03, 13, 01, 00, 00, 00, // 45
            03, 14, 01, 00, 00, 00, // 46
            03, 15, 01, 00, 00, 00, // 47
            03, 16, 01, 00, 00, 00, // 48
            03, 17, 01, 00, 00, 00, // 49
            03, 18, 01, 00, 00, 00, // 50
            03, 00, 02, 00, 00, 00, // 51
            03, 04, 02, 00, 00, 00, // 52
            03, 05, 02, 00, 00, 00, // 53
            03, 06, 02, 00, 00, 00, // 54
            03, 07, 02, 00, 00, 00, // 55
            03, 08, 02, 00, 00, 00, // 56
            03, 09, 02, 00, 00, 00, // 57
            03, 10, 02, 00, 00, 00, // 58
            03, 11, 02, 00, 00, 00, // 59
            03, 12, 02, 00, 00, 00, // 60
            03, 13, 02, 00, 00, 00, // 61
            03, 14, 02, 00, 00, 00, // 62
            03, 15, 02, 00, 00, 00, // 63
            03, 16, 02, 00, 00, 00, // 64
            03, 17, 02, 00, 00, 00, // 65
            03, 18, 02, 00, 00, 00, // 66
            03, 00, 03, 00, 00, 00, // 67
            03, 04, 03, 00, 00, 00, // 68
            03, 05, 03, 00, 00, 00, // 69
            03, 06, 03, 00, 00, 00, // 70
            03, 07, 03, 00, 00, 00, // 71
            03, 08, 03, 00, 00, 00, // 72
            03, 09, 03, 00, 00, 00, // 73
            03, 10, 03, 00, 00, 00, // 74
            03, 11, 03, 00, 00, 00, // 75
            03, 12, 03, 00, 00, 00, // 76
            03, 13, 03, 00, 00, 00, // 77
            03, 14, 03, 00, 00, 00, // 78
            03, 15, 03, 00, 00, 00, // 79
            03, 16, 03, 00, 00, 00, // 80
            03, 17, 03, 00, 00, 00, // 81
            03, 18, 03, 00, 00, 00, // 82
            03, 00, 04, 00, 00, 00, // 83
            03, 04, 04, 00, 00, 00, // 84
            03, 05, 04, 00, 00, 00, // 85
            03, 06, 04, 00, 00, 00, // 86
            03, 07, 04, 00, 00, 00, // 87
            03, 08, 04, 00, 00, 00, // 88
            03, 09, 04, 00, 00, 00, // 89
            03, 10, 04, 00, 00, 00, // 90
            03, 11, 04, 00, 00, 00, // 91
            03, 12, 04, 00, 00, 00, // 92
            03, 13, 04, 00, 00, 00, // 93
            03, 14, 04, 00, 00, 00, // 94
            03, 15, 04, 00, 00, 00, // 95
            03, 16, 04, 00, 00, 00, // 96
            03, 17, 04, 00, 00, 00, // 97
            03, 18, 04, 00, 00, 00, // 98
            03, 00, 05, 00, 00, 00, // 99
            03, 04, 05, 00, 00, 00, // 100
            03, 05, 05, 00, 00, 00, // 101
            03, 06, 05, 00, 00, 00, // 102
            03, 07, 05, 00, 00, 00, // 103
            03, 08, 05, 00, 00, 00, // 104
            03, 09, 05, 00, 00, 00, // 105
            03, 10, 05, 00, 00, 00, // 106
            03, 11, 05, 00, 00, 00, // 107
            03, 12, 05, 00, 00, 00, // 108
            03, 13, 05, 00, 00, 00, // 109
            03, 14, 05, 00, 00, 00, // 110
            03, 15, 05, 00, 00, 00, // 111
            03, 16, 05, 00, 00, 00, // 112
            03, 17, 05, 00, 00, 00, // 113
            03, 18, 05, 00, 00, 00, // 114
            03, 00, 06, 00, 00, 00, // 115
            03, 04, 06, 00, 00, 00, // 116
            03, 05, 06, 00, 00, 00, // 117
            03, 06, 06, 00, 00, 00, // 118
            03, 07, 06, 00, 00, 00, // 119
            03, 08, 06, 00, 00, 00, // 120
            03, 09, 06, 00, 00, 00, // 121
            03, 10, 06, 00, 00, 00, // 122
            03, 11, 06, 00, 00, 00, // 123
            03, 12, 06, 00, 00, 00, // 124
            03, 13, 06, 00, 00, 00, // 125
            03, 14, 06, 00, 00, 00, // 126
            03, 15, 06, 00, 00, 00, // 127
            03, 16, 06, 00, 00, 00, // 128
            03, 17, 06, 00, 00, 00, // 129
            03, 18, 06, 00, 00, 00, // 130
            03, 00, 07, 00, 00, 00, // 131
            03, 04, 07, 00, 00, 00, // 132
            03, 05, 07, 00, 00, 00, // 133
            03, 06, 07, 00, 00, 00, // 134
            03, 07, 07, 00, 00, 00, // 135
            03, 08, 07, 00, 00, 00, // 136
            03, 09, 07, 00, 00, 00, // 137
            03, 10, 07, 00, 00, 00, // 138
            03, 11, 07, 00, 00, 00, // 139
            03, 12, 07, 00, 00, 00, // 140
            03, 13, 07, 00, 00, 00, // 141
            03, 14, 07, 00, 00, 00, // 142
            03, 15, 07, 00, 00, 00, // 143
            03, 16, 07, 00, 00, 00, // 144
            03, 17, 07, 00, 00, 00, // 145
            03, 18, 07, 00, 00, 00, // 146
            03, 00, 08, 00, 00, 00, // 147
            03, 04, 08, 00, 00, 00, // 148
            03, 05, 08, 00, 00, 00, // 149
            03, 06, 08, 00, 00, 00, // 150
            03, 07, 08, 00, 00, 00, // 151
            03, 08, 08, 00, 00, 00, // 152
            03, 09, 08, 00, 00, 00, // 153
            03, 10, 08, 00, 00, 00, // 154
            03, 11, 08, 00, 00, 00, // 155
            03, 12, 08, 00, 00, 00, // 156
            03, 13, 08, 00, 00, 00, // 157
            03, 14, 08, 00, 00, 00, // 158
            03, 15, 08, 00, 00, 00, // 159
            03, 16, 08, 00, 00, 00, // 160
            03, 17, 08, 00, 00, 00, // 161
            03, 18, 08, 00, 00, 00, // 162
            01, 01, 00, 03, 04, 00, // 163
            01, 01, 00, 03, 05, 00, // 164
            01, 01, 00, 03, 06, 00, // 165
            01, 02, 00, 03, 04, 00, // 166
            01, 02, 00, 03, 05, 00, // 167
            01, 02, 00, 03, 06, 00, // 168
            01, 03, 00, 03, 04, 00, // 169
            01, 03, 00, 03, 05, 00, // 170
            01, 03, 00, 03, 06, 00, // 171
            01, 04, 00, 03, 04, 00, // 172
            01, 04, 00, 03, 05, 00, // 173
            01, 04, 00, 03, 06, 00, // 174
            01, 01, 00, 03, 04, 01, // 175
            01, 01, 00, 03, 05, 01, // 176
            01, 01, 00, 03, 06, 01, // 177
            01, 02, 00, 03, 04, 01, // 178
            01, 02, 00, 03, 05, 01, // 179
            01, 02, 00, 03, 06, 01, // 180
            01, 03, 00, 03, 04, 01, // 181
            01, 03, 00, 03, 05, 01, // 182
            01, 03, 00, 03, 06, 01, // 183
            01, 04, 00, 03, 04, 01, // 184
            01, 04, 00, 03, 05, 01, // 185
            01, 04, 00, 03, 06, 01, // 186
            01, 01, 00, 03, 04, 02, // 187
            01, 01, 00, 03, 05, 02, // 188
            01, 01, 00, 03, 06, 02, // 189
            01, 02, 00, 03, 04, 02, // 190
            01, 02, 00, 03, 05, 02, // 191
            01, 02, 00, 03, 06, 02, // 192
            01, 03, 00, 03, 04, 02, // 193
            01, 03, 00, 03, 05, 02, // 194
            01, 03, 00, 03, 06, 02, // 195
            01, 04, 00, 03, 04, 02, // 196
            01, 04, 00, 03, 05, 02, // 197
            01, 04, 00, 03, 06, 02, // 198
            01, 01, 00, 03, 04, 03, // 199
            01, 01, 00, 03, 05, 03, // 200
            01, 01, 00, 03, 06, 03, // 201
            01, 02, 00, 03, 04, 03, // 202
            01, 02, 00, 03, 05, 03, // 203
            01, 02, 00, 03, 06, 03, // 204
            01, 03, 00, 03, 04, 03, // 205
            01, 03, 00, 03, 05, 03, // 206
            01, 03, 00, 03, 06, 03, // 207
            01, 04, 00, 03, 04, 03, // 208
            01, 04, 00, 03, 05, 03, // 209
            01, 04, 00, 03, 06, 03, // 210
            01, 01, 00, 03, 04, 04, // 211
            01, 01, 00, 03, 05, 04, // 212
            01, 01, 00, 03, 06, 04, // 213
            01, 02, 00, 03, 04, 04, // 214
            01, 02, 00, 03, 05, 04, // 215
            01, 02, 00, 03, 06, 04, // 216
            01, 03, 00, 03, 04, 04, // 217
            01, 03, 00, 03, 05, 04, // 218
            01, 03, 00, 03, 06, 04, // 219
            01, 04, 00, 03, 04, 04, // 220
            01, 04, 00, 03, 05, 04, // 221
            01, 04, 00, 03, 06, 04, // 222
            01, 01, 00, 03, 04, 05, // 223
            01, 01, 00, 03, 05, 05, // 224
            01, 01, 00, 03, 06, 05, // 225
            01, 02, 00, 03, 04, 05, // 226
            01, 02, 00, 03, 05, 05, // 227
            01, 02, 00, 03, 06, 05, // 228
            01, 03, 00, 03, 04, 05, // 229
            01, 03, 00, 03, 05, 05, // 230
            01, 03, 00, 03, 06, 05, // 231
            01, 04, 00, 03, 04, 05, // 232
            01, 04, 00, 03, 05, 05, // 233
            01, 04, 00, 03, 06, 05, // 234
            01, 01, 00, 03, 04, 06, // 235
            01, 02, 00, 03, 04, 06, // 236
            01, 03, 00, 03, 04, 06, // 237
            01, 04, 00, 03, 04, 06, // 238
            01, 01, 00, 03, 04, 07, // 239
            01, 02, 00, 03, 04, 07, // 240
            01, 03, 00, 03, 04, 07, // 241
            01, 04, 00, 03, 04, 07, // 242
            01, 01, 00, 03, 04, 08, // 243
            01, 02, 00, 03, 04, 08, // 244
            01, 03, 00, 03, 04, 08, // 245
            01, 04, 00, 03, 04, 08, // 246
            03, 04, 00, 01, 01, 00, // 247
            03, 04, 01, 01, 01, 00, // 248
            03, 04, 02, 01, 01, 00, // 249
            03, 04, 03, 01, 01, 00, // 250
            03, 04, 04, 01, 01, 00, // 251
            03, 04, 05, 01, 01, 00, // 252
            03, 04, 06, 01, 01, 00, // 253
            03, 04, 07, 01, 01, 00, // 254
            03, 04, 08, 01, 01, 00, // 255
        };
    }
}