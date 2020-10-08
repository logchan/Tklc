using System;
using NUnit.Framework;
using Tklc.IO;

namespace Tklc.Tests.IO {
    public class FileNameNaturalCompareTests {
        [Test]
        public void Basics() {
            var names = new[] {
                "1.jpg", "2.jpg", "22.jpg", "3.jpg", "10.jpg", "11.jpg", "15.jpg", "9.jpg"
            };
            var sorted = new[] {
                "1.jpg", "2.jpg", "3.jpg", "9.jpg", "10.jpg", "11.jpg", "15.jpg", "22.jpg"
            };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }

        [Test]
        public void DifferentPrefix() {
            var names = new[] { "A1.jpg", "B3.jpg", "A5.jpg", "B11.jpg" };
            var sorted = new[] { "A1.jpg", "A5.jpg", "B3.jpg", "B11.jpg" };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }

        [Test]
        public void Infix() {
            var names = new[] { "A42D7.jpg", "A42D70.jpg", "A42D65.jpg", "A42D13.jpg" };
            var sorted = new[] { "A42D7.jpg", "A42D13.jpg", "A42D65.jpg", "A42D70.jpg" };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }

        [Test]
        public void DifferentInfix() {
            var names = new[] { "A42D7.jpg", "A33D70.jpg", "A42D65.jpg", "A33D13.jpg" };
            var sorted = new[] { "A33D13.jpg", "A33D70.jpg", "A42D7.jpg", "A42D65.jpg" };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }

        [Test]
        public void LargeNumbers() {
            var names = new[] { "123456789012345678901234567890.png", "98765432109876543210987654321.png" };
            var sorted = new[] { "98765432109876543210987654321.png", "123456789012345678901234567890.png" };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }

        [Test]
        public void LeadingZeros() {
            var names = new[] {
                "0001.jpg", "02.jpg", "022.jpg", "3.jpg", "0010.jpg", "11.jpg", "015.jpg", "9.jpg"
            };
            var sorted = new[] {
                "0001.jpg", "02.jpg", "3.jpg", "9.jpg", "0010.jpg", "11.jpg", "015.jpg", "022.jpg"
            };
            Array.Sort(names, IOHelpers.FileNameNaturalCompare);
            Assert.AreEqual(sorted, names);
        }
    }
}
