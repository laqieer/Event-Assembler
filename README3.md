To respect Nintenlord and Crazycolorz5, I put what I need to say here.

# Convert event scripts to assembly source files (C mode):
```
Core.exe C game_version -input:xxx.event -output:xxx.s
```
Supported *game_version* list:
- FE6
- FE7
- FE7J
- FE8
- FE8J

# Syntax differences in C mode:
* Extended syntax:
	+ GLOABL
	+ EXTERN
	+ SECTION
	+ ENDSECTION
* ORG can be skipped by -ignoreORG option
	
# Inline assembly:
Begin with "T".
Experimental. Not supposed to use.

# Relevance to decomp projects:
C mode is useful for decomp-style workflows because it emits assembly source instead of
patching a ROM directly. The generated `.s` can be assembled and linked by a decomp build,
especially when scripts avoid absolute `ORG` writes or use `-ignoreORG`.

For newer ColorzCore-based workflows, also see ColorzCore's AA mode. AA mode emits GAS
assembly plus a linker-script fragment, while this C mode is the older Event Assembler
path for producing assembly source.
