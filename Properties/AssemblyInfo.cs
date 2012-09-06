// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Srs.WebPlatform.WebServices.Printer;
using Srs.WebPlatform.Common;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Srs.WebPlatform.WebServices.Printer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Service Repair Solutions, Inc.")]
[assembly: AssemblyProduct("Srs.WebPlatform.WebServices.Printer")]
[assembly: AssemblyCopyright("Copyright 2007 - 2012 Service Repair Solutions, Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0e99409a-eb83-479f-a432-1fd3fe76b89e")]

//Register weservice Printer 
[assembly: AssemblyServiceMarkUpV1(typeof(IPrintServiceV1), "91bee358-4de7-4993-a57b-dce3214e18b5")]
//Register webservice HeartBeat
[assembly: AssemblyServiceMarkUpV1(typeof(IHeartBeatMonitoringServiceV1), "2c229630-a199-4011-858c-ba83214339a5")]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// [assembly : AssemblyVersion ("2.1.0.70")]
[assembly : AssemblyVersion ("2.1.0.70")]
[assembly : AssemblyFileVersion ("2.1.0.70")]
