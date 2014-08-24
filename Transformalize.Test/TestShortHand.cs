﻿#region License

// /*
// Transformalize - Replicate, Transform, and Denormalize Your Data...
// Copyright (C) 2013 Dale Newman
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */

#endregion

using NUnit.Framework;
using Transformalize.Main.Transform;
using Transformalize.Operations;

namespace Transformalize.Test {
    [TestFixture]
    public class TestShortHand {

        [Test]
        public void Replace() {
            const string expression = "r(x,y";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("replace", result.Method);
            Assert.AreEqual("x", result.OldValue);
            Assert.AreEqual("y", result.NewValue);
        }

        [Test]
        public void Left() {
            const string expression = "l(3";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("left", result.Method);
            Assert.AreEqual(3, result.Length);
        }

        [Test]
        public void Right() {
            const string expression = "ri(2";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("right", result.Method);
            Assert.AreEqual(2, result.Length);
        }

        [Test]
        public void Append() {
            const string expression = "a(...";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("append", result.Method);
            Assert.AreEqual("...", result.Value);
        }

        [Test]
        public void If() {
            const string expression = "i(x,y,yes,no";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("if", result.Method);
            Assert.AreEqual("x", result.Left);
            Assert.AreEqual("Equal", result.Operator);
            Assert.AreEqual("y", result.Right);
            Assert.AreEqual("yes", result.Then);
            Assert.AreEqual("no", result.Else);
        }

        [Test]
        public void IfWithOperator() {
            const string expression = "i(x,NotEqual,y,yes,no";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("if", result.Method);
            Assert.AreEqual("x", result.Left);
            Assert.AreEqual("NotEqual", result.Operator);
            Assert.AreEqual("y", result.Right);
            Assert.AreEqual("yes", result.Then);
            Assert.AreEqual("no", result.Else);
        }

        [Test]
        public void IfWithEmpty() {
            const string expression = "i(x,,yes,no";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("if", result.Method);
            Assert.AreEqual("x", result.Left);
            Assert.AreEqual("Equal", result.Operator);
            Assert.AreEqual("", result.Right);
            Assert.AreEqual("yes", result.Then);
            Assert.AreEqual("no", result.Else);
        }

        [Test]
        public void IfWithoutElse() {
            const string expression = "i(x,y,yes";
            var result = ShortHandFactory.Interpret(expression);
            Assert.AreEqual("if", result.Method);
            Assert.AreEqual("x", result.Left);
            Assert.AreEqual("Equal", result.Operator);
            Assert.AreEqual("y", result.Right);
            Assert.AreEqual("yes", result.Then);
            Assert.AreEqual("", result.Else);
        }

    }
}