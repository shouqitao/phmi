﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils {
    [TestFixture]
    public class ReflectionHelperTests {
        private class GetValueMock {
            public object Field;

#pragma warning disable 0649

            private object PrivateField;

#pragma warning restore 0649

            public int StructField;
            public object Property { get; set; }

            public int StructProperty { get; set; }

            private object PrivateProperty { get; set; }

            public object Method() {
                return Field;
            }

            public int StructMethod() {
                return StructField;
            }

            private object PrivateMethod() {
                return PrivateField;
            }
        }

        private class DisplayNameDescenderAttribute : DisplayNameAttribute {
            public DisplayNameDescenderAttribute(string displayName) : base(displayName) { }
        }

        private class GetDisplayNameMock {
            [DisplayName("Property Display Name")]
            public string PropertyWithDisplayNameAttr { get; set; }

            public string PropertyWithoutDisplayName { get; set; }

            [DisplayNameDescender("Property Display Name of Descender attr")]
            public string PropertyWithDisplayNameDescenderAttr { get; set; }
        }

        [MetadataType(typeof(GetDisplayNameMock))]
        private class GetDisplayNameMockWithMetadataTypeAttr {
            public string PropertyWithDisplayNameAttr { get; set; }
        }

        [Test]
        public void GetDisplayNameReturnsDisplayName() {
            string result =
                ReflectionHelper.GetDisplayName(typeof(GetDisplayNameMock), "PropertyWithDisplayNameAttr");
            Assert.AreEqual("Property Display Name", result);
        }

        [Test]
        public void GetDisplayNameReturnsDisplayNameLinqExpressionVersion() {
            string result =
                ReflectionHelper.GetDisplayName<GetDisplayNameMock>(m => m.PropertyWithDisplayNameAttr);
            Assert.AreEqual("Property Display Name", result);
        }

        [Test]
        public void GetDisplayNameReturnsDisplayNameLinqExpressionVersionWithObject() {
            var obj = new GetDisplayNameMock();
            string result = ReflectionHelper.GetDisplayName(obj, m => m.PropertyWithDisplayNameAttr);
            Assert.AreEqual("Property Display Name", result);
        }

        [Test]
        public void GetDisplayNameReturnsDisplayNameObjectVersion() {
            var obj = new GetDisplayNameMock();
            string result = ReflectionHelper.GetDisplayName(obj, "PropertyWithDisplayNameAttr");
            Assert.AreEqual("Property Display Name", result);
        }

        [Test]
        public void GetDisplayNameReturnsDisplayNameOfDescenderAttr() {
            string result = ReflectionHelper.GetDisplayName(
                typeof(GetDisplayNameMock), "PropertyWithDisplayNameDescenderAttr");
            Assert.AreEqual("Property Display Name of Descender attr", result);
        }

        [Test]
        public void GetDisplayNameReturnsMetadataDisplayName() {
            string result = ReflectionHelper.GetDisplayName(typeof(GetDisplayNameMockWithMetadataTypeAttr),
                "PropertyWithDisplayNameAttr");
            Assert.AreEqual("Property Display Name", result);
        }

        [Test]
        public void GetDisplayNameReturnsPropertyNameIfPropertyIsAbsent() {
            string result = ReflectionHelper.GetDisplayName(typeof(GetDisplayNameMock), "UnexistentProperty");
            Assert.AreEqual("UnexistentProperty", result);
        }

        [Test]
        public void GetDisplayNameReturnsPropertyNameIfWithoutAttr() {
            string result =
                ReflectionHelper.GetDisplayName(typeof(GetDisplayNameMock), "PropertyWithoutDisplayName");
            Assert.AreEqual("PropertyWithoutDisplayName", result);
        }

        [Test]
        public void GetValueReturnsNullField() {
            var obj = new GetValueMock();
            object result = ReflectionHelper.GetValue(obj, "Field");
            Assert.IsNull(result);
        }

        [Test]
        public void GetValueReturnsNullMethod() {
            var obj = new GetValueMock();
            object result = ReflectionHelper.GetValue(obj, "Method");
            Assert.IsNull(result);
        }

        [Test]
        public void GetValueReturnsNullProperty() {
            var obj = new GetValueMock();
            object result = ReflectionHelper.GetValue(obj, "Property");
            Assert.IsNull(result);
        }

        [Test]
        public void GetValueReturnsValueOfField() {
            var value = new object();
            var obj = new GetValueMock {Field = value};
            object result = ReflectionHelper.GetValue(obj, "Field");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueReturnsValueOfMethod() {
            var value = new object();
            var obj = new GetValueMock {Field = value};
            object result = ReflectionHelper.GetValue(obj, "Method");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueReturnsValueOfProperty() {
            var value = new object();
            var obj = new GetValueMock {Property = value};
            object result = ReflectionHelper.GetValue(obj, "Property");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueReturnsValueOfStructField() {
            const int value = 99;
            var obj = new GetValueMock {StructField = value};
            object result = ReflectionHelper.GetValue(obj, "StructField");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueReturnsValueOfStructMethod() {
            const int value = 99;
            var obj = new GetValueMock {StructField = value};
            object result = ReflectionHelper.GetValue(obj, "StructMethod");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueReturnsValueOfStructProperty() {
            const int value = 99;
            var obj = new GetValueMock {StructProperty = value};
            object result = ReflectionHelper.GetValue(obj, "StructProperty");
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValueThrowsOnNonExistentMemberName() {
            var obj = new GetValueMock();
            Assert.Catch(
                typeof(MissingMemberException),
                () => ReflectionHelper.GetValue(obj, "NonExistentProperty"));
        }

        [Test]
        public void GetValueThrowsOnPrivateField() {
            var obj = new GetValueMock();
            Assert.Catch(
                typeof(MissingMemberException),
                () => ReflectionHelper.GetValue(obj, "PrivateField"));
        }

        [Test]
        public void GetValueThrowsOnPrivateProperty() {
            var obj = new GetValueMock();
            Assert.Catch(
                typeof(MissingMemberException),
                () => ReflectionHelper.GetValue(obj, "PrivateProperty"));
        }
    }
}