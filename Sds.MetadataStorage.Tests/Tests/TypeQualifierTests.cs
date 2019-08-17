using FluentAssertions;
using Sds.MetadataStorage.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Sds.MetadataStorage.Tests
{
    public class TypeQualifierTests
    {
        [Fact]
        public void BooleanTests()
        {
            string[] booleans = new[] { "true", "false", "1", "0", "y", "n", "yes", "no" };
            var strings = new List<string>();
            var rnd = new Random();
            for (int i = 0; i < 1_000_000; i++)
                strings.Add(booleans[rnd.Next(7)].ToString());

            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("boolean");
            tq.MaxValue.Should().BeNull();
            tq.MinValue.Should().BeNull();
        }

        [Fact]
        public void IntegerTests()
        {
            var strings = new List<string>();
            var ints = new List<int>();
            strings.Add(null);
            var rnd = new Random();
            for (int i = 0; i < 1_000_000; i++)
            {
                var n = rnd.Next(799999);
                strings.Add(n.ToString());
                ints.Add(n);
            }
            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("integer");
            tq.MaxValue.Should().Be(ints.Max());
            tq.MinValue.Should().Be(ints.Min());
        }

        [Fact]
        public void DecimalTests()
        {
            var strings = new List<string>();
            var decimals = new List<decimal>();

            var rnd = new Random();
            for (int i = 0; i < 1_000_000; i++)
            {
                var n = ((decimal)rnd.Next(799999))/3;
                strings.Add(n.ToString(NumberFormatInfo.InvariantInfo));
                decimals.Add(n);
            }
            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("decimal");
            tq.MaxValue.Should().Be(decimals.Max());
            tq.MinValue.Should().Be(decimals.Min());
        }

        [Fact]
        public void BooleanDecimalTests()
        {
            var strings = new List<string>();
            var decimals = new List<decimal>();

            for (int i = 0; i < 1000; i++)
            {
                decimals.Add(0);
                strings.Add("0");
            }

            var rnd = new Random();
            for (int i = 0; i < 1_000; i++)
            {
                var n = ((decimal)rnd.Next(7999999)) / 3;
                strings.Add(n.ToString(NumberFormatInfo.InvariantInfo));
                decimals.Add(n);
            }
            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("decimal");
            tq.MaxValue.Should().Be(decimals.Max());
            tq.MinValue.Should().Be(decimals.Min());
        }

        [Fact]
        public void IntegerDecimalTests()
        {
            var strings = new List<string>();
            var decimals = new List<decimal>();

            var rnd = new Random();
            for (int i = 0; i < 3333; i++)
            {
                var k = rnd.Next(7999999);
                decimals.Add(k);
                strings.Add(k.ToString());
            }
            
            for (int i = 0; i < 3333; i++)
            {
                var n = ((decimal)rnd.Next(7999999)) / 3;
                strings.Add(n.ToString(NumberFormatInfo.InvariantInfo));
                decimals.Add(n);
            }
            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("decimal");
            tq.MaxValue.Should().Be(decimals.Max());
            tq.MinValue.Should().Be(decimals.Min());
        }

        [Fact]
        public void BooleanIntDecimalTests()
        {
            var strings = new List<string>();
            var decimals = new List<decimal>();

            for (int i = 0; i < 1_000; i++)
            {
                strings.Add("1");
                decimals.Add(1);
            }

            var rnd = new Random();
            for (int i = 0; i < 1_000; i++)
            {
                var n = ((decimal)rnd.Next(799999)) / 3;
                strings.Add(n.ToString(NumberFormatInfo.InvariantInfo));
                decimals.Add(n);
            }
            for (int i = 0; i < 1_000; i++)
            {
                var n = rnd.Next(799999);
                strings.Add(n.ToString());
                decimals.Add(n);
            }
            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("decimal");
            tq.MaxValue.Should().Be(decimals.Max());
            tq.MinValue.Should().Be(decimals.Min());
        }

        [Fact]
        public void BooleanIntDecimalStringTests()
        {
            var strings = new List<string>();

            for (int i = 0; i < 1_000; i++)
                strings.Add("1");

            var rnd = new Random();
            for (int i = 0; i < 1_000; i++)
            {
                var n = ((decimal)rnd.Next(799999)) / 3;
                strings.Add(n.ToString(NumberFormatInfo.InvariantInfo));
            }

            for (int i = 0; i < 1_000; i++)
            {
                var n = rnd.Next(799999);
                strings.Add(n.ToString());
            }

            strings.Add("zzzz");

            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("string");
            tq.MaxValue.Should().BeNull();
            tq.MinValue.Should().BeNull();
        }

        [Fact]
        public void StringTests()
        {
            var strings = new List<string>(new[] { "true", "1", "2", "3.5" });

            var tq = new TypeQualifier();
            foreach (var s in strings)
                tq.Qualify(s);

            tq.DataType.Should().Be("string");
            tq.MaxValue.Should().BeNull();
            tq.MinValue.Should().BeNull();
        }
    }
}
