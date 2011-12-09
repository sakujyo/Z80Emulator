REM del /P lex.yy.c
del /P lex.yy.c
del /P ToyASM80.tab.c
del lex.yy.obj
del ToyASM80.tab.obj
del ToyASM80.tab.tds

REM 実行テスト
ToyASM80.tab.exe < sample1.z80
