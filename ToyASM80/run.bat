REM del /P lex.yy.c
del lex.yy.c
REM del /P ToyASM80.tab.c
del ToyASM80.tab.c
del lex.yy.obj
del ToyASM80.tab.obj
del ToyASM80.tab.tds

REM 実行テスト
ToyASM80.tab.exe 0< sample1.z80
