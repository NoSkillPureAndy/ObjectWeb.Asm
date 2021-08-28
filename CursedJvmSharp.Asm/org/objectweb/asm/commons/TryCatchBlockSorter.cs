﻿using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;
using MethodNode = org.objectweb.asm.tree.MethodNode;
using TryCatchBlockNode = org.objectweb.asm.tree.TryCatchBlockNode;

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

namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A <seealso cref="MethodVisitor"/> adapter to sort the exception handlers. The handlers are sorted in a
	/// method innermost-to-outermost. This allows the programmer to add handlers without worrying about
	/// ordering them correctly with respect to existing, in-code handlers.
	/// 
	/// <para>Behavior is only defined for properly-nested handlers. If any "try" blocks overlap (something
	/// that isn't possible in Java code) then this may not do what you want. In fact, this adapter just
	/// sorts by the length of the "try" block, taking advantage of the fact that a given try block must
	/// be larger than any block it contains).
	/// 
	/// @author Adrian Sampson
	/// </para>
	/// </summary>
	public class TryCatchBlockSorter : MethodNode
	{

	  /// <summary>
	  /// Constructs a new <seealso cref="TryCatchBlockSorter"/>.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to which this visitor must delegate method calls. May
	  ///     be {@literal null}. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). This parameter also indicates if
	  ///     the method is synthetic and/or deprecated. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="org.objectweb.asm.Type"/>). </param>
	  /// <param name="signature"> the method's signature. May be {@literal null} if the method parameters,
	  ///     return type and exceptions do not use generic types. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes (see {@link
	  ///     org.objectweb.asm.Type#getInternalName()}). May be {@literal null}. </param>
	  public TryCatchBlockSorter(MethodVisitor methodVisitor, int access, string name, string descriptor, string signature, string[] exceptions) : this(Opcodes.ASM9, methodVisitor, access, name, descriptor, signature, exceptions)
	  {
		if (this.GetType() != typeof(TryCatchBlockSorter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  public TryCatchBlockSorter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor, string signature, string[] exceptions) : base(api, access, name, descriptor, signature, exceptions)
	  {
		this.mv = methodVisitor;
	  }

	  public override void visitEnd()
	  {
		// Sort the TryCatchBlockNode elements by the length of their "try" block.
		tryCatchBlocks.Sort(new ComparatorAnonymousInnerClass(this));
		// Update the 'target' of each try catch block annotation.
		for (int i = 0; i < tryCatchBlocks.Count; ++i)
		{
		  tryCatchBlocks[i].updateIndex(i);
		}
		if (mv != null)
		{
		  accept(mv);
		}
	  }

	  private class ComparatorAnonymousInnerClass : IComparer<TryCatchBlockNode>
	  {
		  private readonly TryCatchBlockSorter outerInstance;

		  public ComparatorAnonymousInnerClass(TryCatchBlockSorter outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }


		  public int Compare(TryCatchBlockNode tryCatchBlockNode1, TryCatchBlockNode tryCatchBlockNode2)
		  {
			return blockLength(tryCatchBlockNode1) - blockLength(tryCatchBlockNode2);
		  }

		  private int blockLength(TryCatchBlockNode tryCatchBlockNode)
		  {
			int startIndex = outerInstance.instructions.indexOf(tryCatchBlockNode.start);
			int endIndex = outerInstance.instructions.indexOf(tryCatchBlockNode.end);
			return endIndex - startIndex;
		  }
	  }
	}

}