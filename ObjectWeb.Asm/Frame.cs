using System;
using System.Text;

// ASM: a very small and fast Java bytecode manipulation framework
// Copyright (c) 2000-2011 INRIA, France Telecom
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
namespace ObjectWeb.Asm;

/// <summary>
/// The input and output stack map frames of a basic block.
/// 
/// <para>Stack map frames are computed in two steps:
/// 
/// <ul>
///   <li>During the visit of each instruction in MethodWriter, the state of the frame at the end of
///       the current basic block is updated by simulating the action of the instruction on the
///       previous state of this so called "output frame".
///   <li>After all instructions have been visited, a fix point algorithm is used in MethodWriter to
///       compute the "input frame" of each basic block (i.e. the stack map frame at the beginning of
///       the basic block). See <see cref="MethodWriter.ComputeAllFrames"/>.
/// </ul>
/// 
/// </para>
/// <para>Output stack map frames are computed relatively to the input frame of the basic block, which
/// is not yet known when output frames are computed. It is therefore necessary to be able to
/// represent abstract types such as "the type at position x in the input frame locals" or "the type
/// at position x from the top of the input frame stack" or even "the type at position x in the input
/// frame, with y more (or less) array dimensions". This explains the rather complicated type format
/// used in this class, explained below.
/// 
/// </para>
/// <para>The local variables and the operand stack of input and output frames contain Values called
/// "abstract types" hereafter. An abstract type is represented with 4 fields named DIM, KIND, FLAGS
/// and VALUE, packed in a single int value for better performance and memory efficiency:
/// 
/// <pre>
///   =====================================
///   |...DIM|KIND|.F|...............VALUE|
///   =====================================
/// </pre>
/// 
/// <ul>
///   <li>the DIM field, stored in the 6 most significant bits, is a signed number of array
///       dimensions (from -32 to 31, included). It can be retrieved with <see cref="DimMask"/> and a
///       right shift of <see cref="DimShift"/>.
///   <li>the KIND field, stored in 4 bits, indicates the kind of VALUE used. These 4 bits can be
///       retrieved with <see cref="KindMask"/> and, without any shift, must be equal to <see cref="ConstantKind"/>, <see cref="ReferenceKind"/>, <see cref="UninitializedKind"/>,
///       <see cref="ForwardUninitializedKind"/>, <see cref="LocalKind"/> or <see cref="StackKind"/>.
///   <li>the FLAGS field, stored in 2 bits, contains up to 2 boolean flags. Currently only one flag
///       is defined, namely <see cref="TopIfLongOrDoubleFlag"/>.
///   <li>the VALUE field, stored in the remaining 20 bits, contains either
///       <ul>
///         <li>one of the constants <see cref="Item_Top"/>, <see cref="ItemAsmBoolean"/>, <see cref="ItemAsmByte"/>, <see cref="ItemAsmChar"/> or <see cref="ItemAsmShort"/>, <see cref="Item_Integer"/>, <see cref="Item_Float"/>, <see cref="Item_Long"/>, <see cref="Item_Double"/>, <see cref="ITEM_NULL"/> or <see cref="Item_Uninitialized_This"/>, if KIND is equal to <see cref="ConstantKind"/>.
///         <li>the index of a <see cref="Symbol.Type_Tag"/> <see cref="Symbol"/> in the type table of a <see cref="SymbolTable"/>, if Kind is equal to <see cref="ReferenceKind"/>.
///         <li>the index of an <see cref="Symbol.Uninitialized_Type_Tag"/> <see cref="Symbol"/> in the type
///             table of a <see cref="SymbolTable"/>, if KIND is equal to <see cref="UninitializedKind"/>.
///         <li>the index of a <see cref="Symbol.Forward_Uninitialized_Type_Tag"/> <see cref="Symbol"/> in the
///             type table of a <see cref="SymbolTable"/>, if KIND is equal to <see cref="ForwardUninitializedKind"/>.
///         <li>the index of a local variable in the input stack frame, if KIND is equal to <see cref="LocalKind"/>.
///         <li>a position relatively to the top of the stack of the input stack frame, if KIND is
///             equal to <see cref="StackKind"/>,
///       </ul>
/// </ul>
/// 
/// </para>
/// <para>Output frames can contain abstract types of any kind and with a positive or negative array
/// dimension (and even unassigned types, represented by 0 - which does not correspond to any valid
/// abstract type value). Input frames can only contain ConstantKind, ReferenceKind,
/// UninitializedKind, or ForwardUninitializedKind abstract types of positive or <c>null</c>
/// array dimension. In all cases the type table contains only internal type names (array type
/// descriptors are forbidden - array dimensions must be represented through the DIM field).
/// 
/// </para>
/// <para>The LONG and DOUBLE types are always represented by using two slots (LONG + TOP or DOUBLE +
/// TOP), for local variables as well as in the operand stack. This is necessary to be able to
/// simulate DUPx_y instructions, whose effect would be dependent on the concrete types represented
/// by the abstract types in the stack (which are not always known).
/// 
/// @author Eric Bruneton
/// </para>
/// </summary>
internal class Frame
{
    // Constants used in the StackMapTable attribute.
    // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4.

    internal const int Same_Frame = 0;
    internal const int Same_Locals_1_Stack_Item_Frame = 64;
    internal const int Reserved = 128;
    internal const int Same_Locals_1_Stack_Item_Frame_Extended = 247;
    internal const int Chop_Frame = 248;
    internal const int Same_Frame_Extended = 251;
    internal const int Append_Frame = 252;
    internal const int Full_Frame = 255;

    internal const int Item_Top = 0;
    internal const int Item_Integer = 1;
    internal const int Item_Float = 2;
    internal const int Item_Double = 3;
    internal const int Item_Long = 4;
    internal const int Item_Null = 5;
    internal const int Item_Uninitialized_This = 6;
    internal const int Item_Object = 7;

    internal const int Item_Uninitialized = 8;

    // Additional, ASM specific constants used in abstract types below.
    private const int ItemAsmBoolean = 9;
    private const int ItemAsmByte = 10;
    private const int ItemAsmChar = 11;
    private const int ItemAsmShort = 12;

    // The size and offset in bits of each field of an abstract type.

    private const int DimSize = 6;
    private const int KindSize = 4;
    private const int FlagsSize = 2;
    private const int ValueSize = 32 - DimSize - KindSize - FlagsSize;

    private const int DimShift = KindSize + FlagsSize + ValueSize;
    private const int KindShift = FlagsSize + ValueSize;
    private const int FlagsShift = ValueSize;

    // Bitmasks to get each field of an abstract type.

    private const int DimMask = ((1 << DimSize) - 1) << DimShift;
    private const int KindMask = ((1 << KindSize) - 1) << KindShift;
    private const int ValueMask = (1 << ValueSize) - 1;

    // Constants to manipulate the DIM field of an abstract type.

    /// <summary>
    /// The constant to be added to an abstract type to get one with one more array dimension. </summary>
    private const int ArrayOf = +1 << DimShift;

    /// <summary>
    /// The constant to be added to an abstract type to get one with one less array dimension. </summary>
    private const int ElementOf = -1 << DimShift;

    // Possible Values for the KIND field of an abstract type.

    private const int ConstantKind = 1 << KindShift;
    private const int ReferenceKind = 2 << KindShift;
    private const int UninitializedKind = 3 << KindShift;
    private const int ForwardUninitializedKind = 4 << KindShift;
    private const int LocalKind = 5 << KindShift;
    private const int StackKind = 6 << KindShift;

    // Possible flags for the FLAGS field of an abstract type.

    /// <summary>
    /// A flag used for LOCAL_KIND and STACK_KIND abstract types, indicating that if the resolved,
    /// concrete type is LONG or DOUBLE, TOP should be used instead (because the value has been
    /// partially overridden with an xSTORE instruction).
    /// </summary>
    private const int TopIfLongOrDoubleFlag = 1 << FlagsShift;

    // Useful predefined abstract types (all the possible ConstantKind types).

    private const int Top = ConstantKind | Item_Top;
    private const int Boolean = ConstantKind | ItemAsmBoolean;
    private const int Byte = ConstantKind | ItemAsmByte;
    private const int Char = ConstantKind | ItemAsmChar;
    private const int Short = ConstantKind | ItemAsmShort;
    private const int Integer = ConstantKind | Item_Integer;
    private const int Float = ConstantKind | Item_Float;
    private const int Long = ConstantKind | Item_Long;
    private const int Double = ConstantKind | Item_Double;
    private const int Null = ConstantKind | Item_Null;
    private const int UninitializedThis = ConstantKind | Item_Uninitialized_This;

    // -----------------------------------------------------------------------------------------------
    // Instance fields
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// The basic block to which these input and output stack map frames correspond. </summary>
    internal Label owner;

    /// <summary>
    /// The input stack map frame locals. This is an array of abstract types. </summary>
    private int[] _inputLocals;

    /// <summary>
    /// The input stack map frame stack. This is an array of abstract types. </summary>
    private int[] _inputStack;

    /// <summary>
    /// The output stack map frame locals. This is an array of abstract types. </summary>
    private int[] _outputLocals;

    /// <summary>
    /// The output stack map frame stack. This is an array of abstract types. </summary>
    private int[] _outputStack;

    /// <summary>
    /// The start of the output stack, relatively to the input stack. This offset is always negative or
    /// null. A null offset means that the output stack must be appended to the input stack. A -n
    /// offset means that the first n output stack elements must replace the top n input stack
    /// elements, and that the other elements must be appended to the input stack.
    /// </summary>
    private short _outputStackStart;

    /// <summary>
    /// The index of the top stack element in <see cref="_outputStack"/>. </summary>
    private short _outputStackTop;

    /// <summary>
    /// The number of types that are initialized in the basic block. See <see cref="_initializations"/>. </summary>
    private int _initializationCount;

    /// <summary>
    /// The abstract types that are initialized in the basic block. A constructor invocation on an
    /// UNINITIALIZED, FORWARD_UNINITIALIZED or UNINITIALIZED_THIS abstract type must replace <i>every
    /// occurrence</i> of this type in the local variables and in the operand stack. This cannot be
    /// done during the first step of the algorithm since, during this step, the local variables and
    /// the operand stack types are still abstract. It is therefore necessary to store the abstract
    /// types of the constructors which are invoked in the basic block, in order to do this replacement
    /// during the second step of the algorithm, where the frames are fully computed. Note that this
    /// array can contain abstract types that are relative to the input locals or to the input stack.
    /// </summary>
    private int[] _initializations;

    // -----------------------------------------------------------------------------------------------
    // Constructor
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Constructs a new Frame.
    /// </summary>
    /// <param name="owner"> the basic block to which these input and output stack map frames correspond. </param>
    public Frame(Label owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Sets this frame to the value of the given frame.
    /// 
    /// <para>WARNING: after this method is called the two frames share the same data structures. It is
    /// recommended to discard the given frame to avoid unexpected side effects.
    /// 
    /// </para>
    /// </summary>
    /// <param name="frame"> The new frame value. </param>
    public void CopyFrom(Frame frame)
    {
        _inputLocals = frame._inputLocals;
        _inputStack = frame._inputStack;
        _outputStackStart = 0;
        _outputLocals = frame._outputLocals;
        _outputStack = frame._outputStack;
        _outputStackTop = frame._outputStackTop;
        _initializationCount = frame._initializationCount;
        _initializations = frame._initializations;
    }

    // -----------------------------------------------------------------------------------------------
    // Static methods to get abstract types from other type formats
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the abstract type corresponding to the given public API frame element type.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="type"> a frame element type described using the same format as in <see cref="MethodVisitor.visitFrame"/>, i.e. either <see cref="Opcodes.top/>, <see cref="Opcodes.integer/>, <see cref="Opcodes.FLOAT"/>, <see cref="Opcodes.long/>, <see cref="Opcodes.double/>, <see cref="Opcodes.null/>, or
    ///     <see cref="Opcodes.uninitializedThis/>, or the internal name of a class, or a Label designating
    ///     a NEW instruction (for uninitialized types). </param>
    /// <returns> the abstract type corresponding to the given frame element type. </returns>
    internal static int GetAbstractTypeFromApiFormat(SymbolTable symbolTable, object type)
    {
        if (type is int)
        {
            return ConstantKind | ((int?)type).Value;
        }
        else if (type is string)
        {
            string descriptor = JType.GetObjectType((string)type).Descriptor;
            return GetAbstractTypeFromDescriptor(symbolTable, descriptor, 0);
        }
        else
        {
            //return UninitializedKind | symbolTable.AddUninitializedType("", ((Label)type).bytecodeOffset);
            Label label = (Label)type;
            if ((label.flags & Label.Flag_Resolved) != 0)
            {
                return UninitializedKind | symbolTable.AddUninitializedType("", label.bytecodeOffset);
            }
            else
            {
                return ForwardUninitializedKind | symbolTable.AddForwardUninitializedType("", label);
            }
        }
    }

    /// <summary>
    /// Returns the abstract type corresponding to the internal name of a class.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="internalName"> the internal name of a class. This must <i>not</i> be an array type
    ///     descriptor. </param>
    /// <returns> the abstract type value corresponding to the given internal name. </returns>
    internal static int GetAbstractTypeFromInternalName(SymbolTable symbolTable, string internalName)
    {
        return ReferenceKind | symbolTable.AddType(internalName);
    }

    /// <summary>
    /// Returns the abstract type corresponding to the given type descriptor.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="buffer"> a string ending with a type descriptor. </param>
    /// <param name="offset"> the start offset of the type descriptor in buffer. </param>
    /// <returns> the abstract type corresponding to the given type descriptor. </returns>
    private static int GetAbstractTypeFromDescriptor(SymbolTable symbolTable, string buffer, int offset)
    {
        string internalName;
        switch (buffer[offset])
        {
            case 'V':
                return 0;
            case 'Z':
            case 'C':
            case 'B':
            case 'S':
            case 'I':
                return Integer;
            case 'F':
                return Float;
            case 'J':
                return Long;
            case 'D':
                return Double;
            case 'L':
                internalName = buffer.Substring(offset + 1, (buffer.Length - 1) - (offset + 1));
                return ReferenceKind | symbolTable.AddType(internalName);
            case '[':
                int elementDescriptorOffset = offset + 1;
                while (buffer[elementDescriptorOffset] == '[')
                {
                    ++elementDescriptorOffset;
                }

                int typeValue;
                switch (buffer[elementDescriptorOffset])
                {
                    case 'Z':
                        typeValue = Boolean;
                        break;
                    case 'C':
                        typeValue = Char;
                        break;
                    case 'B':
                        typeValue = Byte;
                        break;
                    case 'S':
                        typeValue = Short;
                        break;
                    case 'I':
                        typeValue = Integer;
                        break;
                    case 'F':
                        typeValue = Float;
                        break;
                    case 'J':
                        typeValue = Long;
                        break;
                    case 'D':
                        typeValue = Double;
                        break;
                    case 'L':
                        internalName = buffer.Substring(elementDescriptorOffset + 1,
                            (buffer.Length - 1) - (elementDescriptorOffset + 1));
                        typeValue = ReferenceKind | symbolTable.AddType(internalName);
                        break;
                    default:
                        throw new System.ArgumentException(
                            "Invalid descriptor fragment: " + buffer.Substring(elementDescriptorOffset));
                }

                return ((elementDescriptorOffset - offset) << DimShift) | typeValue;
            default:
                throw new System.ArgumentException("Invalid descriptor: " + buffer.Substring(offset));
        }
    }

    // -----------------------------------------------------------------------------------------------
    // Methods related to the input frame
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Sets the input frame from the given method description. This method is used to initialize the
    /// first frame of a method, which is implicit (i.e. not stored explicitly in the StackMapTable
    /// attribute).
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="access"> the method's access flags. </param>
    /// <param name="descriptor"> the method descriptor. </param>
    /// <param name="maxLocals"> the maximum number of local variables of the method. </param>
    public void SetInputFrameFromDescriptor(SymbolTable symbolTable, int access, string descriptor, int maxLocals)
    {
        _inputLocals = new int[maxLocals];
        _inputStack = new int[0];
        int inputLocalIndex = 0;
        if ((access & Opcodes.Acc_Static) == 0)
        {
            if ((access & Constants.Acc_Constructor) == 0)
            {
                _inputLocals[inputLocalIndex++] = ReferenceKind | symbolTable.AddType(symbolTable.ClassName);
            }
            else
            {
                _inputLocals[inputLocalIndex++] = UninitializedThis;
            }
        }

        foreach (JType argumentType in JType.GetArgumentTypes(descriptor))
        {
            int abstractType = GetAbstractTypeFromDescriptor(symbolTable, argumentType.Descriptor, 0);
            _inputLocals[inputLocalIndex++] = abstractType;
            if (abstractType == Long || abstractType == Double)
            {
                _inputLocals[inputLocalIndex++] = Top;
            }
        }

        while (inputLocalIndex < maxLocals)
        {
            _inputLocals[inputLocalIndex++] = Top;
        }
    }

    /// <summary>
    /// Sets the input frame from the given public API frame description.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="numLocal"> the number of local variables. </param>
    /// <param name="local"> the local variable types, described using the same format as in <see cref="MethodVisitor.visitFrame"/>. </param>
    /// <param name="numStack"> the number of operand stack elements. </param>
    /// <param name="stack"> the operand stack types, described using the same format as in <see cref="MethodVisitor.visitFrame"/>. </param>
    public void SetInputFrameFromApiFormat(SymbolTable symbolTable, int numLocal, object[] local, int numStack,
        object[] stack)
    {
        int inputLocalIndex = 0;
        for (int i = 0; i < numLocal; ++i)
        {
            _inputLocals[inputLocalIndex++] = GetAbstractTypeFromApiFormat(symbolTable, local[i]);
            if (Equals(local[i], Opcodes.@long) || Equals(local[i], Opcodes.@double))
            {
                _inputLocals[inputLocalIndex++] = Top;
            }
        }

        while (inputLocalIndex < _inputLocals.Length)
        {
            _inputLocals[inputLocalIndex++] = Top;
        }

        int numStackTop = 0;
        for (int i = 0; i < numStack; ++i)
        {
            if (Equals(stack[i], Opcodes.@long) || Equals(stack[i], Opcodes.@double))
            {
                ++numStackTop;
            }
        }

        _inputStack = new int[numStack + numStackTop];
        int inputStackIndex = 0;
        for (int i = 0; i < numStack; ++i)
        {
            _inputStack[inputStackIndex++] = GetAbstractTypeFromApiFormat(symbolTable, stack[i]);
            if (Equals(stack[i], Opcodes.@long) || Equals(stack[i], Opcodes.@double))
            {
                _inputStack[inputStackIndex++] = Top;
            }
        }

        _outputStackTop = 0;
        _initializationCount = 0;
    }

    public int InputStackSize => _inputStack.Length;

    // -----------------------------------------------------------------------------------------------
    // Methods related to the output frame
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the abstract type stored at the given local variable index in the output frame.
    /// </summary>
    /// <param name="localIndex"> the index of the local variable whose value must be returned. </param>
    /// <returns> the abstract type stored at the given local variable index in the output frame. </returns>
    private int GetLocal(int localIndex)
    {
        if (_outputLocals == null || localIndex >= _outputLocals.Length)
        {
            // If this local has never been assigned in this basic block, it is still equal to its value
            // in the input frame.
            return LocalKind | localIndex;
        }
        else
        {
            int abstractType = _outputLocals[localIndex];
            if (abstractType == 0)
            {
                // If this local has never been assigned in this basic block, so it is still equal to its
                // value in the input frame.
                abstractType = _outputLocals[localIndex] = LocalKind | localIndex;
            }

            return abstractType;
        }
    }

    /// <summary>
    /// Replaces the abstract type stored at the given local variable index in the output frame.
    /// </summary>
    /// <param name="localIndex"> the index of the output frame local variable that must be set. </param>
    /// <param name="abstractType"> the value that must be set. </param>
    private void SetLocal(int localIndex, int abstractType)
    {
        // Create and/or resize the output local variables array if necessary.
        if (_outputLocals == null)
        {
            _outputLocals = new int[10];
        }

        int outputLocalsLength = _outputLocals.Length;
        if (localIndex >= outputLocalsLength)
        {
            int[] newOutputLocals = new int[Math.Max(localIndex + 1, 2 * outputLocalsLength)];
            Array.Copy(_outputLocals, 0, newOutputLocals, 0, outputLocalsLength);
            _outputLocals = newOutputLocals;
        }

        // Set the local variable.
        _outputLocals[localIndex] = abstractType;
    }

    /// <summary>
    /// Pushes the given abstract type on the output frame stack.
    /// </summary>
    /// <param name="abstractType"> an abstract type. </param>
    private void Push(int abstractType)
    {
        // Create and/or resize the output stack array if necessary.
        if (_outputStack == null)
        {
            _outputStack = new int[10];
        }

        int outputStackLength = _outputStack.Length;
        if (_outputStackTop >= outputStackLength)
        {
            int[] newOutputStack = new int[Math.Max(_outputStackTop + 1, 2 * outputStackLength)];
            Array.Copy(_outputStack, 0, newOutputStack, 0, outputStackLength);
            _outputStack = newOutputStack;
        }

        // Pushes the abstract type on the output stack.
        _outputStack[_outputStackTop++] = abstractType;
        // Updates the maximum size reached by the output stack, if needed (note that this size is
        // relative to the input stack size, which is not known yet).
        short outputStackSize = (short)(_outputStackStart + _outputStackTop);
        if (outputStackSize > owner.outputStackMax)
        {
            owner.outputStackMax = outputStackSize;
        }
    }

    /// <summary>
    /// Pushes the abstract type corresponding to the given descriptor on the output frame stack.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="descriptor"> a type or method descriptor (in which case its return type is pushed). </param>
    private void Push(SymbolTable symbolTable, string descriptor)
    {
        int typeDescriptorOffset = descriptor[0] == '(' ? JType.GetReturnTypeOffset(descriptor) : 0;
        int abstractType = GetAbstractTypeFromDescriptor(symbolTable, descriptor, typeDescriptorOffset);
        if (abstractType != 0)
        {
            Push(abstractType);
            if (abstractType == Long || abstractType == Double)
            {
                Push(Top);
            }
        }
    }

    /// <summary>
    /// Pops an abstract type from the output frame stack and returns its value.
    /// </summary>
    /// <returns> the abstract type that has been popped from the output frame stack. </returns>
    private int Pop()
    {
        if (_outputStackTop > 0)
        {
            return _outputStack[--_outputStackTop];
        }
        else
        {
            // If the output frame stack is empty, pop from the input stack.
            return StackKind | -(--_outputStackStart);
        }
    }

    /// <summary>
    /// Pops the given number of abstract types from the output frame stack.
    /// </summary>
    /// <param name="elements"> the number of abstract types that must be popped. </param>
    private void Pop(int elements)
    {
        if (_outputStackTop >= elements)
        {
            _outputStackTop -= (short)elements;
        }
        else
        {
            // If the number of elements to be popped is greater than the number of elements in the output
            // stack, clear it, and pop the remaining elements from the input stack.
            _outputStackStart -= (short)(elements - _outputStackTop);
            _outputStackTop = 0;
        }
    }

    /// <summary>
    /// Pops as many abstract types from the output frame stack as described by the given descriptor.
    /// </summary>
    /// <param name="descriptor"> a type or method descriptor (in which case its argument types are popped). </param>
    private void Pop(string descriptor)
    {
        char firstDescriptorChar = descriptor[0];
        if (firstDescriptorChar == '(')
        {
            Pop((JType.GetArgumentsAndReturnSizes(descriptor) >> 2) - 1);
        }
        else if (firstDescriptorChar == 'J' || firstDescriptorChar == 'D')
        {
            Pop(2);
        }
        else
        {
            Pop(1);
        }
    }

    // -----------------------------------------------------------------------------------------------
    // Methods to handle uninitialized types
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Adds an abstract type to the list of types on which a constructor is invoked in the basic
    /// block.
    /// </summary>
    /// <param name="abstractType"> an abstract type on a which a constructor is invoked. </param>
    private void AddInitializedType(int abstractType)
    {
        // Create and/or resize the initializations array if necessary.
        if (_initializations == null)
        {
            _initializations = new int[2];
        }

        int initializationsLength = _initializations.Length;
        if (_initializationCount >= initializationsLength)
        {
            int[] newInitializations = new int[Math.Max(_initializationCount + 1, 2 * initializationsLength)];
            Array.Copy(_initializations, 0, newInitializations, 0, initializationsLength);
            _initializations = newInitializations;
        }

        // Store the abstract type.
        _initializations[_initializationCount++] = abstractType;
    }

    /// <summary>
    /// Returns the "initialized" abstract type corresponding to the given abstract type.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="abstractType"> an abstract type. </param>
    /// <returns> the REFERENCE_KIND abstract type corresponding to abstractType if it is
    ///     UNINITIALIZED_THIS or an UNINITIALIZED_KIND or FORWARD_UNINITIALIZED_KIND abstract type for
    ///     one of the types on which a constructor is invoked in the basic block. Otherwise returns
    ///     abstractType. </returns>
    private int GetInitializedType(SymbolTable symbolTable, int abstractType)
    {
        if (abstractType == UninitializedThis
            || (abstractType & (DimMask | KindMask)) == UninitializedKind
            || (abstractType & (DimMask | KindMask)) == ForwardUninitializedKind)
        {
            for (int i = 0; i < _initializationCount; ++i)
            {
                int initializedType = _initializations[i];
                int dim = initializedType & DimMask;
                int kind = initializedType & KindMask;
                int value = initializedType & ValueMask;
                if (kind == LocalKind)
                {
                    initializedType = dim + _inputLocals[value];
                }
                else if (kind == StackKind)
                {
                    initializedType = dim + _inputStack[_inputStack.Length - value];
                }

                if (abstractType == initializedType)
                {
                    if (abstractType == UninitializedThis)
                    {
                        return ReferenceKind | symbolTable.AddType(symbolTable.ClassName);
                    }
                    else
                    {
                        return ReferenceKind |
                               symbolTable.AddType(symbolTable.GetType(abstractType & ValueMask).value);
                    }
                }
            }
        }

        return abstractType;
    }

    // -----------------------------------------------------------------------------------------------
    // Main method, to simulate the execution of each instruction on the output frame
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Simulates the action of the given instruction on the output stack frame.
    /// </summary>
    /// <param name="opcode"> the opcode of the instruction. </param>
    /// <param name="arg"> the numeric operand of the instruction, if any. </param>
    /// <param name="argSymbol"> the Symbol operand of the instruction, if any. </param>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    public virtual void Execute(int opcode, int arg, Symbol argSymbol, SymbolTable symbolTable)
    {
        // Abstract types popped from the stack or read from local variables.
        int abstractType1;
        int abstractType2;
        int abstractType3;
        int abstractType4;
        switch (opcode)
        {
            case Opcodes.Nop:
            case Opcodes.Ineg:
            case Opcodes.Lneg:
            case Opcodes.Fneg:
            case Opcodes.Dneg:
            case Opcodes.I2B:
            case Opcodes.I2C:
            case Opcodes.I2S:
            case Opcodes.Goto:
            case Opcodes.Return:
                break;
            case Opcodes.Aconst_Null:
                Push(Null);
                break;
            case Opcodes.Iconst_M1:
            case Opcodes.Iconst_0:
            case Opcodes.Iconst_1:
            case Opcodes.Iconst_2:
            case Opcodes.Iconst_3:
            case Opcodes.Iconst_4:
            case Opcodes.Iconst_5:
            case Opcodes.Bipush:
            case Opcodes.Sipush:
            case Opcodes.Iload:
                Push(Integer);
                break;
            case Opcodes.Lconst_0:
            case Opcodes.Lconst_1:
            case Opcodes.Lload:
                Push(Long);
                Push(Top);
                break;
            case Opcodes.Fconst_0:
            case Opcodes.Fconst_1:
            case Opcodes.Fconst_2:
            case Opcodes.Fload:
                Push(Float);
                break;
            case Opcodes.Dconst_0:
            case Opcodes.Dconst_1:
            case Opcodes.Dload:
                Push(Double);
                Push(Top);
                break;
            case Opcodes.Ldc:
                switch (argSymbol.tag)
                {
                    case Symbol.Constant_Integer_Tag:
                        Push(Integer);
                        break;
                    case Symbol.Constant_Long_Tag:
                        Push(Long);
                        Push(Top);
                        break;
                    case Symbol.Constant_Float_Tag:
                        Push(Float);
                        break;
                    case Symbol.Constant_Double_Tag:
                        Push(Double);
                        Push(Top);
                        break;
                    case Symbol.Constant_Class_Tag:
                        Push(ReferenceKind | symbolTable.AddType("java/lang/Class"));
                        break;
                    case Symbol.Constant_String_Tag:
                        Push(ReferenceKind | symbolTable.AddType("java/lang/String"));
                        break;
                    case Symbol.Constant_Method_Type_Tag:
                        Push(ReferenceKind | symbolTable.AddType("java/lang/invoke/MethodType"));
                        break;
                    case Symbol.Constant_Method_Handle_Tag:
                        Push(ReferenceKind | symbolTable.AddType("java/lang/invoke/MethodHandle"));
                        break;
                    case Symbol.Constant_Dynamic_Tag:
                        Push(symbolTable, argSymbol.value);
                        break;
                    default:
                        throw new Exception("AssertionError");
                }

                break;
            case Opcodes.Aload:
                Push(GetLocal(arg));
                break;
            case Opcodes.Laload:
            case Opcodes.D2L:
                Pop(2);
                Push(Long);
                Push(Top);
                break;
            case Opcodes.Daload:
            case Opcodes.L2D:
                Pop(2);
                Push(Double);
                Push(Top);
                break;
            case Opcodes.Aaload:
                Pop(1);
                abstractType1 = Pop();
                Push(abstractType1 == Null ? abstractType1 : ElementOf + abstractType1);
                break;
            case Opcodes.Istore:
            case Opcodes.Fstore:
            case Opcodes.Astore:
                abstractType1 = Pop();
                SetLocal(arg, abstractType1);
                if (arg > 0)
                {
                    int previousLocalType = GetLocal(arg - 1);
                    if (previousLocalType == Long || previousLocalType == Double)
                    {
                        SetLocal(arg - 1, Top);
                    }
                    else if ((previousLocalType & KindMask) == LocalKind ||
                             (previousLocalType & KindMask) == StackKind)
                    {
                        // The type of the previous local variable is not known yet, but if it later appears
                        // to be LONG or DOUBLE, we should then use TOP instead.
                        SetLocal(arg - 1, previousLocalType | TopIfLongOrDoubleFlag);
                    }
                }

                break;
            case Opcodes.Lstore:
            case Opcodes.Dstore:
                Pop(1);
                abstractType1 = Pop();
                SetLocal(arg, abstractType1);
                SetLocal(arg + 1, Top);
                if (arg > 0)
                {
                    int previousLocalType = GetLocal(arg - 1);
                    if (previousLocalType == Long || previousLocalType == Double)
                    {
                        SetLocal(arg - 1, Top);
                    }
                    else if ((previousLocalType & KindMask) == LocalKind ||
                             (previousLocalType & KindMask) == StackKind)
                    {
                        // The type of the previous local variable is not known yet, but if it later appears
                        // to be LONG or DOUBLE, we should then use TOP instead.
                        SetLocal(arg - 1, previousLocalType | TopIfLongOrDoubleFlag);
                    }
                }

                break;
            case Opcodes.Iastore:
            case Opcodes.Bastore:
            case Opcodes.Castore:
            case Opcodes.Sastore:
            case Opcodes.Fastore:
            case Opcodes.Aastore:
                Pop(3);
                break;
            case Opcodes.Lastore:
            case Opcodes.Dastore:
                Pop(4);
                break;
            case Opcodes.Pop:
            case Opcodes.Ifeq:
            case Opcodes.Ifne:
            case Opcodes.Iflt:
            case Opcodes.Ifge:
            case Opcodes.Ifgt:
            case Opcodes.Ifle:
            case Opcodes.Ireturn:
            case Opcodes.Freturn:
            case Opcodes.Areturn:
            case Opcodes.Tableswitch:
            case Opcodes.Lookupswitch:
            case Opcodes.Athrow:
            case Opcodes.Monitorenter:
            case Opcodes.Monitorexit:
            case Opcodes.Ifnull:
            case Opcodes.Ifnonnull:
                Pop(1);
                break;
            case Opcodes.Pop2:
            case Opcodes.If_Icmpeq:
            case Opcodes.If_Icmpne:
            case Opcodes.If_Icmplt:
            case Opcodes.If_Icmpge:
            case Opcodes.If_Icmpgt:
            case Opcodes.If_Icmple:
            case Opcodes.If_Acmpeq:
            case Opcodes.If_Acmpne:
            case Opcodes.Lreturn:
            case Opcodes.Dreturn:
                Pop(2);
                break;
            case Opcodes.Dup:
                abstractType1 = Pop();
                Push(abstractType1);
                Push(abstractType1);
                break;
            case Opcodes.Dup_X1:
                abstractType1 = Pop();
                abstractType2 = Pop();
                Push(abstractType1);
                Push(abstractType2);
                Push(abstractType1);
                break;
            case Opcodes.Dup_X2:
                abstractType1 = Pop();
                abstractType2 = Pop();
                abstractType3 = Pop();
                Push(abstractType1);
                Push(abstractType3);
                Push(abstractType2);
                Push(abstractType1);
                break;
            case Opcodes.Dup2:
                abstractType1 = Pop();
                abstractType2 = Pop();
                Push(abstractType2);
                Push(abstractType1);
                Push(abstractType2);
                Push(abstractType1);
                break;
            case Opcodes.Dup2_X1:
                abstractType1 = Pop();
                abstractType2 = Pop();
                abstractType3 = Pop();
                Push(abstractType2);
                Push(abstractType1);
                Push(abstractType3);
                Push(abstractType2);
                Push(abstractType1);
                break;
            case Opcodes.Dup2_X2:
                abstractType1 = Pop();
                abstractType2 = Pop();
                abstractType3 = Pop();
                abstractType4 = Pop();
                Push(abstractType2);
                Push(abstractType1);
                Push(abstractType4);
                Push(abstractType3);
                Push(abstractType2);
                Push(abstractType1);
                break;
            case Opcodes.Swap:
                abstractType1 = Pop();
                abstractType2 = Pop();
                Push(abstractType1);
                Push(abstractType2);
                break;
            case Opcodes.Iaload:
            case Opcodes.Baload:
            case Opcodes.Caload:
            case Opcodes.Saload:
            case Opcodes.Iadd:
            case Opcodes.Isub:
            case Opcodes.Imul:
            case Opcodes.Idiv:
            case Opcodes.Irem:
            case Opcodes.Iand:
            case Opcodes.Ior:
            case Opcodes.Ixor:
            case Opcodes.Ishl:
            case Opcodes.Ishr:
            case Opcodes.Iushr:
            case Opcodes.L2I:
            case Opcodes.D2I:
            case Opcodes.Fcmpl:
            case Opcodes.Fcmpg:
                Pop(2);
                Push(Integer);
                break;
            case Opcodes.Ladd:
            case Opcodes.Lsub:
            case Opcodes.Lmul:
            case Opcodes.Ldiv:
            case Opcodes.Lrem:
            case Opcodes.Land:
            case Opcodes.Lor:
            case Opcodes.Lxor:
                Pop(4);
                Push(Long);
                Push(Top);
                break;
            case Opcodes.Faload:
            case Opcodes.Fadd:
            case Opcodes.Fsub:
            case Opcodes.Fmul:
            case Opcodes.Fdiv:
            case Opcodes.Frem:
            case Opcodes.L2F:
            case Opcodes.D2F:
                Pop(2);
                Push(Float);
                break;
            case Opcodes.Dadd:
            case Opcodes.Dsub:
            case Opcodes.Dmul:
            case Opcodes.Ddiv:
            case Opcodes.Drem:
                Pop(4);
                Push(Double);
                Push(Top);
                break;
            case Opcodes.Lshl:
            case Opcodes.Lshr:
            case Opcodes.Lushr:
                Pop(3);
                Push(Long);
                Push(Top);
                break;
            case Opcodes.Iinc:
                SetLocal(arg, Integer);
                break;
            case Opcodes.I2L:
            case Opcodes.F2L:
                Pop(1);
                Push(Long);
                Push(Top);
                break;
            case Opcodes.I2F:
                Pop(1);
                Push(Float);
                break;
            case Opcodes.I2D:
            case Opcodes.F2D:
                Pop(1);
                Push(Double);
                Push(Top);
                break;
            case Opcodes.F2I:
            case Opcodes.Arraylength:
            case Opcodes.Instanceof:
                Pop(1);
                Push(Integer);
                break;
            case Opcodes.Lcmp:
            case Opcodes.Dcmpl:
            case Opcodes.Dcmpg:
                Pop(4);
                Push(Integer);
                break;
            case Opcodes.Jsr:
            case Opcodes.Ret:
                throw new System.ArgumentException("JSR/RET are not supported with computeFrames option");
            case Opcodes.Getstatic:
                Push(symbolTable, argSymbol.value);
                break;
            case Opcodes.Putstatic:
                Pop(argSymbol.value);
                break;
            case Opcodes.Getfield:
                Pop(1);
                Push(symbolTable, argSymbol.value);
                break;
            case Opcodes.Putfield:
                Pop(argSymbol.value);
                Pop();
                break;
            case Opcodes.Invokevirtual:
            case Opcodes.Invokespecial:
            case Opcodes.Invokestatic:
            case Opcodes.Invokeinterface:
                Pop(argSymbol.value);
                if (opcode != Opcodes.Invokestatic)
                {
                    abstractType1 = Pop();
                    if (opcode == Opcodes.Invokespecial && argSymbol.name[0] == '<')
                    {
                        AddInitializedType(abstractType1);
                    }
                }

                Push(symbolTable, argSymbol.value);
                break;
            case Opcodes.Invokedynamic:
                Pop(argSymbol.value);
                Push(symbolTable, argSymbol.value);
                break;
            case Opcodes.New:
                Push(UninitializedKind | symbolTable.AddUninitializedType(argSymbol.value, arg));
                break;
            case Opcodes.Newarray:
                Pop();
                switch (arg)
                {
                    case Opcodes.Boolean:
                        Push(ArrayOf | Boolean);
                        break;
                    case Opcodes.Char:
                        Push(ArrayOf | Char);
                        break;
                    case Opcodes.Byte:
                        Push(ArrayOf | Byte);
                        break;
                    case Opcodes.Short:
                        Push(ArrayOf | Short);
                        break;
                    case Opcodes.Int:
                        Push(ArrayOf | Integer);
                        break;
                    case Opcodes.Float:
                        Push(ArrayOf | Float);
                        break;
                    case Opcodes.Double:
                        Push(ArrayOf | Double);
                        break;
                    case Opcodes.Long:
                        Push(ArrayOf | Long);
                        break;
                    default:
                        throw new System.ArgumentException();
                }

                break;
            case Opcodes.Anewarray:
                string arrayElementType = argSymbol.value;
                Pop();
                if (arrayElementType[0] == '[')
                {
                    Push(symbolTable, '[' + arrayElementType);
                }
                else
                {
                    Push(ArrayOf | ReferenceKind | symbolTable.AddType(arrayElementType));
                }

                break;
            case Opcodes.Checkcast:
                string castType = argSymbol.value;
                Pop();
                if (castType[0] == '[')
                {
                    Push(symbolTable, castType);
                }
                else
                {
                    Push(ReferenceKind | symbolTable.AddType(castType));
                }

                break;
            case Opcodes.Multianewarray:
                Pop(arg);
                Push(symbolTable, argSymbol.value);
                break;
            default:
                throw new System.ArgumentException();
        }
    }

    // -----------------------------------------------------------------------------------------------
    // Frame merging methods, used in the second step of the stack map frame computation algorithm
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Computes the concrete output type corresponding to a given abstract output type.
    /// </summary>
    /// <param name="abstractOutputType"> an abstract output type. </param>
    /// <param name="numStack"> the size of the input stack, used to resolve abstract output types of
    ///     STACK_KIND kind. </param>
    /// <returns> the concrete output type corresponding to 'abstractOutputType'. </returns>
    private int GetConcreteOutputType(int abstractOutputType, int numStack)
    {
        int dim = abstractOutputType & DimMask;
        int kind = abstractOutputType & KindMask;
        if (kind == LocalKind)
        {
            // By definition, a LOCAL_KIND type designates the concrete type of a local variable at
            // the beginning of the basic block corresponding to this frame (which is known when
            // this method is called, but was not when the abstract type was computed).
            int concreteOutputType = dim + _inputLocals[abstractOutputType & ValueMask];
            if ((abstractOutputType & TopIfLongOrDoubleFlag) != 0 &&
                (concreteOutputType == Long || concreteOutputType == Double))
            {
                concreteOutputType = Top;
            }

            return concreteOutputType;
        }
        else if (kind == StackKind)
        {
            // By definition, a STACK_KIND type designates the concrete type of a local variable at
            // the beginning of the basic block corresponding to this frame (which is known when
            // this method is called, but was not when the abstract type was computed).
            int concreteOutputType = dim + _inputStack[numStack - (abstractOutputType & ValueMask)];
            if ((abstractOutputType & TopIfLongOrDoubleFlag) != 0 &&
                (concreteOutputType == Long || concreteOutputType == Double))
            {
                concreteOutputType = Top;
            }

            return concreteOutputType;
        }
        else
        {
            return abstractOutputType;
        }
    }

    /// <summary>
    /// Merges the input frame of the given <see cref="Frame"/> with the input and output frames of this
    /// <see cref="Frame"/>. Returns <c>true</c> if the given frame has been changed by this operation
    /// (the input and output frames of this <see cref="Frame"/> are never changed).
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="dstFrame"> the <see cref="Frame"/> whose input frame must be updated. This should be the frame
    ///     of a successor, in the control flow graph, of the basic block corresponding to this frame. </param>
    /// <param name="catchTypeIndex"> if 'frame' corresponds to an exception handler basic block, the type
    ///     table index of the caught exception type, otherwise 0. </param>
    /// <returns> <c>true</c> if the input frame of 'frame' has been changed by this operation. </returns>
    public bool Merge(SymbolTable symbolTable, Frame dstFrame, int catchTypeIndex)
    {
        bool frameChanged = false;

        // Compute the concrete types of the local variables at the end of the basic block corresponding
        // to this frame, by resolving its abstract output types, and merge these concrete types with
        // those of the local variables in the input frame of dstFrame.
        int numLocal = _inputLocals.Length;
        int numStack = _inputStack.Length;
        if (dstFrame._inputLocals == null)
        {
            dstFrame._inputLocals = new int[numLocal];
            frameChanged = true;
        }

        for (int i = 0; i < numLocal; ++i)
        {
            int concreteOutputType;
            if (_outputLocals != null && i < _outputLocals.Length)
            {
                int abstractOutputType = _outputLocals[i];
                if (abstractOutputType == 0)
                {
                    // If the local variable has never been assigned in this basic block, it is equal to its
                    // value at the beginning of the block.
                    concreteOutputType = _inputLocals[i];
                }
                else
                {
                    concreteOutputType = GetConcreteOutputType(abstractOutputType, numStack);
                }
            }
            else
            {
                // If the local variable has never been assigned in this basic block, it is equal to its
                // value at the beginning of the block.
                concreteOutputType = _inputLocals[i];
            }

            // concreteOutputType might be an uninitialized type from the input locals or from the input
            // stack. However, if a constructor has been called for this class type in the basic block,
            // then this type is no longer uninitialized at the end of basic block.
            if (_initializations != null)
            {
                concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
            }

            frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame._inputLocals, i);
        }

        // If dstFrame is an exception handler block, it can be reached from any instruction of the
        // basic block corresponding to this frame, in particular from the first one. Therefore, the
        // input locals of dstFrame should be compatible (i.e. merged) with the input locals of this
        // frame (and the input stack of dstFrame should be compatible, i.e. merged, with a one
        // element stack containing the caught exception type).
        if (catchTypeIndex > 0)
        {
            for (int i = 0; i < numLocal; ++i)
            {
                frameChanged |= Merge(symbolTable, _inputLocals[i], dstFrame._inputLocals, i);
            }

            if (dstFrame._inputStack == null)
            {
                dstFrame._inputStack = new int[1];
                frameChanged = true;
            }

            frameChanged |= Merge(symbolTable, catchTypeIndex, dstFrame._inputStack, 0);
            return frameChanged;
        }

        // Compute the concrete types of the stack operands at the end of the basic block corresponding
        // to this frame, by resolving its abstract output types, and merge these concrete types with
        // those of the stack operands in the input frame of dstFrame.
        int numInputStack = _inputStack.Length + _outputStackStart;
        if (dstFrame._inputStack == null)
        {
            dstFrame._inputStack = new int[numInputStack + _outputStackTop];
            frameChanged = true;
        }

        // First, do this for the stack operands that have not been popped in the basic block
        // corresponding to this frame, and which are therefore equal to their value in the input
        // frame (except for uninitialized types, which may have been initialized).
        for (int i = 0; i < numInputStack; ++i)
        {
            int concreteOutputType = _inputStack[i];
            if (_initializations != null)
            {
                concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
            }

            frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame._inputStack, i);
        }

        // Then, do this for the stack operands that have pushed in the basic block (this code is the
        // same as the one above for local variables).
        for (int i = 0; i < _outputStackTop; ++i)
        {
            int abstractOutputType = _outputStack[i];
            int concreteOutputType = GetConcreteOutputType(abstractOutputType, numStack);
            if (_initializations != null)
            {
                concreteOutputType = GetInitializedType(symbolTable, concreteOutputType);
            }

            frameChanged |= Merge(symbolTable, concreteOutputType, dstFrame._inputStack, numInputStack + i);
        }

        return frameChanged;
    }

    /// <summary>
    /// Merges the type at the given index in the given abstract type array with the given type.
    /// Returns <c>true</c> if the type array has been modified by this operation.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="sourceType"> the abstract type with which the abstract type array element must be merged.
    ///     This type should be of <see cref="ConstantKind"/>, <see cref="ReferenceKind"/>, <see cref="UninitializedKind"/>
    ///     or <see cref="ForwardUninitializedKind"/> kind, with positive or <c>null</c> array dimensions. </param>
    /// <param name="dstTypes"> an array of abstract types. These types should be of <see cref="ConstantKind"/>,
    ///     <see cref="ReferenceKind"/>, <see cref="UninitializedKind"/> or <see cref="ForwardUninitializedKind"/>
    ///     kind, with positive or <c>null</c> array dimensions. </param>
    /// <param name="dstIndex"> the index of the type that must be merged in dstTypes. </param>
    /// <returns> <c>true</c> if the type array has been modified by this operation. </returns>
    private static bool Merge(SymbolTable symbolTable, int sourceType, int[] dstTypes, int dstIndex)
    {
        int dstType = dstTypes[dstIndex];
        if (dstType == sourceType)
        {
            // If the types are equal, merge(sourceType, dstType) = dstType, so there is no change.
            return false;
        }

        int srcType = sourceType;
        if ((sourceType & ~DimMask) == Null)
        {
            if (dstType == Null)
            {
                return false;
            }

            srcType = Null;
        }

        if (dstType == 0)
        {
            // If dstTypes[dstIndex] has never been assigned, merge(srcType, dstType) = srcType.
            dstTypes[dstIndex] = srcType;
            return true;
        }

        int mergedType;
        if ((dstType & DimMask) != 0 || (dstType & KindMask) == ReferenceKind)
        {
            // If dstType is a reference type of any array dimension.
            if (srcType == Null)
            {
                // If srcType is the NULL type, merge(srcType, dstType) = dstType, so there is no change.
                return false;
            }
            else if ((srcType & (DimMask | KindMask)) == (dstType & (DimMask | KindMask)))
            {
                // If srcType has the same array dimension and the same kind as dstType.
                if ((dstType & KindMask) == ReferenceKind)
                {
                    // If srcType and dstType are reference types with the same array dimension,
                    // merge(srcType, dstType) = dim(srcType) | common super class of srcType and dstType.
                    mergedType = (srcType & DimMask) | ReferenceKind |
                                 symbolTable.AddMergedType(srcType & ValueMask, dstType & ValueMask);
                }
                else
                {
                    // If srcType and dstType are array types of equal dimension but different element types,
                    // merge(srcType, dstType) = dim(srcType) - 1 | java/lang/Object.
                    int mergedDim = ElementOf + (srcType & DimMask);
                    mergedType = mergedDim | ReferenceKind | symbolTable.AddType("java/lang/Object");
                }
            }
            else if ((srcType & DimMask) != 0 || (srcType & KindMask) == ReferenceKind)
            {
                // If srcType is any other reference or array type,
                // merge(srcType, dstType) = min(srcDdim, dstDim) | java/lang/Object
                // where srcDim is the array dimension of srcType, minus 1 if srcType is an array type
                // with a non reference element type (and similarly for dstDim).
                int srcDim = srcType & DimMask;
                if (srcDim != 0 && (srcType & KindMask) != ReferenceKind)
                {
                    srcDim = ElementOf + srcDim;
                }

                int dstDim = dstType & DimMask;
                if (dstDim != 0 && (dstType & KindMask) != ReferenceKind)
                {
                    dstDim = ElementOf + dstDim;
                }

                mergedType = Math.Min(srcDim, dstDim) | ReferenceKind | symbolTable.AddType("java/lang/Object");
            }
            else
            {
                // If srcType is any other type, merge(srcType, dstType) = TOP.
                mergedType = Top;
            }
        }
        else if (dstType == Null)
        {
            // If dstType is the NULL type, merge(srcType, dstType) = srcType, or TOP if srcType is not a
            // an array type or a reference type.
            mergedType = (srcType & DimMask) != 0 || (srcType & KindMask) == ReferenceKind ? srcType : Top;
        }
        else
        {
            // If dstType is any other type, merge(srcType, dstType) = TOP whatever srcType.
            mergedType = Top;
        }

        if (mergedType != dstType)
        {
            dstTypes[dstIndex] = mergedType;
            return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------------------------------
    // Frame output methods, to generate StackMapFrame attributes
    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Makes the given <see cref="MethodWriter"/> visit the input frame of this <see cref="Frame"/>. The visit is
    /// done with the <see cref="MethodWriter.VisitFrameStart"/>, <see cref="MethodWriter.VisitAbstractType"/> and
    /// <see cref="MethodWriter.VisitFrameEnd"/> methods.
    /// </summary>
    /// <param name="methodWriter"> the <see cref="MethodWriter"/> that should visit the input frame of this <see cref="Frame"/>. </param>
    public void Accept(MethodWriter methodWriter)
    {
        // Compute the number of locals, ignoring TOP types that are just after a LONG or a DOUBLE, and
        // all trailing TOP types.
        int[] localTypes = _inputLocals;
        int numLocal = 0;
        int numTrailingTop = 0;
        int i = 0;
        while (i < localTypes.Length)
        {
            int localType = localTypes[i];
            i += (localType == Long || localType == Double) ? 2 : 1;
            if (localType == Top)
            {
                numTrailingTop++;
            }
            else
            {
                numLocal += numTrailingTop + 1;
                numTrailingTop = 0;
            }
        }

        // Compute the stack size, ignoring TOP types that are just after a LONG or a DOUBLE.
        int[] stackTypes = _inputStack;
        int numStack = 0;
        i = 0;
        while (i < stackTypes.Length)
        {
            int stackType = stackTypes[i];
            i += (stackType == Long || stackType == Double) ? 2 : 1;
            numStack++;
        }

        // Visit the frame and its content.
        int frameIndex = methodWriter.VisitFrameStart(owner.bytecodeOffset, numLocal, numStack);
        i = 0;
        while (numLocal-- > 0)
        {
            int localType = localTypes[i];
            i += (localType == Long || localType == Double) ? 2 : 1;
            methodWriter.VisitAbstractType(frameIndex++, localType);
        }

        i = 0;
        while (numStack-- > 0)
        {
            int stackType = stackTypes[i];
            i += (stackType == Long || stackType == Double) ? 2 : 1;
            methodWriter.VisitAbstractType(frameIndex++, stackType);
        }

        methodWriter.VisitFrameEnd();
    }

    /// <summary>
    /// Put the given abstract type in the given ByteVector, using the JVMS verification_type_info
    /// format used in StackMapTable attributes.
    /// </summary>
    /// <param name="symbolTable"> the type table to use to lookup and store type <see cref="Symbol"/>. </param>
    /// <param name="abstractType"> an abstract type, restricted to <see cref="ConstantKind"/>, <see cref="ReferenceKind"/>,
    /// <see cref="UninitializedKind"/> or <see cref="ForwardUninitializedKind"/> types. </param>
    /// <param name="output"> where the abstract type must be put. </param>
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4">See also: JVMS 4.7.4</a>
    internal static void PutAbstractType(SymbolTable symbolTable, int abstractType, ByteVector output)
    {
        int arrayDimensions = (abstractType & Frame.DimMask) >> DimShift;
        if (arrayDimensions == 0)
        {
            int typeValue = abstractType & ValueMask;
            switch (abstractType & KindMask)
            {
                case ConstantKind:
                    output.PutByte(typeValue);
                    break;
                case ReferenceKind:
                    output.PutByte(Item_Object)
                        .PutShort(symbolTable.AddConstantClass(symbolTable.GetType(typeValue).value).index);
                    break;
                case UninitializedKind:
                    output.PutByte(Item_Uninitialized).PutShort((int)symbolTable.GetType(typeValue).data);
                    break;
                case ForwardUninitializedKind:
                    output.PutByte(Item_Uninitialized);
                    symbolTable.GetForwardUninitializedLabel(typeValue).Put(output);
                    break;
                default:
                    throw new Exception("AssertionError");
            }
        }
        else
        {
            // Case of an array type, we need to build its descriptor first.
            StringBuilder typeDescriptor = new StringBuilder();
            while (arrayDimensions-- > 0)
            {
                typeDescriptor.Append('[');
            }

            if ((abstractType & KindMask) == ReferenceKind)
            {
                typeDescriptor.Append('L').Append(symbolTable.GetType(abstractType & ValueMask).value).Append(';');
            }
            else
            {
                switch (abstractType & ValueMask)
                {
                    case Frame.ItemAsmBoolean:
                        typeDescriptor.Append('Z');
                        break;
                    case Frame.ItemAsmByte:
                        typeDescriptor.Append('B');
                        break;
                    case Frame.ItemAsmChar:
                        typeDescriptor.Append('C');
                        break;
                    case Frame.ItemAsmShort:
                        typeDescriptor.Append('S');
                        break;
                    case Frame.Item_Integer:
                        typeDescriptor.Append('I');
                        break;
                    case Frame.Item_Float:
                        typeDescriptor.Append('F');
                        break;
                    case Frame.Item_Long:
                        typeDescriptor.Append('J');
                        break;
                    case Frame.Item_Double:
                        typeDescriptor.Append('D');
                        break;
                    default:
                        throw new Exception("AssertionError");
                }
            }

            output.PutByte(Item_Object).PutShort(symbolTable.AddConstantClass(typeDescriptor.ToString()).index);
        }
    }
}