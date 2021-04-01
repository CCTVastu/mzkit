﻿Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#If netcore5 = 0 Then
<Assembly: AssemblyTitle("Targeted Metabolomics")>
<Assembly: AssemblyDescription("Library of targeted metabolomics quantify analysis from mzkit")>
<Assembly: AssemblyCompany("BioNovoGene")>
<Assembly: AssemblyProduct("mzkit")>
<Assembly: AssemblyCopyright("Copyright © BioNovoGene 2020")>
<Assembly: AssemblyTrademark("BioDeep")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("10acebf4-95d8-4098-bbdd-48c5d8fafe36")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.10.*")>
<Assembly: AssemblyFileVersion("1.34.*")>
#end if