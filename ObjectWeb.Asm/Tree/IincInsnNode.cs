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
/// A node that represents an IINC instruction.
/// 
/// @author Eric Bruneton
/// </summary>
public class IincInsnNode : AbstractInsnNode
{
    /// <summary>
    /// Index of the local variable to be incremented. </summary>
    public int Var { get; set; }

    /// <summary>
    /// Amount to increment the local variable by. </summary>
    public int Incr { get; set; }

    /// <summary>
    /// Constructs a new <seealso cref = "IincInsnNode"/>.
    /// </summary>
    /// <param name = "var"> index of the local variable to be incremented. </param>
    /// <param name = "incr"> increment amount to increment the local variable by. </param>
    public IincInsnNode(int var, int incr) : base(Opcodes.Iinc)
    {
        this.Var = var;
        this.Incr = incr;
    }

    public override int Type => Iinc_Insn;

    public override void Accept(MethodVisitor methodVisitor)
    {
        methodVisitor.VisitIincInsn(Var, Incr);
        AcceptAnnotations(methodVisitor);
    }

    public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
    {
        return (new IincInsnNode(Var, Incr)).CloneAnnotations(this);
    }
}