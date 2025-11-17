/*
 * @(#)Instruction.cs                        2.1 2003/10/07
 *
 * Copyright (C) 1999, 2003 D.A. Watt and D.F. Brown
 * Dept. of Computing Science, University of Glasgow, Glasgow G12 8QQ Scotland
 * and School of Computer and Math Sciences, The Robert Gordon University,
 * St. Andrew Street, Aberdeen AB25 1HG, Scotland.
 * All rights reserved.
 *
 * This software is provided free for educational use only. It may
 * not be used for commercial purposes without the prior written permission
 * of the authors.
 *
 * Ported to C# for SproutLang compiler
 */

namespace SproutLang.TAM;

public class Instruction
{
    public Instruction()
    {
        Op = 0;
        R = 0;
        N = 0;
        D = 0;
    }

    // C# has no type synonyms, so the following representations are
    // assumed:
    //
    //  type
    //    OpCode = 0..15;  {4 bits unsigned}
    //    Length = 0..255;  {8 bits unsigned}
    //    Operand = -32767..+32767;  {16 bits signed}

    // Represents TAM instructions.
    public int Op { get; set; } // OpCode
    public int R { get; set; }  // RegisterNumber
    public int N { get; set; }  // Length
    public int D { get; set; }  // Operand

    public void Write(BinaryWriter output)
    {
        WriteBigEndianInt32(output, Op);
        WriteBigEndianInt32(output, R);
        WriteBigEndianInt32(output, N);
        WriteBigEndianInt32(output, D);
    }

    private static void WriteBigEndianInt32(BinaryWriter output, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value); // platform-endian
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);                    // make it big-endian
        output.Write(bytes);
    }

    public static Instruction? Read(BinaryReader input)
    {
        var inst = new Instruction();
        try
        {
            inst.Op = input.ReadInt32();
            inst.R = input.ReadInt32();
            inst.N = input.ReadInt32();
            inst.D = input.ReadInt32();
            return inst;
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }
}

