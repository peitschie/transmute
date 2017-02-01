# transmute

A performant object-to-object mapper, that recursively maps child members automatically.

Key design goals:

1. Low runtime overhead
2. Runtime thread safety (after initialisation)
3. Immediate incomplete map detection during map initialisation stage at runtime (i.e., no late-bound errors if a type is unable to be completely mapped)

Further documentation is available on [the wiki](http://transmute.tiddlyspot.com/#readOnly:yes)