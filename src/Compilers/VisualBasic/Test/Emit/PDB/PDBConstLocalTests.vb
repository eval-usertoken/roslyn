﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Roslyn.Test.Utilities
Imports System.Xml.Linq
Imports Microsoft.CodeAnalysis.Test.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests.PDB

    Public Class PDBConstLocalTests
        Inherits BasicTestBase

        <Fact()>
        Public Sub TestSimpleLocalConstants()
            Dim source =
<compilation>
    <file>
Imports System                                 
Public Class C
    Public Sub M()
        const x as integer = 1
        const y as integer = 2
        Console.WriteLine(x + y)
    end sub
end class
    </file>
</compilation>
            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
                source,
                TestOptions.DebugDll)

            compilation.VerifyDiagnostics()

            compilation.VerifyPdb("C.M",
<symbols>
    <methods>
        <method containingType="C" name="M">
            <sequencePoints>
                <entry offset="0x0" startLine="3" startColumn="5" endLine="3" endColumn="19" document="0"/>
                <entry offset="0x1" startLine="6" startColumn="9" endLine="6" endColumn="33" document="0"/>
                <entry offset="0x8" startLine="7" startColumn="5" endLine="7" endColumn="12" document="0"/>
            </sequencePoints>
            <locals>
                <constant name="x" value="1" type="Int32"/>
                <constant name="y" value="2" type="Int32"/>
            </locals>
            <scope startOffset="0x0" endOffset="0x9">
                <namespace name="System" importlevel="file"/>
                <currentnamespace name=""/>
                <constant name="x" value="1" type="Int32"/>
                <constant name="y" value="2" type="Int32"/>
            </scope>
        </method>
    </methods>
</symbols>)

        End Sub

        <Fact()>
        Public Sub TestLambdaLocalConstants()
            Dim source =
<compilation>
    <file>
Imports System                                 
Public Class C
    Public Sub M(a as action)
        const x as integer = 1
        M(
            Sub()
                const y as integer = 2
                const z as integer = 3
                Console.WriteLine(x + y + z)
            end Sub
         )
    end sub
end class
    </file>
</compilation>

            Dim c = CompileAndVerify(source, options:=TestOptions.DebugDll)

            c.VerifyPdb(
<symbols>
    <methods>
        <method containingType="C" name="M" parameterNames="a">
            <customDebugInfo>
                <encLambdaMap>
                    <methodOrdinal>1</methodOrdinal>
                    <lambda offset="48"/>
                </encLambdaMap>
            </customDebugInfo>
            <sequencePoints>
                <entry offset="0x0" startLine="3" startColumn="5" endLine="3" endColumn="30" document="0"/>
                <entry offset="0x1" startLine="5" startColumn="9" endLine="11" endColumn="11" document="0"/>
                <entry offset="0x2c" startLine="12" startColumn="5" endLine="12" endColumn="12" document="0"/>
            </sequencePoints>
            <locals>
                <constant name="x" value="1" type="Int32"/>
            </locals>
            <scope startOffset="0x0" endOffset="0x2d">
                <namespace name="System" importlevel="file"/>
                <currentnamespace name=""/>
                <constant name="x" value="1" type="Int32"/>
            </scope>
        </method>
        <method containingType="C+_Closure$__" name="_Lambda$__1-0">
            <sequencePoints>
                <entry offset="0x0" startLine="6" startColumn="13" endLine="6" endColumn="18" document="0"/>
                <entry offset="0x1" startLine="9" startColumn="17" endLine="9" endColumn="45" document="0"/>
                <entry offset="0x8" startLine="10" startColumn="13" endLine="10" endColumn="20" document="0"/>
            </sequencePoints>
            <locals>
                <constant name="y" value="2" type="Int32"/>
                <constant name="z" value="3" type="Int32"/>
            </locals>
            <scope startOffset="0x0" endOffset="0x9">
                <importsforward declaringType="C" methodName="M" parameterNames="a"/>
                <constant name="y" value="2" type="Int32"/>
                <constant name="z" value="3" type="Int32"/>
            </scope>
        </method>
    </methods>
</symbols>)
        End Sub

#If False Then
        <WorkItem(11017)>
        <Fact()>
        Public Sub TestIteratorLocalConstants()
            Dim text = <text>
using System.Collections.Generic;

class C
{
    IEnumerable&lt;int&gt; M()
    {
        const int x = 1;
        for (int i = 0; i &lt; 10; i++)
        {
            const int y = 2;
            yield return x + y + i;
        }
    }
}
</text>.Value

            AssertXmlEqual(expected, actual)
        End Sub
#End If

        <WorkItem(529101, "DevDiv")>
        <Fact()>
        Public Sub TestLocalConstantsTypes()
            Dim source = <compilation>
                             <file>
Imports System                                    
Public Class C
    Sub M()
        const o as object = nothing
        const s as string  = "hello"
        const f as single = single.MinValue
        const d as double = double.MaxValue
        const dec as decimal = 1.5D
        const dt as datetime = #2/29/2012#
    End Sub
End Class
</file>
                         </compilation>
            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
                source,
                TestOptions.DebugDll)

            compilation.VerifyDiagnostics(
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "o").WithArguments("o"),
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "s").WithArguments("s"),
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "f").WithArguments("f"),
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "d").WithArguments("d"),
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "dec").WithArguments("dec"),
                 Diagnostic(ERRID.WRN_UnusedLocalConst, "dt").WithArguments("dt"))

            Dim actual As XElement = GetPdbXml(compilation, "C.M")
            Dim invariantStr = actual.ToString()
            invariantStr = invariantStr.Replace("-3,402823E+38", "-3.402823E+38")
            invariantStr = invariantStr.Replace("1,79769313486232E+308", "1.79769313486232E+308")
            invariantStr = invariantStr.Replace("2,98187743664301E-266", "2.98187743664301E-266")
            invariantStr = invariantStr.Replace("value=""1,5""", "value=""1.5""")
            Dim expected = <symbols>
                               <methods>
                                   <method containingType="C" name="M">
                                       <sequencePoints>
                                           <entry offset="0x0" startLine="3" startColumn="5" endLine="3" endColumn="12" document="0"/>
                                           <entry offset="0x1" startLine="10" startColumn="5" endLine="10" endColumn="12" document="0"/>
                                       </sequencePoints>
                                       <locals>
                                           <constant name="o" value="0" type="Int32"/>
                                           <constant name="s" value="hello" type="String"/>
                                           <constant name="f" value="-3.402823E+38" type="Single"/>
                                           <constant name="d" value="1.79769313486232E+308" type="Double"/>
                                           <constant name="dec" value="1.5" type="Decimal"/>
                                           <constant name="dt" value="2.98187743664301E-266" type="Double"/>
                                       </locals>
                                       <scope startOffset="0x0" endOffset="0x2">
                                           <namespace name="System" importlevel="file"/>
                                           <currentnamespace name=""/>
                                           <constant name="o" value="0" type="Int32"/>
                                           <constant name="s" value="hello" type="String"/>
                                           <constant name="f" value="-3.402823E+38" type="Single"/>
                                           <constant name="d" value="1.79769313486232E+308" type="Double"/>
                                           <constant name="dec" value="1.5" type="Decimal"/>
                                           <constant name="dt" value="2.98187743664301E-266" type="Double"/>
                                       </scope>
                                   </method>
                               </methods>
                           </symbols>

            'AssertXmlEqual(expected, actual)
            Assert.Equal(expected.ToString(), invariantStr)
        End Sub

    End Class
End Namespace


