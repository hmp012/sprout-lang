/*
 * @(#)Machine.cs                        2.1 2003/10/07
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

public static class Machine
{
    public const int MaxRoutineLevel = 7;

    // WORDS AND ADDRESSES

    // C# has no type synonyms, so the following representations are
    // assumed:
    //
    //  type
    //    Word = -32767..+32767; {16 bits signed}
    //    DoubleWord = -2147483648..+2147483647; {32 bits signed}
    //    CodeAddress = 0..+32767; {15 bits unsigned}
    //    DataAddress = 0..+32767; {15 bits unsigned}


    // INSTRUCTIONS

    // Operation codes
    public const int
        LOADop = 0,
        LOADAop = 1,
        LOADIop = 2,
        LOADLop = 3,
        STOREop = 4,
        STOREIop = 5,
        CALLop = 6,
        CALLIop = 7,
        RETURNop = 8,
        PUSHop = 10,
        POPop = 11,
        JUMPop = 12,
        JUMPIop = 13,
        JUMPIFop = 14,
        HALTop = 15;


    // CODE STORE

    public static Instruction[] Code = new Instruction[1024];


    // CODE STORE REGISTERS

    public const int
        CB = 0,
        PB = 1024,  // = upper bound of code array + 1
        PT = 1052;  // = PB + 28

    // REGISTER NUMBERS

    public const int
        CBr = 0,
        CTr = 1,
        PBr = 2,
        PTr = 3,
        SBr = 4,
        STr = 5,
        HBr = 6,
        HTr = 7,
        LBr = 8,
        L1r = LBr + 1,
        L2r = LBr + 2,
        L3r = LBr + 3,
        L4r = LBr + 4,
        L5r = LBr + 5,
        L6r = LBr + 6,
        CPr = 15;


    // DATA REPRESENTATION

    public const int
        BooleanSize = 1,
        CharacterSize = 1,
        IntegerSize = 1,
        AddressSize = 1,
        ClosureSize = 2 * AddressSize,

        LinkDataSize = 3 * AddressSize,

        FalseRep = 0,
        TrueRep = 1,
        MaxintRep = 32767;


    // ADDRESSES OF PRIMITIVE ROUTINES

    public const int
        IdDisplacement = 1,
        NotDisplacement = 2,
        AndDisplacement = 3,
        OrDisplacement = 4,
        SuccDisplacement = 5,
        PredDisplacement = 6,
        NegDisplacement = 7,
        AddDisplacement = 8,
        SubDisplacement = 9,
        MultDisplacement = 10,
        DivDisplacement = 11,
        ModDisplacement = 12,
        LtDisplacement = 13,
        LeDisplacement = 14,
        GeDisplacement = 15,
        GtDisplacement = 16,
        EqDisplacement = 17,
        NeDisplacement = 18,
        EolDisplacement = 19,
        EofDisplacement = 20,
        GetDisplacement = 21,
        PutDisplacement = 22,
        GeteolDisplacement = 23,
        PuteolDisplacement = 24,
        GetintDisplacement = 25,
        PutintDisplacement = 26,
        NewDisplacement = 27,
        DisposeDisplacement = 28;

    // Initialize the code array
    static Machine()
    {
        for (int i = 0; i < Code.Length; i++)
        {
            Code[i] = new Instruction();
        }
    }
}

