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
* Disabled syntax:
	- ORG
	- PUSH
	- POP
	
# Inline assembly:
Begin with "T".
Experimental. Not supposed to use.
