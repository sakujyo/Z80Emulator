REM http://gnuwin32.sourceforge.net/packages/flex.htm
flex ToyASM80.l
REM http://gnuwin32.sourceforge.net/packages/bison.htm
bison --defines ToyASM80.y
REM BCC32‚Å‚ÌƒRƒ“ƒpƒCƒ‹
bcc32 ToyASM80.tab.c lex.yy.c
