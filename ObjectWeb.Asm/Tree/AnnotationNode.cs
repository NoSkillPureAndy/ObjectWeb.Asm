using System;
using System.Collections;
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
/// A node that represents an annotation.
/// 
/// @author Eric Bruneton
/// </summary>
public class AnnotationNode : AnnotationVisitor
{
    /// <summary>
    /// The class descriptor of the annotation class. </summary>
    public string Desc { get; set; }

    /// <summary>
    /// The name value pairs of this annotation. Each name value pair is stored as two consecutive
    /// elements in the list. The name is a <seealso cref = "string "/>, and the value may be a <seealso cref = "byte "/>, <see cref="Boolean"/>, <seealso cref = "Character"/>, <seealso cref = "Short"/>, <seealso cref = "Integer"/>, <seealso cref = "Long"/>, <seealso cref = "Float"/>,
    /// <seealso cref = "double "/>, <seealso cref = "string "/> or <seealso cref = "Type"/>, or a two elements String
    /// array (for enumeration values), an <seealso cref = "AnnotationNode"/>, or a <seealso cref = "System.Collections.IList"/> of values of one
    /// of the preceding types. The list may be null if there is no name value pair.
    /// </summary>
    public List<object> Values { get; set; }

    /// <summary>
    /// Constructs a new <seealso cref = "AnnotationNode"/>. <i>Subclasses must not use this constructor</i>.
    /// Instead, they must use the <seealso cref = "AnnotationNode(int, String)"/> version.
    /// </summary>
    /// <param name = "descriptor"> the class descriptor of the annotation class. </param>
    /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
    public AnnotationNode(string descriptor) : this(Opcodes.Asm9, descriptor)
    {
        if (this.GetType() != typeof(AnnotationNode))
        {
            throw new System.InvalidOperationException();
        }
    }

    /// <summary>
    /// Constructs a new <seealso cref = "AnnotationNode"/>.
    /// </summary>
    /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of the <c>ASM</c><i>x</i> values in <seealso cref = "Opcodes"/>. </param>
    /// <param name = "descriptor"> the class descriptor of the annotation class. </param>
    public AnnotationNode(int api, string descriptor) : base(api)
    {
        this.Desc = descriptor;
    }

    /// <summary>
    /// Constructs a new <seealso cref = "AnnotationNode"/> to visit an array value.
    /// </summary>
    /// <param name = "values"> where the visited values must be stored. </param>
    public AnnotationNode(List<object> values) : base(Opcodes.Asm9)
    {
        this.Values = values;
    }

    // ------------------------------------------------------------------------
    // Implementation of the AnnotationVisitor abstract class
    // ------------------------------------------------------------------------
    public override void Visit(string name, object value)
    {
        if (Values == null)
        {
            Values = new List<object>(!string.ReferenceEquals(this.Desc, null) ? 2 : 1);
        }

        if (!string.ReferenceEquals(this.Desc, null))
        {
            Values.Add(name);
        }

        if (value is sbyte[] || value is byte[])
        {
            Values.Add(Util.AsArrayList((sbyte[])value));
        }
        else if (value is bool[])
        {
            Values.Add(Util.AsArrayList((bool[])value));
        }
        else if (value is short[])
        {
            Values.Add(Util.AsArrayList((short[])value));
        }
        else if (value is char[])
        {
            Values.Add(Util.AsArrayList((char[])value));
        }
        else if (value is int[])
        {
            Values.Add(Util.AsArrayList((int[])value));
        }
        else if (value is long[])
        {
            Values.Add(Util.AsArrayList((long[])value));
        }
        else if (value is float[])
        {
            Values.Add(Util.AsArrayList((float[])value));
        }
        else if (value is double[])
        {
            Values.Add(Util.AsArrayList((double[])value));
        }
        else
        {
            Values.Add(value);
        }
    }

    public override void VisitEnum(string name, string descriptor, string value)
    {
        if (Values == null)
        {
            Values = new List<object>(!string.ReferenceEquals(this.Desc, null) ? 2 : 1);
        }

        if (!string.ReferenceEquals(this.Desc, null))
        {
            Values.Add(name);
        }

        Values.Add(new string[] { descriptor, value });
    }

    public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
    {
        if (Values == null)
        {
            Values = new List<object>(!string.ReferenceEquals(this.Desc, null) ? 2 : 1);
        }

        if (!string.ReferenceEquals(this.Desc, null))
        {
            Values.Add(name);
        }

        AnnotationNode annotation = new AnnotationNode(descriptor);
        Values.Add(annotation);
        return annotation;
    }

    public override AnnotationVisitor VisitArray(string name)
    {
        if (Values == null)
        {
            Values = new List<object>(!string.ReferenceEquals(this.Desc, null) ? 2 : 1);
        }

        if (!string.ReferenceEquals(this.Desc, null))
        {
            Values.Add(name);
        }

        List<object> array = new List<object>();
        Values.Add(array);
        return new AnnotationNode(array);
    }

    public override void VisitEnd()
    {
        // Nothing to do.
    }

    // ------------------------------------------------------------------------
    // Accept methods
    // ------------------------------------------------------------------------
    /// <summary>
    /// Checks that this annotation node is compatible with the given ASM API version. This method
    /// checks that this node, and all its children recursively, do not contain elements that were
    /// introduced in more recent versions of the ASM API than the given version.
    /// </summary>
    /// <param name = "api"> an ASM API version. Must be one of the <c>ASM</c><i>x</i> values in <see cref="Opcodes"/>. </param>
    public virtual void Check(int api)
    {
        // nothing to do
    }

    /// <summary>
    /// Makes the given visitor visit this annotation.
    /// </summary>
    /// <param name = "annotationVisitor"> an annotation visitor. Maybe null. </param>
    public virtual void Accept(AnnotationVisitor annotationVisitor)
    {
        if (annotationVisitor != null)
        {
            if (Values != null)
            {
                for (int i = 0, n = Values.Count; i < n; i += 2)
                {
                    string name = (string)Values[i];
                    object value = Values[i + 1];
                    Accept(annotationVisitor, name, value);
                }
            }

            annotationVisitor.VisitEnd();
        }
    }

    /// <summary>
    /// Makes the given visitor visit a given annotation value.
    /// </summary>
    /// <param name = "annotationVisitor"> an annotation visitor. Maybe <c>null</c>. </param>
    /// <param name = "name"> the value name. </param>
    /// <param name = "value"> the actual value. </param>
    internal static void Accept(AnnotationVisitor annotationVisitor, string name, object value)
    {
        if (annotationVisitor != null)
        {
            if (value is string[])
            {
                string[] typeValue = (string[])value;
                annotationVisitor.VisitEnum(name, typeValue[0], typeValue[1]);
            }
            else if (value is AnnotationNode)
            {
                AnnotationNode annotationValue = (AnnotationNode)value;
                annotationValue.Accept(annotationVisitor.VisitAnnotation(name, annotationValue.Desc));
            }
            else if (value is System.Collections.IList)
            {
                AnnotationVisitor arrayAnnotationVisitor = annotationVisitor.VisitArray(name);
                if (arrayAnnotationVisitor != null)
                {
                    IList arrayValue = (IList)value;
                    for (int i = 0, n = arrayValue.Count; i < n; ++i)
                    {
                        Accept(arrayAnnotationVisitor, null, arrayValue[i]);
                    }

                    arrayAnnotationVisitor.VisitEnd();
                }
            }
            else
            {
                annotationVisitor.Visit(name, value);
            }
        }
    }
}