using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using AssertExpectedActualAnalyser;

namespace AssertExpectedActualAnalyser.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyCodeDoesNotTriggerDiagnostic()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void AssertEquals_IfActualValueIsAStringLiteral_ThenItTriggersTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(result.Id, ""someId"");
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.Equal': '\"someId\"' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void AssertEquals_IfExpectedValueIsAStringLiteral_ThenItDoesNotTriggerTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(""someId"", result.Id);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void AssertEquals_IfActualValueIsAnIntegerLiteral_ThenItTriggersTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(result.Size, 42);
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.Equal': '42' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void AssertEquals_IfExpectedValueIsAnIntegerLiteral_ThenItDoesNotTriggerTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(42, result.Size);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void AssertEquals_IfActualValueIsABooleanLiteral_ThenItTriggersTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(result.IsReticulated, true);
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.Equal': 'true' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void AssertEquals_IfExpectedValueIsABooleanLiteral_ThenItDoesNotTriggerTheDiagnostic()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(true, result.IsReticulated);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DiagnosticAnalyserRecognises_NUnitAndMSTest_AssertAreEqual()
        {
            var test = @"
    using System;
    using System.Linq;
    using NUnit.Framework;

    namespace WongaTests
    {
        class WongaTest
        {
            [Test]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.AreEqual(result.Id, ""someId"");
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.AreEqual': '\"someId\"' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DiagnosticAnalyserRecognises_Xunit_AssertNotEqual()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.NotEqual(result.Id, ""someId"");
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.NotEqual': '\"someId\"' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DiagnosticAnalyserRecognises_NUnitAndMSTest_AssertAreNotEqual()
        {
            var test = @"
    using System;
    using System.Linq;
    using NUnit.Framework;

    namespace WongaTests
    {
        class WongaTest
        {
            [Test]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.AreNotEqual(result.Id, ""someId"");
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "AssertExpectedActualAnalyser",
                Message = "Possible incorrect argument order in 'Assert.AreNotEqual': '\"someId\"' is likely to be the expected value",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void FixReversesTheOrderOfArguments()
        {
            var test = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(result.Id, ""someId"");
            }
        }
    }";

            var fix = @"
    using System;
    using System.Linq;
    using Xunit;

    namespace WongaTests
    {
        class WongaTest
        {
            [Fact]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.Equal(""someId"", result.Id);
            }
        }
    }";

            VerifyCSharpFix(test, fix);
        }

        [TestMethod]
        public void FixRespectsTrailingArguments()
        {
            var test = @"
    using System;
    using System.Linq;
    using NUnit.Framework;

    namespace WongaTests
    {
        class WongaTest
        {
            [Test]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.AreEqual(result.Id, ""someId"", ""oh noes!"");
            }
        }
    }";

            var fix = @"
    using System;
    using System.Linq;
    using NUnit.Framework;

    namespace WongaTests
    {
        class WongaTest
        {
            [Test]
            public void MyTest()
            {
                var result = Testee.GetSomeData();
                Assert.AreEqual(""someId"", result.Id, ""oh noes!"");
            }
        }
    }";

            VerifyCSharpFix(test, fix);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AssertExpectedActualAnalyserCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AssertExpectedActualAnalyserAnalyzer();
        }
    }
}