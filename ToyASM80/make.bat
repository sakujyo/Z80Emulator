REM bcc32 -DDEBUG -c trace.c
REM bcc32 -c trace.c

REM http://gnuwin32.sourceforge.net/packages/flex.htm
flex ToyASM80.l
REM http://gnuwin32.sourceforge.net/packages/bison.htm
bison --defines ToyASM80.y
REM BCC32‚Å‚ÌƒRƒ“ƒpƒCƒ‹
bcc32 -eToyASM80.exe ToyASM80.tab.c lex.yy.c trace.obj
