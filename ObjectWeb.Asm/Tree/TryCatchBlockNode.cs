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
/// A node that represents a try catch block.
/// 
/// @author Eric Bruneton
/// </summary>
public class TryCatchBlockNode
{
    /// <summary>
    /// The beginning of the exception handler's scope (inclusive). </summary>
    public LabelNode Start { get; set; }

    /// <summary>
    /// The end of the exception handler's scope (exclusive). </summary>
    public LabelNode End { get; set; }

    /// <summary>
    /// The beginning of the exception handler's code. </summary>
    public LabelNode Handler { get; set; }

    /// <summary>
    /// The internal name of the type of exceptions handled by the handler. May be <c>null</c> to
    /// catch any exceptions (for "finally" blocks).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The runtime visible type annotations on the exception handler type. May be <c>null</c>. </summary>
    public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

    /// <summary>
    /// The runtime invisible type annotations on the exception handler type. May be <c>null</c>.
    /// </summary>
    public List<TypeAnnotationNode> InvisibleTypeAnnotations { get; set; }

    /// <summary>
    /// Constructs a new <seealso cref = "TryCatchBlockNode"/>.
    /// </summary>
    /// <param name = "start"> the beginning of the exception handler's scope (inclusive). </param>
    /// <param name = "end"> the end of the exception handler's scope (exclusive). </param>
    /// <param name = "handler"> the beginning of the exception handler's code. </param>
    /// <param name = "type"> the internal name of the type of exceptions handled by the handler (see <see cref="JType.InternalName"/>), or <c>null</c> to catch any exceptions (for "finally" blocks). </param>
    public TryCatchBlockNode(LabelNode start, LabelNode end, LabelNode handler, string type)
    {
        this.Start = start;
        this.End = end;
        this.Handler = handler;
        this.Type = type;
    }

    /// <summary>
    /// Updates the index of this try catch block in the method's list of try catch block nodes. This
    /// index maybe stored in the 'target' field of the type annotations of this block.
    /// </summary>
    /// <param name = "index"> the new index of this try catch block in the method's list of try catch block
    ///     nodes. </param>
    public virtual void UpdateIndex(int index)
    {
        int newTypeRef = 0x42000000 | (index << 8);
        if (VisibleTypeAnnotations != null)
        {
            for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
            {
                VisibleTypeAnnotations[i].TypeRef = newTypeRef;
            }
        }

        if (InvisibleTypeAnnotations != null)
        {
            for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
            {
                InvisibleTypeAnnotations[i].TypeRef = newTypeRef;
            }
        }
    }

    /// <summary>
    /// Makes the given visitor visit this try catch block.
    /// </summary>
    /// <param name = "methodVisitor"> a method visitor. </param>
    public virtual void Accept(MethodVisitor methodVisitor)
    {
        methodVisitor.VisitTryCatchBlock(Start.Label, End.Label, Handler == null ? null : Handler.Label, Type);
        if (VisibleTypeAnnotations != null)
        {
            for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode typeAnnotation = VisibleTypeAnnotations[i];
                typeAnnotation.Accept(methodVisitor.VisitTryCatchAnnotation(typeAnnotation.TypeRef,
                    typeAnnotation.TypePath, typeAnnotation.Desc, true));
            }
        }

        if (InvisibleTypeAnnotations != null)
        {
            for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
            {
                TypeAnnotationNode typeAnnotation = InvisibleTypeAnnotations[i];
                typeAnnotation.Accept(methodVisitor.VisitTryCatchAnnotation(typeAnnotation.TypeRef,
                    typeAnnotation.TypePath, typeAnnotation.Desc, false));
            }
        }
    }
}