# Yuzu reference

  * [Serializers](#serializers)
  * [Deserializers](#deserializers)
  * [Item attributes](#item-attributes)
  * [Method attributes](#method-attributes)
  * [Class attributes](#class-attributes)
  * [Options](#options)
  * [JsonOptions](#json-options)
  * [BinarySerializeOptions](#binaryserializeoptions)
  * [Meta overrides](#meta-overrides)
  * [Cloning](#cloning)
  * [Type serialization](#type-serialization)
  * [Packing and unpacking to dictionary](#packing-and-unpacking-to-dictionary)

## Serializers

### `AbstractSerializer`

Common base class for all serializers.

#### `ToWriter(object, BinaryWriter)`
Serialize to a given `BinaryWriter`. This is the most efficiant option as it avoids extra memory allocation.

#### `ToString(object)`
Serialize to a string and return it.

#### `ToBytes(object)`
Serialize to a byte array and return it.

#### `ToBytes(object)`
Serialize to a given `Stream`.

### `JsonSerializer`
Serializes into a JSON format with some extensions.
```cs
vas s = (new JsonSerializer()).ToString(new MyClass { MyField = 5 });
```

### `BinarySerializer`
Serializes into a Yuzu-specific binary format.
```cs
var b = (new BinarySerializer()).ToBytes(new MyClass { MyField = 5 });
```

## Deserializers

### `AbstractDeserializer`

Common base class for all deserializers.

#### `FromReader(object, BinaryReader)`
Deserialize from a given `BinaryReader` into a given object.

#### `FromString(object, string)`
Deserialize from a given string into a given object.

#### `FromStream(object, Stream)`
Deserialize from a given `Stream` into a given object.

#### `FromBytes(object, byte[])`
Deserialize from a given byte array into a given object.

#### `FromReader(BinaryReader)` or `FromReader<T>(BinaryReader)`.
Deserialize from a given `BinaryReader` and return the result.
Generic version generates exception if deserialized object type is not compatible with `T`.

#### `FromString(string)` or `FromString<T>(string)`
Deserialize from a given string and return the result.
Generic version generates exception if deserialized object type is not compatible with `T`.

#### `FromStream(Stream)` or `FromStream<T>(Stream)`
Deserialize from a given `Stream` and return the result.
Generic version generates exception if deserialized object type is not compatible with `T`.

#### `FromBytes(byte[])` or `FromBytes<T>(byte[])`
Deserialize from a given byte array and return the result.
Generic version generates exception if deserialized object type is not compatible with `T`.

### `JsonDeserializer`
Deserializes from a JSON format with some extensions.
```cs
var t = (new JsonDeserializer()).FromString<MyClass>("{ \"MyField\": 5 }");
```

### `BinaryDeserializer`
Deserializes from a Yuzu-specific binary format.
```cs
var t = (new BinaryDeserializer()).FromBytes<MyClass>(b);
```

## Item attributes

Item attributes can be applied to either a public field or a public readable property.
The item is considered for serialization and deserialization if it is annotated with exacty one of
(`[YuzuRequired]`, `[YuzuOptional]` and `[YuzuMember]`) attributes.

#### `[YuzuRequired]` or `[YuzuRequired("alias")]`
Denotes required item. This item is always serialized. An exception is thrown if the item is absent during deserialization.
If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuOptional]` and `[YuzuMember]`.

Can be substituted by changing `MetaOptions.RequiredAttribute` and/or `MetaOptions.GetAlias`.

#### `[YuzuOptional]` or `[YuzuOptional("alias")]`
Denotes optional item. This item may be omitted during serialization by using `YuzuSerializeIf` attribute.
If the item is absent during deserialization, its value in the target object is left unchanged.
If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuRequired]` and `[YuzuMember]`.

Can be substituted by changing `MetaOptions.OptionalAttribute` and/or `MetaOptions.GetAlias`.

#### `[YuzuMember]` or `[YuzuMember("alias")]`
Denotes optional item with the default falue.
Immediately before serialization of the scalar item, item value is compared with the default value of the item's type.
If the item is `ICollection`, it is checked for emptiness instead.
If the item is absent during deserialization, its value in the target object is left unchanged.
If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuRequired]` and `[YuzuOptional]`.

Can be substituted by changing `MetaOptions.MemberAttribute` and/or `MetaOptions.GetAlias`.

#### `[YuzuSerializeIf(nameof(conditionFunc))]`
#### `public bool conditionFunc() { ... }`
Denotes serialization condition. Can only be applied to `YuzuOptional` item.
The argument must be a name of boolean function without arguments, member of the current class.
Immediately before serialization of the item, this function is called.
If the function returns `true`, the item is serialized, otherwise the item is omitted.

Can be substituted by changing `MetaOptions.SerializeIfAttribute` and/or `MetaOptions.GetSerializeCondition`.

#### `[YuzuDefault(defValue)]`
Denotes default value for serialization. Can only be applied to `YuzuOptional` item.
Immediately before serialization of the item, item value is compared with `defValue`.
If they are equal, the item is omitted, otherwise the item is serialized.

#### `[YuzuMerge]`
Denotes that the deserialized value must be merged with the original item value instead of replacing it.
Can only be applied to items of structured types: `class`, `struct`, `interface` or `object`.
When deserializing an item of structured type without merging, a new object is constructed,
sub-item values are deserialized into this new object, and finally new object is assigned to the item of the containing object.
When deserializing an item of structured type with merging,
sub-item values are deserialized into the existing object. If some of the sub-items are omitted, previous values are retained.

Can be substituted by changing `MetaOptions.MergeAttribute`.

#### `[YuzuCompact]`
Selects more compact serialized representation at the expense of backward and forward compatibility.
This attrubute can be used for increasing serialization and deserialization speed as well as readablity of text-based formats.
The downside is that changing `YuzuCompact` item will break compatibility with both old and new versions of the serialized data.

Can be substituted by changing `MetaOptions.CompactAttribute`.

#### `[YuzuCopyable]`
Denotes that an item should be cloned by direct assignment, even if it is of a reference type or contains one.

Can be substituted by changing `MetaOptions.CopyableAttribute`.

#### `[YuzuExclude]`
Denotes that an item must not be serialized and deserialized despite the presence of `YuzuAll` attribute on the containing class.

Can be substituted by changing `MetaOptions.ExcludeAttribute`.

## Method attributes

#### `[YuzuBeforeSerialization]`
Denotes a `void` method without arguments, which will be called immediately before this object is serialized.
If there are several `[YuzuBeforeSerialization]` methods, first methods of the current class are called in the order of source code definition,
then methods from the parent class are called. This order is opposite of `[YuzuAfterDeserialization]`.

Can be substituted by changing `MetaOptions.BeforeSerializationAttribute`.

#### `[YuzuAfterSerialization]`
Denotes a `void` method without arguments, which will be called immediately after this object is serialized.
If there are several `[YuzuAfterSerialization]` methods, first methods of the current class are called in the order of source code definition,
then methods from the parent class are called. This order is opposite of `[YuzuBeforeDeserialization]`.

Can be substituted by changing `MetaOptions.AfterSerializationAttribute`.

#### `[YuzuBeforeDeserialization]`
Denotes a `void` method without arguments, which will be called immediately before this object is deserialized.
Usually this method is called immediately after construction of deserialized object, except when merging into existing object.
If there are several `[YuzuBeforeDeserialization]` methods, first methods from the parent class are called,
then methods of the current class are called in the order of source code definition. This order is opposite of `[YuzuAfterSerialization]`.

Can be substituted by changing `MetaOptions.YuzuBeforeDeserializationAttribute`.

#### `[YuzuAfterDeserialization]`
Denotes a `void` method without arguments, which will be called immediately after this object is deserialized.
If there are several `[YuzuAfterDeserialization]` methods, first methods from the parent class are called,
then methods of the current class are called in the order of source code definition. This order is opposite of `[YuzuBeforeSerialization]`.

Can be substituted by changing `MetaOptions.YuzuAfterDeserializationAttribute`.

#### `[YuzuSerializeItemIf]`
#### `public bool conditionFunc(int index, object item) { ... }`
Denotes serialization condition for collection items. Can only be applied to method of a class implementing `IEnumerable`.
Method must be a boolean function accepting integer and object arguments.
Immediately before serialization of each collection item, this function is called.
If the function returns `true`, the item is serialized, otherwise the item is omitted.

Can be substituted by changing `MetaOptions.SerializeItemIfAttribute` and/or `MetaOptions.GetSerializeItemCondition`.

#### `[YuzuFactory]`
Denotes a static method without arguments, which will be called to create an instance of this class during deserialization.
Only one factory per class is allowed.

Can be substituted by changing `MetaOptions.FactoryAttribute`.

## Class attributes

#### `[YuzuCompact]`
Selects more compact serialized representation at the expense of backward and forward compatibility.
This attrubute can be used for increasing serialization and deserialization speed as well as readablity of text-based formats.
The downside is that changing `YuzuCompact` class will break compatibility with both old and new versions of the serialized data.

Can be substituted by changing `MetaOptions.CompactAttribute`.

#### `[YuzuCopyable]`
Denotes that instances of this class should be cloned by direct assignment, even if they are or contain reference types.

Can be substituted by changing `MetaOptions.CopyableAttribute`.

#### `[YuzuMust]` or `[YuzuMust(itemKind)]`
Denotes that all items must be serialized. Exception is thrown if at least one public item lacks serialization attribute.
If present, `itemKind` argument limits the requirement to either just fields (`YuzuItemKind.Field`) or just properties (`YuzuItemKind.Property`).

Can be substituted by changing `MetaOptions.MustAttribute` and/or `MetaOptions.GetItemKind`.

#### `[YuzuAll]` or `[YuzuAll(optionality, itemKind)]`
Denotes that all public items are serialized by default, even if not annotated by serialization attribute.
Some items can be excluded by using `YuzuExclude` attrubute.
If present, `optionality` argument indicates the level of optionality
(`YuzuItemOptionality.Optional`, `YuzuItemOptionality.Required` or `YuzuItemOptionality.Member`) applied to the items by default.
Annotating an item with `[YuzuRequired]`, `[YuzuOptional]` or `[YuzuMember]` attribute overrides default given by `YuzuAll`.
If present, `itemKind` argument limits the default serialization to either fields (`YuzuItemKind.Field`) or properties (`YuzuItemKind.Property`).
Using both `YuzuAll` and `YuzuMust` simultaneously prohibits `YuzuExclude`.

Can be substituted by changing `MetaOptions.AllAttribute`, and/or `MetaOptions.GetItemOptionalityAndKind`.

#### `[YuzuAllowReadingFromAncestor]`
Normally, an item of structured type can be deserialized if the serialized item is of the same class or descendant class.
This attribute allows deserializing from ancestor class, as long as all fields not present in the ancestor are optional.
It is NOT recommended to use this attribute.

Can be substituted by changing `MetaOptions.AllowReadingFromAncestorAttribute`.

#### `[YuzuAlias("alias")]` or `[YuzuAlias(read: readAliasList, write: writeAlias)]`
Denotes that during serializarion, `writeAlias` is used instead of class name,
and during deserialization any of the given read aliases plus original class name can be used for this class.
All read aliases must be globally unique between all classes.
If the single-argument form is used, it defines both write alias and  a single read alias.

Can be substituted by changing `MetaOptions.AliasAttribute`, `MetaOptions.ReadAliases` and/or `MetaOptions.WriteAlias`.

## Options

Options common for all formats. Note that this is a struct, not a class, and has value semantics.

#### `Meta`

Contains attribute classes.
Change them to use different attributes, such as `Serializable` or `ProtoContract`.
Note that this field is a reference,
so it is usually better to create a new `MetaOptions` object instead of assigning directly to its fields.

#### `TagMode`

Experimental. Do not use.

#### `AllowUnknownFields`

Controls what happens when unknown field is encountered during deserialization.
If `true`, it is either ignored or stored in `YuzuUnknownStorage`.
Otherwise an exception is thrown. Default value is `false`.

#### `AllowEmptyTypes`

Controls what happens when a class without serializable fields is encountered during serialization or deserialization.
If `true`, it is ignored.
Otherwise an exception is thrown. Default value is `false`.

#### `ReportErrorPosition`

If `true`, a source stream position is included in error messages during deserialization.
Default value is `false`.

#### `CheckForEmptyCollections`

If `true`, a collection with `YuzuSerializeItemIf` attribute which returns `false` for all items will not be serialized.
This requires an extra pass through the collection.
Otherwise a collection will be serialized as empty.
Default value is `false`.

## JSON options

#### `FieldSeparator`

String inserted after every object field, list item, opening `[` and `{`.
Default value is `\n`.

#### `Indent`

String inserted several times in front of every object field and list item.
Number of insertions is equal to the nesting depth.
Default value is `\t`.

#### `ClassTag`

Name of the system field used to serialize object type. Should not be set to any acceptable C# identifier to avoid conflicts.
Default value is `class`.

#### `MaxOnelineFields`

If `[YuzuCompact]` type contains only primitive fields, and their number is no more than `MaxOnelineFields`,
`FieldSeparator` and `Indent` strings are not inserted around fields.
Default value is `0`.

#### `EnumAsString`

Controls the format of enumeration values.
If `true`, enumeration values are serialized as strings, allowing to add elements without breaking compatibility.
Otherwise enumeration values are serialized as integers, allowing to rename elements without breaking compatibility.
Default value is `false`.

#### `ArrayLengthPrefix`

Controls the format of arrays.
If `true`, array length is serialized before array items, allowing to preallocate memory during deserialization.
Default value is `false`.

#### `SaveRootClass`

If `true`, the name of the root structured type will be serialized even if it concides with generic argument type.
Default value is `false`.

#### `IgnoreCompact`

Globally controls `[YuzuCompact]` attribute. If `true`, `[YuzuCompact]` attribute has no effect.
Default value is `false`.

#### `DateFormat`, `DateOffsetFormat`

Controls the format of date and date offset values.
Note that not all formats guarantee roundrtip due to possible information loss and culture differences.
Default value is `"O"`, which does guarantee roundrtip.

#### `TimeSpanFormat`

Controls the format of timespan values. Note that not all formats guarantee roundrtip due to possible information loss and culture differences.
Default value is `"c"`, which does guarantee roundrtip.

#### `Int64AsString`

Controls the format of 64-bit integer (`long`) values.
If `true`, 64-bit integers are serialized as strings, which provides compatibility with JSON standard.
Default value is `false`.

#### `DecimalAsString`

Controls the format of `decimal` values.
If `true`, `decimal` values are serialized as strings, which provides compatibility with JSON standard.
Default value is `false`.

#### `Unordered`

Controls ordering of object fields.
If `false`, fields are serialized in alphabetical order of their names (or write aliases, if provided).
Same ordering is also required on deserialization.
If `true`, fields may be deserializad in any order, and serialization order is undefined.
Note that `Unordered` mode does not guarantee text-data-text roundtrip and is not supported by generated deserializers.
Default value is `false`.

#### `Comments`
If `true`, single-line comments starting with double slash (`//`) are accepted during deserialization.
Comments are ignored and not preserved on roundtrip.
Default value is `false`.

#### `BOM`
If `true`, UTF-8 byte order mark (bytes `EF BB BF`) is allowed before the first byte of input stream during deserialization.
Default value is `false`.

#### `FloatingPointFormat`
If empty, `float` and `double` values are serialized by internal code. Otherwise, `ToString(FloatingPointFormat)` is used.
Note that roundtrip is only guaranteed when using internal serializer.
Default value is empty string.

## `Binary options`

#### `Signature`

Byte prefix used to detect binary serialization format and version.
Default value is `YB01`.

#### `AutoSignature`

If `true`, `Signature` must be present in front of every serialized stream, or an error will be thrown.
Default value is `false`.

#### `Unordered`

If `true`, deserialization accepts fields in arbitrary order.
This mode is slow and not supported by generated deserializers. Use for compatibility only.
Default value is `false`.

## Meta overrides

Class `MetaOptions` provides an API for specifying Yuzu attributes at runtime.
Note that changing `MetaOptions` does not directly affect `Meta` cache,
so either change global options (as in example 1) before any call to `Meta.Get`,
or create a new `MetaOptions` instance for a localized change (as in example 2).

Example 1:
```cs
MetaOptions.Default.AddOverride(typeof(MyClass), o => o.AddAttr(new YuzuAll()));
```

Example 2:
```cs
var opt = new CommonOptions() { Meta = new MetaOptions().AddOverride(
    typeof(MyClass), o => o.
        AddAttr(new YuzuAlias("MyAlias")).
        AddItem(nameof(MyClass.MyField), i => i.
            AddAttr(new YuzuMember("FieldAlias")).
            AddAttr(new YuzuSerializeIf(nameof(MyClass.CondFunc)))
        ).
        AddItem(nameof(MyClass.MyMethod), i => i.AddAttr(new YuzuFactory()))
) };
```

### `AddOverride(Type t, Action after)`

Creates an override for type `t`. Upon success, executes `after` action with newly created override as an argument.
Returns `this` to allow chaining.

### `AddAttr(Attribute attr)`

Adds an attribute to either type or item override.
Returns `this` to allow chaining.

### `NegateAttr(Type attrType)`

Negates an existing attribute on either type or item.
Returns `this` to allow chaining.

### `AddItem(string itemName, Action after)`

Creates an override for item `itemName`. Upon success, executes `after` action with newly created override as an argument.
Returns `this` to allow chaining.

## Cloning

To clone an object, use `Yuzu.Clone.Cloner.Deep(object)`, for example:

```cs
using Yuzu.Clone;
var d = new List<int> { 1, 2, 3 };
var result = Cloner.Instance.Deep(d);
```

For a shallow clone use `Cloner.Shallow` method instead.

When cloning an object, only members with Yuzu attributes are copied.
Use [meta overrides](#meta-overrides) to clone non-serializable items:

```cs
public class Point { int X; int Y; }
var cl = new Cloner();
cl.Options.Meta = new MetaOptions().AddOverride(typeof(Point), o => o.AddAttr(new YuzuAll()));
var result = cl.Deep(new Point());
```

For a minor speedup of multiple deep clones of the same type, use `GetCloner`:
```cs
var cl = Cloner.Instance.GetCloner(typeof(MyClass));
var result = new List<MyClass>();
for(int i = 0; i < 1000000; ++i)
    result.Add(cl(new MyClass()));
```

Cloning behaves as similarly as possible to serialize / deserialize sequence.
In particular, it calls serialization and deserialization events, honors `[YuzuSerializeIf]` etc.

Note: Cloning of recursive objects graphs is currently not supported.

Use `Cloner.Merge(destination, source)` to copy/add fields and collections into an existing object.

## Type serialization

Types are serialized as strings.

To serialize type, use `Yuzu.Util.TypeSerializer.Serialize(typeof(MyType))`.

To deserialize type, use `Yuzu.Util.TypeSerializer.Deserialize("SerializedTypeName")`.

## Packing and unpacking to dictionary

Structured type instances may be converted to/from `Dictionary<string, object>` representation
by using `Pack` and `Unpack` functions of `Yuzu.DictOfObjects.DictOfObjects` class.

This is indended as a helper for reading badly structured JSON, e.g.
```cs
var d = (Dictionary<string, object>)JsonDeserializer.Instance.FromString(response);
if (d["type"] == "someType")
    return DictOfObjects.Unpack<SomeType>(d["value"]);
```
