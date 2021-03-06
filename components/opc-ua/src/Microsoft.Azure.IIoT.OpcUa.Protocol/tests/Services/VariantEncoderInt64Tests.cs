// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.OpcUa.Protocol.Services {
    using Opc.Ua;
    using Xunit;
    using Newtonsoft.Json.Linq;

    public class VariantEncoderInt64Tests {

        [Fact]
        public void DecodeEncodeInt64FromJValue() {
            var codec = new JsonVariantEncoder();
            var str = new JValue(-123L);
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(str, encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromJArray() {
            var codec = new JsonVariantEncoder();
            var str = new JArray(-123L, -124L, -125L);
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(str, encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromJValueTypeNullIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = new JValue(-123L);
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromJArrayTypeNullIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = new JArray(-123L, -124L, -125L);
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(str, encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromString() {
            var codec = new JsonVariantEncoder();
            var str = "-123";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromString() {
            var codec = new JsonVariantEncoder();
            var str = "-123, -124, -125";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromString2() {
            var codec = new JsonVariantEncoder();
            var str = "[-123, -124, -125]";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromString3() {
            var codec = new JsonVariantEncoder();
            var str = "[]";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[0]);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromStringTypeIntegerIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = "-123";
            var variant = codec.Decode(str, BuiltInType.Integer, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeIntegerIsInt641() {
            var codec = new JsonVariantEncoder();
            var str = "[-123, -124, -125]";
            var variant = codec.Decode(str, BuiltInType.Integer, null);
            var expected = new Variant(new Variant[] {
                new Variant(-123L), new Variant(-124L), new Variant(-125L)
            });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeIntegerIsInt642() {
            var codec = new JsonVariantEncoder();
            var str = "[]";
            var variant = codec.Decode(str, BuiltInType.Integer, null);
            var expected = new Variant(new Variant[0]);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromStringTypeNumberIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = "-123";
            var variant = codec.Decode(str, BuiltInType.Number, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeNumberIsInt641() {
            var codec = new JsonVariantEncoder();
            var str = "[-123, -124, -125]";
            var variant = codec.Decode(str, BuiltInType.Number, null);
            var expected = new Variant(new Variant[] {
                new Variant(-123L), new Variant(-124L), new Variant(-125L)
            });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeNumberIsInt642() {
            var codec = new JsonVariantEncoder();
            var str = "[]";
            var variant = codec.Decode(str, BuiltInType.Number, null);
            var expected = new Variant(new Variant[0]);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromStringTypeNullIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = "-123";
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }
        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeNullIsInt64() {
            var codec = new JsonVariantEncoder();
            var str = "-123, -124, -125";
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeNullIsInt642() {
            var codec = new JsonVariantEncoder();
            var str = "[-123, -124, -125]";
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromStringTypeNullIsNull() {
            var codec = new JsonVariantEncoder();
            var str = "[]";
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = Variant.Null;
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
        }

        [Fact]
        public void DecodeEncodeInt64FromQuotedString() {
            var codec = new JsonVariantEncoder();
            var str = "\"-123\"";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromSinglyQuotedString() {
            var codec = new JsonVariantEncoder();
            var str = "  '-123'";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromQuotedString() {
            var codec = new JsonVariantEncoder();
            var str = "\"-123\",'-124',\"-125\"";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromQuotedString2() {
            var codec = new JsonVariantEncoder();
            var str = " [\"-123\",'-124',\"-125\"] ";
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonTokenTypeVariant() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = -123
            });
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonTokenTypeVariant1() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = new long[] { -123, -124, -125 }
            });
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonTokenTypeVariant2() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = new long[0]
            });
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[0]);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonStringTypeVariant() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = -123
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonStringTypeVariant() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = new long[] { -123, -124, -125 }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonTokenTypeNull() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = -123
            });
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonTokenTypeNull1() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                TYPE = "INT64",
                BODY = new long[] { -123, -124, -125 }
            });
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonTokenTypeNull2() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "Int64",
                Body = new long[0]
            });
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[0]);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonStringTypeNull() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                Type = "int64",
                Body = -123
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonStringTypeNull() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                type = "Int64",
                body = new long[] { -123, -124, -125 }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonTokenTypeNullMsftEncoding() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                DataType = "Int64",
                Value = -123
            });
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64FromVariantJsonStringTypeVariantMsftEncoding() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                DataType = "Int64",
                Value = -123
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(-123L);
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JValue(-123L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64ArrayFromVariantJsonTokenTypeVariantMsftEncoding() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                dataType = "Int64",
                value = new long[] { -123, -124, -125 }
            });
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[] { -123, -124, -125 });
            var encoded = codec.Encode(variant);
            Assert.Equal(expected, variant);
            Assert.Equal(new JArray(-123L, -124L, -125L), encoded);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromStringJsonTypeNull() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new long[,,] {
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromStringJsonTypeInt64() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new long[,,] {
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Int64, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromVariantJsonTypeVariant() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                type = "Int64",
                body = new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromVariantJsonTokenTypeVariantMsftEncoding() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                dataType = "Int64",
                value = new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Variant, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromVariantJsonTypeNull() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                type = "Int64",
                body = new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

        [Fact]
        public void DecodeEncodeInt64MatrixFromVariantJsonTokenTypeNullMsftEncoding() {
            var codec = new JsonVariantEncoder();
            var str = JToken.FromObject(new {
                dataType = "Int64",
                value = new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                }
            }).ToString();
            var variant = codec.Decode(str, BuiltInType.Null, null);
            var expected = new Variant(new long[,,] {
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } },
                    { { 123L, -124L, -125L }, { 123L, -124L, -125L }, { 123L, -124L, -125L } }
                });
            var encoded = codec.Encode(variant);
            Assert.True(expected.Value is Matrix);
            Assert.True(variant.Value is Matrix);
            Assert.Equal(((Matrix)expected.Value).Elements, ((Matrix)variant.Value).Elements);
            Assert.Equal(((Matrix)expected.Value).Dimensions, ((Matrix)variant.Value).Dimensions);
        }

    }
}
