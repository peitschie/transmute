# transmute

A performant object-to-object mapper, that recursively maps child members automatically.

**DISCLAIMER: This was started as a personal side project and is not actively maintained. Use at own risk**

Key design goals:

1. Low runtime overhead
2. Runtime thread safety (after initialisation)
3. Immediate incomplete map detection during map initialisation stage at runtime (i.e., no late-bound errors if a type is unable to be completely mapped)

## Technical details

* Written for .NET 3.5 & 4, including support for Silverlight and Mono (last tested cira 2010!)
* Supports both IL emit-based mappers (maximum speed) and reflection-based mappers (less likely to break between major .NET versions)
* Contains both micro benchmarks and unit tests to ensure correct behaviour

Further documentation is available on [the wiki](http://transmute.tiddlyspot.com/#readOnly:yes)

# TODO (2010)
* RequiresMappingByDefault - ensure logic remains constant
* Maps with no actual setters - null mappers?
* Mapping object to object - throw exception? Ignore by default?