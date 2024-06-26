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
/// A node that represents a required module with its name and access of a module descriptor.
/// 
/// @author Remi Forax
/// </summary>
public class ModuleRequireNode
{
    /// <summary>
    /// The fully qualified name (using dots) of the dependence. </summary>
    public string Module { get; set; }

    /// <summary>
    /// The access flag of the dependence among <c>ACC_TRANSITIVE</c>, <c>ACC_STATIC_PHASE</c>,
    /// <c>ACC_SYNTHETIC</c> and <c>ACC_MANDATED</c>.
    /// </summary>
    public int Access { get; set; }

    /// <summary>
    /// The module version at compile time, or <c>null</c>. </summary>
    public string Version { get; set; }

    /// <summary>
    /// Constructs a new <seealso cref = "ModuleRequireNode"/>.
    /// </summary>
    /// <param name = "module"> the fully qualified name (using dots) of the dependence. </param>
    /// <param name = "access"> the access flag of the dependence among <c>ACC_TRANSITIVE</c>, <c>ACC_STATIC_PHASE</c>, <c>ACC_SYNTHETIC</c> and <c>ACC_MANDATED</c>. </param>
    /// <param name = "version"> the module version at compile time, or <c>null</c>. </param>
    public ModuleRequireNode(string module, int access, string version)
    {
        this.Module = module;
        this.Access = access;
        this.Version = version;
    }

    /// <summary>
    /// Makes the given module visitor visit this require directive.
    /// </summary>
    /// <param name = "moduleVisitor"> a module visitor. </param>
    public virtual void Accept(ModuleVisitor moduleVisitor)
    {
        moduleVisitor.VisitRequire(Module, Access, Version);
    }
}