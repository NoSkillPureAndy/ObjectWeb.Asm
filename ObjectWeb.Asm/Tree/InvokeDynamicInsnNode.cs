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
/// A node that represents an invokedynamic instruction.
/// 
/// @author Remi Forax
/// </summary>
public class InvokeDynamicInsnNode : AbstractInsnNode
{
    /// <summary>
    /// The method's name. </summary>
    public string Name { get; set; }

    /// <summary>
    /// The method's descriptor (see <seealso cref = "org.objectweb.asm.JType"/>). </summary>
    public string Desc { get; set; }

    /// <summary>
    /// The bootstrap method. </summary>
    public Handle Bsm { get; set; }

    /// <summary>
    /// The bootstrap method constant arguments. </summary>
    public object[] BsmArgs { get; set; }

    /// <summary>
    /// Constructs a new <seealso cref = "InvokeDynamicInsnNode"/>.
    /// </summary>
    /// <param name = "name"> the method's name. </param>
    /// <param name = "descriptor"> the method's descriptor (see <seealso cref = "org.objectweb.asm.JType"/>). </param>
    /// <param name = "bootstrapMethodHandle"> the bootstrap method. </param>
    /// <param name = "bootstrapMethodArguments"> the bootstrap method constant arguments. Each argument must be
    ///     an <seealso cref = "Integer"/>, <seealso cref = "Float"/>, <seealso cref = "Long"/>, <seealso cref = "Double"/>, <seealso cref = "string "/>, <see cref="org.objectweb.asm.JType"/> or <seealso cref = "Handle"/> value. This method is allowed to modify the
    ///     content of the array so a caller should expect that this array may change. </param>
    public InvokeDynamicInsnNode(string name, string descriptor, Handle bootstrapMethodHandle,
        params object[] bootstrapMethodArguments) : base(Opcodes.Invokedynamic)
    {
        this.Name = name;
        this.Desc = descriptor;
        this.Bsm = bootstrapMethodHandle;
        this.BsmArgs = bootstrapMethodArguments;
    }

    public override int Type => Invoke_Dynamic_Insn;

    public override void Accept(MethodVisitor methodVisitor)
    {
        methodVisitor.VisitInvokeDynamicInsn(Name, Desc, Bsm, BsmArgs);
        AcceptAnnotations(methodVisitor);
    }

    public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
    {
        return (new InvokeDynamicInsnNode(Name, Desc, Bsm, BsmArgs)).CloneAnnotations(this);
    }
}