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
/// A visitor to visit a Java field. The methods of this class must be called in the following order:
/// ( <c>visitAnnotation</c> | <c>visitTypeAnnotation</c> | <c>visitAttribute</c> )* <c>visitEnd</c>.
/// 
/// @author Eric Bruneton
/// </summary>
public abstract class FieldVisitor
{
    /// <summary>
    /// The ASM API version implemented by this visitor. The value of this field must be one of the
    /// <c>ASM</c><i>x</i> Values in <see cref="Opcodes"/>.
    /// </summary>
    protected internal readonly int api;

    /// <summary>
    /// The field visitor to which this visitor must delegate method calls. May be <c>null</c>. </summary>
    protected internal FieldVisitor fv;

    /// <summary>
    /// Constructs a new <see cref="FieldVisitor"/>.
    /// </summary>
    /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the <c>ASM</c><i>x</i> Values in <see cref="Opcodes"/>. </param>
    public FieldVisitor(int api) : this(api, null)
    {
    }

    /// <summary>
    /// Constructs a new <see cref="FieldVisitor"/>.
    /// </summary>
    /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the <c>ASM</c><i>x</i> Values in <see cref="Opcodes"/>. </param>
    /// <param name="fieldVisitor"> the field visitor to which this visitor must delegate method calls. May be
    ///     null. </param>
    public FieldVisitor(int api, FieldVisitor fieldVisitor)
    {
        if (api != Opcodes.Asm9 && api != Opcodes.Asm8 && api != Opcodes.Asm7 && api != Opcodes.Asm6 &&
            api != Opcodes.Asm5 && api != Opcodes.Asm4 && api != Opcodes.Asm10_Experimental)
        {
            throw new System.ArgumentException("Unsupported api " + api);
        }

        if (api == Opcodes.Asm10_Experimental)
        {
            Constants.CheckAsmExperimental(this);
        }

        this.api = api;
        this.fv = fieldVisitor;
    }

    /// <summary>
    /// The field visitor to which this visitor must delegate method calls. May be <c>null</c>.
    /// </summary>
    public FieldVisitor Delegate => fv;

    /// <summary>
    /// Visits an annotation of the field.
    /// </summary>
    /// <param name="descriptor"> the class descriptor of the annotation class. </param>
    /// <param name="visible"> <c>true</c> if the annotation is visible at runtime. </param>
    /// <returns> a visitor to visit the annotation Values, or <c>null</c> if this visitor is not
    ///     interested in visiting this annotation. </returns>
    public virtual AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
    {
        if (fv != null)
        {
            return fv.VisitAnnotation(descriptor, visible);
        }

        return null;
    }

    /// <summary>
    /// Visits an annotation on the type of the field.
    /// </summary>
    /// <param name="typeRef"> a reference to the annotated type. The sort of this type reference must be
    ///     <see cref="TypeReference.Field"/>. See <see cref="TypeReference"/>. </param>
    /// <param name="typePath"> the path to the annotated type argument, wildcard bound, array element type, or
    ///     static inner type within 'typeRef'. May be <c>null</c> if the annotation targets
    ///     'typeRef' as a whole. </param>
    /// <param name="descriptor"> the class descriptor of the annotation class. </param>
    /// <param name="visible"> <c>true</c> if the annotation is visible at runtime. </param>
    /// <returns> a visitor to visit the annotation Values, or <c>null</c> if this visitor is not
    ///     interested in visiting this annotation. </returns>
    public virtual AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
        bool visible)
    {
        if (api < Opcodes.Asm5)
        {
            throw new System.NotSupportedException("This feature requires ASM5");
        }

        if (fv != null)
        {
            return fv.VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
        }

        return null;
    }

    /// <summary>
    /// Visits a non standard attribute of the field.
    /// </summary>
    /// <param name="attribute"> an attribute. </param>
    public virtual void VisitAttribute(Attribute attribute)
    {
        if (fv != null)
        {
            fv.VisitAttribute(attribute);
        }
    }

    /// <summary>
    /// Visits the end of the field. This method, which is the last one to be called, is used to inform
    /// the visitor that all the annotations and attributes of the field have been visited.
    /// </summary>
    public virtual void VisitEnd()
    {
        if (fv != null)
        {
            fv.VisitEnd();
        }
    }
}