using System;

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

namespace ObjectWeb.Asm.Commons;

/// <summary>
///     An <see cref="AnnotationVisitor" /> that remaps types with a <see cref="Remapper" />.
///     @author Eugene Kuleshov
/// </summary>
public class AnnotationRemapper : AnnotationVisitor
{
    /// <summary>
    ///     The descriptor of the visited annotation. May be <c>null</c>, for instance for
    ///     AnnotationDefault.
    /// </summary>
    protected internal readonly string descriptor;

    /// <summary>
    ///     The remapper used to remap the types in the visited annotation.
    /// </summary>
    protected internal readonly Remapper remapper;

    /// <summary>
    ///     Constructs a new <see cref="AnnotationRemapper" />. <i>Subclasses must not use this constructor</i>.
    ///     Instead, they must use the <see cref="AnnotationRemapper(int,AnnotationVisitor,Remapper)" /> version.
    /// </summary>
    /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
    /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
    /// @deprecated use
    /// <see cref="AnnotationRemapper(string, AnnotationVisitor, Remapper)" />
    /// instead.
    [Obsolete("use <seealso cref=\"AnnotationRemapper(String, AnnotationVisitor, Remapper)\"/> instead.")]
    public AnnotationRemapper(AnnotationVisitor annotationVisitor, Remapper remapper) : this(null,
        annotationVisitor, remapper)
    {
    }

    /// <summary>
    ///     Constructs a new <see cref="AnnotationRemapper" />. <i>Subclasses must not use this constructor</i>.
    ///     Instead, they must use the <see cref="AnnotationRemapper(int,string,AnnotationVisitor,Remapper)" />
    ///     version.
    /// </summary>
    /// <param name="descriptor"> the descriptor of the visited annotation. May be <c>null</c>. </param>
    /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
    /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
    public AnnotationRemapper(string descriptor, AnnotationVisitor annotationVisitor, Remapper remapper) : this(
        Opcodes.Asm9, descriptor, annotationVisitor, remapper)
    {
    }

    /// <summary>
    ///     Constructs a new <see cref="AnnotationRemapper" />.
    /// </summary>
    /// <param name="api">
    ///     the ASM API version supported by this remapper. Must be one of the <c>ASM</c><i>x</i> Values in <see cref="Opcodes" />.
    /// </param>
    /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
    /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
    /// @deprecated use
    /// <see cref="AnnotationRemapper(int, string, AnnotationVisitor, Remapper)" />
    /// instead.
    [Obsolete("use <seealso cref=\"AnnotationRemapper(int, String, AnnotationVisitor, Remapper)\"/> instead.")]
    public AnnotationRemapper(int api, AnnotationVisitor annotationVisitor, Remapper remapper) : this(api, null,
        annotationVisitor, remapper)
    {
    }

    /// <summary>
    ///     Constructs a new <see cref="AnnotationRemapper" />.
    /// </summary>
    /// <param name="api">
    ///     the ASM API version supported by this remapper. Must be one of the <c>ASM</c><i>x</i> Values in <see cref="Opcodes" />.
    /// </param>
    /// <param name="descriptor"> the descriptor of the visited annotation. May be <c>null</c>. </param>
    /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
    /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
    public AnnotationRemapper(int api, string descriptor, AnnotationVisitor annotationVisitor, Remapper remapper) :
        base(api, annotationVisitor)
    {
        this.descriptor = descriptor;
        this.remapper = remapper;
    }

    public override void Visit(string name, object value)
    {
        base.Visit(MapAnnotationAttributeName(name), remapper.MapValue(value));
    }

    public override void VisitEnum(string name, string descriptor, string value)
    {
        base.VisitEnum(MapAnnotationAttributeName(name), remapper.MapDesc(descriptor), value);
    }

    public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
    {
        AnnotationVisitor annotationVisitor =
            base.VisitAnnotation(MapAnnotationAttributeName(name), remapper.MapDesc(descriptor));
        if (annotationVisitor == null)
            return null;
        return annotationVisitor == av ? this : CreateAnnotationRemapper(descriptor, annotationVisitor);
    }

    public override AnnotationVisitor VisitArray(string name)
    {
        AnnotationVisitor annotationVisitor = base.VisitArray(MapAnnotationAttributeName(name));
        if (annotationVisitor == null)
            return null;
        return annotationVisitor == av ? this : CreateAnnotationRemapper(null, annotationVisitor);
    }

    /// <summary>
    ///     Constructs a new remapper for annotations. The default implementation of this method returns a
    ///     new <see cref="AnnotationRemapper" />.
    /// </summary>
    /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
    /// <returns> the newly created remapper. </returns>
    /// @deprecated use
    /// <see cref="CreateAnnotationRemapper(string,ObjectWeb.Asm.AnnotationVisitor)" />
    /// instead.
    [Obsolete("use <seealso cref=\"createAnnotationRemapper(String, AnnotationVisitor)\"/> instead.")]
    public virtual AnnotationVisitor CreateAnnotationRemapper(AnnotationVisitor annotationVisitor)
    {
        return new AnnotationRemapper(api, null, annotationVisitor, remapper);
    }

    /// <summary>
    ///     Constructs a new remapper for annotations. The default implementation of this method returns a
    ///     new <see cref="AnnotationRemapper" />.
    /// </summary>
    /// <param name="descriptor"> the descriptor of the visited annotation. </param>
    /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
    /// <returns> the newly created remapper. </returns>
    public virtual AnnotationVisitor CreateAnnotationRemapper(string descriptor,
        AnnotationVisitor annotationVisitor)
    {
        return new AnnotationRemapper(api, descriptor, annotationVisitor, remapper).OrDeprecatedValue(
            CreateAnnotationRemapper(annotationVisitor));
    }

    /// <summary>
    ///     Returns either this object, or the given one. If the given object is equal to the object
    ///     returned by the default implementation of the deprecated createAnnotationRemapper method,
    ///     meaning that this method has not been overridden (or only in minor ways, for instance to add
    ///     logging), then we can return this object instead, supposed to have been created by the new
    ///     createAnnotationRemapper method. Otherwise we must return the given object.
    /// </summary>
    /// <param name="deprecatedAnnotationVisitor">
    ///     the result of a call to the deprecated
    ///     createAnnotationRemapper method.
    /// </param>
    /// <returns> either this object, or the given one. </returns>
    public AnnotationVisitor OrDeprecatedValue(AnnotationVisitor deprecatedAnnotationVisitor)
    {
        if (deprecatedAnnotationVisitor.GetType() == GetType())
        {
            AnnotationRemapper deprecatedAnnotationRemapper = (AnnotationRemapper)deprecatedAnnotationVisitor;
            if (deprecatedAnnotationRemapper.api == api && deprecatedAnnotationRemapper.av == av &&
                deprecatedAnnotationRemapper.remapper == remapper) return this;
        }

        return deprecatedAnnotationVisitor;
    }

    /// <summary>
    ///     Maps an annotation attribute name with the remapper. Returns the original name unchanged if the
    ///     descriptor of the annotation is <c>null</c>.
    /// </summary>
    /// <param name="name"> the name of the annotation attribute. </param>
    /// <returns> the new name of the annotation attribute. </returns>
    private string MapAnnotationAttributeName(string name)
    {
        if (ReferenceEquals(descriptor, null)) return name;
        return remapper.MapAnnotationAttributeName(descriptor, name);
    }
}