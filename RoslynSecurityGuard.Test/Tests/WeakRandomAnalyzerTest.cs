﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynSecurityGuard.Analyzers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestHelper;

namespace RoslynSecurityGuard.Tests
{
    [TestClass]
    public class WeakRandomAnalyzerTest : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            return new[] { new WeakRandomAnalyzer() };
        }

        [TestMethod]
        public async Task RandomFalsePositive()
        {
            var cSharpTest = @"
using System;
using System.Security.Cryptography;

class WeakRandom
{
    static String generateSecureToken()
    {

        RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        byte[] buffer = new byte[16];
        rnd.GetBytes(buffer);
        return BitConverter.ToString(buffer);
    }
}
";
            var visualBasicTest = @"
Imports System
Imports System.Security.Cryptography

Class WeakRandom
	Private Shared Function generateSecureToken() As String
		Dim rnd As RandomNumberGenerator = RandomNumberGenerator.Create()
		Dim buffer As Byte() = New Byte(15) {}
		rnd.GetBytes(buffer)
		Return BitConverter.ToString(buffer)
	End Function
End Class
";
            await VerifyCSharpDiagnostic(cSharpTest);
            await VerifyVisualBasicDiagnostic(visualBasicTest);
        }

        [TestMethod]
        public async Task RandomVulnerable1()
        {
            var cSharpTest = @"
using System;

class WeakRandom
{
    static string generateWeakToken()
    {
        Random rnd = new Random();
        return rnd.Next().ToString(); 
    }
}
";
            var visualBasicTest = @"
Imports System

Class WeakRandom
	Private Shared Function generateWeakToken() As String
		Dim rnd As New Random()
		Return rnd.Next().ToString()
	End Function
End Class
";
            var expected = new DiagnosticResult
            {
                Id = "SG0005",
                Severity = DiagnosticSeverity.Warning,
            };

            await VerifyCSharpDiagnostic(cSharpTest, expected.WithLocation(9, -1)); ;
            await VerifyVisualBasicDiagnostic(visualBasicTest, expected.WithLocation("Test0.vb", 7));
        }
    }
}
