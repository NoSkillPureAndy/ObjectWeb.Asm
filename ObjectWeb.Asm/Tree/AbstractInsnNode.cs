using System.Collections.Generic;

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
namespace ObjectWeb.Asm.Tree;

/// <summary>
/// A node that represents a bytecode instruction. <i>An instruction can appear at most once in at
/// most one <seealso cref = "InsnList"/> at a time</i>.
/// 
/// @author Eric Bruneton
/// </summary>
public abstract class AbstractInsnNode
{
    /// <summary>
    /// The type of <seealso cref = "InsnNode"/> instructions. </summary>
    public const int Insn = 0;

    /// <summary>
    /// The type of <seealso cref = "IntInsnNode"/> instructions. </summary>
    public const int Int_Insn = 1;

    /// <summary>
    /// The type of <seealso cref = "VarInsnNode"/> instructions. </summary>
    public const int Var_Insn = 2;

    /// <summary>
    /// The type of <seealso cref = "TypeInsnNode"/> instructions. </summary>
    public const int Type_Insn = 3;

    /// <summary>
    /// The type of <seealso cref = "FieldInsnNode"/> instructions. </summary>
    public const int Field_Insn = 4;

    /// <summary>
    /// The type of <seealso cref = "MethodInsnNode"/> instructions. </summary>
    public const int Method_Insn = 5;

    /// <summary>
    /// The type of <seealso cref = "InvokeDynamicInsnNode"/> instructions. </summary>
    public const int Invoke_Dynamic_Insn = 6;

    /// <summary>
    /// The type of <seealso cref = "JumpInsnNode"/> instructions. </summary>
    public const int Jump_Insn = 7;

    /// <summary>
    /// The type of <seealso cref = "LabelNode"/> "instructions". </summary>
    public const int Label_Insn = 8;

    /// <summary>
    /// The type of <seealso cref = "LdcInsnNode"/> instructions. </summary>
    public const int Ldc_Insn = 9;

    /// <summary>
    /// The type of <seealso cref = "IincInsnNode"/> instructions. </summary>
    public const int Iinc_Insn = 10;

    /// <summary>
    /// The type of <seealso cref = "TableSwitchInsnNode"/> instructions. </summary>
    public const int Tableswitch_Insn = 11;

    /// <summary>
    /// The type of <seealso cref = "LookupSwitchInsnNode"/> instructions. </summary>
    public const int Lookupswitch_Insn = 12;

    /// <summary>
    /// The type of <seealso cref = "MultiANewArrayInsnNode"/> instructions. </summary>
    public const int Multianewarray_Insn = 13;

    /// <summary>
    /// The type of <seealso cref = "FrameNode"/> "instructions". </summary>
    public const int Frame_Insn = 14;

    /// <summary>
    /// The type of <seealso cref = "LineNumberNode"/> "instructions". </summary>
    public const int Line_Insn = 15;

    /// <summary>
    /// The opcode of this instruction. </summary>
    protected internal int opcode;

    /// <summary>
    /// The runtime visible type annotations of this instruction. This field is only used for real
    /// instructions (i.e. not for labels, frames, or line number nodes). This list is a list of <see cref="TypeAnnotationNode"/> objects. May be null.
    /// </summary>
    public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

    /// <summary>
    /// The runtime invisible type annotations of this instruction. This field is only used for real
    /// instructions (i.e. not for labels, frames, or line number nodes). This list is a list of <see cref="TypeAnnotationNode"/> objects. May be null.
    /// </summary>
    public List<TypeAnnotationNode> InvisibleTypeAnnotations { get; set; }

    /// <summary>
    /// The previous instruction in the list to which this instruction belongs. </summary>
    internal AbstractInsnNode previousInsn;

    /// <summary>
    /// The next instruction in the list to which this instruction belongs. </summary>
    internal AbstractInsnNode nextInsn;

    /// <summary>
    /// The index of this instruction in the list to which it belongs. The value of this field is
    /// correct only when <seealso cref = "InsnList.cache"/> is not null. A value of -1 indicates that this
    /// instruction does not belong to any <seealso cref = "InsnList"/>.
    /// </summary>
    internal int index;

    /// <summary>
    /// Constructs a new <seealso cref = "AbstractInsnNode"/>.
    /// </summary>
    /// <param name = "opcode"> the opcode of the instruction to be constructed. </param>
    public AbstractInsnNode(int opcode)
    {
        this.opcode = opcode;
        this.index = -1;
    }

    /// <summary>
    /// Returns the opcode of this instruction.
    /// </summary>
    /// <returns> the opcode of this instruction. </returns>
    public virtual int Opcode => opcode;

    /// <summary>
    /// Returns the type of this instruction.
    /// </summary>
    /// <returns> the type of this instruction, i.e. one the constants defined in this class. </returns>
    public abstract int Type { get; }

    /// <summary>
    /// Returns the previous instruction in the list to which this instruction belongs, if any.
    /// </summary>
    /// <returns> the previous instruction in the list to which this instruction belongs, if any. May be
    ///     null. </returns>
    public virtual AbstractInsnNode Previous => previousInsn;

    /// <summary>
    /// Returns the next instruction in the list to which this instruction belongs, if any.
    /// </summary>
    /// <returns> the next instruction in the list to which this instruction belongs, if any. May be
    ///     null. </returns>
    public virtual AbstractInsnNode Next => nextInsn;

    /// <summary>
    /// Makes the given method visitor visit this instruction.
    /// </summary>
    /// <param name = "methodVisitor"> a method visitor. </param>
    public abstract void Accept(MethodVisitor methodVisitor);

    /// <summary>
    /// Makes the given visitor visit the annotations of this instruction.
    /// </summary>
    /// <param name = "methodVisitor"> a method visitor. </param>
    public void AcceptAnnotations(MethodVisitor methodVisitor)
    {
        if (VisibleTypeAnnotations != null)
        {
            for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode typeAnnotation = VisibleTypeAnnotations[i];
                typeAnnotation.Accept(methodVisitor.VisitInsnAnnotation(typeAnnotation.TypeRef,
                    typeAnnotation.TypePath, typeAnnotation.Desc, true));
            }
        }

        if (InvisibleTypeAnnotations != null)
        {
            for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode typeAnnotation = InvisibleTypeAnnotations[i];
                typeAnnotation.Accept(methodVisitor.VisitInsnAnnotation(typeAnnotation.TypeRef,
                    typeAnnotation.TypePath, typeAnnotation.Desc, false));
            }
        }
    }

    /// <summary>
    /// Returns a copy of this instruction.
    /// </summary>
    /// <param name = "clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
    /// <returns> a copy of this instruction. The returned instruction does not belong to any <see cref="InsnList"/>. </returns>
    public abstract AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels);

    /// <summary>
    /// Returns the clone of the given label.
    /// </summary>
    /// <param name = "label"> a label. </param>
    /// <param name = "clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
    /// <returns> the clone of the given label. </returns>
    internal static LabelNode Clone(LabelNode label, IDictionary<LabelNode, LabelNode> clonedLabels)
    {
        clonedLabels.TryGetValue(label, out LabelNode ret);
        return ret;
    }

    /// <summary>
    /// Returns the clones of the given labels.
    /// </summary>
    /// <param name = "labels"> a list of labels. </param>
    /// <param name = "clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
    /// <returns> the clones of the given labels. </returns>
    internal static LabelNode[] Clone(List<LabelNode> labels, IDictionary<LabelNode, LabelNode> clonedLabels)
    {
        LabelNode[] clones = new LabelNode[labels.Count];
        for (int i = 0, n = clones.Length; i < n; ++i)
        {
            clonedLabels.TryGetValue(labels[i], out clones[i]);
        }

        return clones;
    }

    /// <summary>
    /// Clones the annotations of the given instruction into this instruction.
    /// </summary>
    /// <param name = "insnNode"> the source instruction. </param>
    /// <returns> this instruction. </returns>
    public AbstractInsnNode CloneAnnotations(AbstractInsnNode insnNode)
    {
        if (insnNode.VisibleTypeAnnotations != null)
        {
            this.VisibleTypeAnnotations = new List<TypeAnnotationNode>();
            for (int i = 0, n = insnNode.VisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode sourceAnnotation = insnNode.VisibleTypeAnnotations[i];
                TypeAnnotationNode cloneAnnotation = new TypeAnnotationNode(sourceAnnotation.TypeRef, sourceAnnotation.TypePath,
                    sourceAnnotation.Desc);
                sourceAnnotation.Accept(cloneAnnotation);
                this.VisibleTypeAnnotations.Add(cloneAnnotation);
            }
        }

        if (insnNode.InvisibleTypeAnnotations != null)
        {
            this.InvisibleTypeAnnotations = new List<TypeAnnotationNode>();
            for (int i = 0, n = insnNode.InvisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode sourceAnnotation = insnNode.InvisibleTypeAnnotations[i];
                TypeAnnotationNode cloneAnnotation = new TypeAnnotationNode(sourceAnnotation.TypeRef, sourceAnnotation.TypePath,
                    sourceAnnotation.Desc);
                sourceAnnotation.Accept(cloneAnnotation);
                this.InvisibleTypeAnnotations.Add(cloneAnnotation);
            }
        }

        return this;
    }
}