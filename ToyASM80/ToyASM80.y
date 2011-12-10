/* Toy Assembler for Z80(Limited Instructions). */

%{
    //#define YYSTYPE double
    #define YYSTYPE int
    #include <stdio.h>
    #include <ctype.h>
    #include <math.h>

    #include <stdlib.h>
    #include <string.h>

    #include <stdarg.h>

    #define SYMTABSIZE          4096
    #define CODESIZE            (4096 * 4)
    #define UNDEFINED_SYMBOL    (0xffffffff)
    extern char *yytext;

    int yylex(void);
    int yyparse(void);
    void yyerror (char const *);
	
    // 単なるWarning対策。インタフェース変わる可能性あり
    void yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep);

    void putobj(int code);
    void puttype(int codeSize, int type);
    void increg8(int reg8);
    void ldr8r8(int dest, int src);
    void jp(int absoluteAddress);
    void jplabel(void);
    void out(int port);
    void ldregim8(int dest, int immediate);
    //void ldaim8(int reg8, int immediate);
    void deflabel(void);
    void org(int address);
    int symbolNum(char *symbol);

    int pass2(void);

    void debug(const char *format, ...);
    void trace(const char *format, ...);

    enum codetype {
        nolabel,
        labeled,            // ラベルのアドレスの埋め込み
        labeldef,           // ラベルのアドレスのリスティング出力用
        ORIGIN              // アドレス指定
    };

%}


%token REGA

%token REGB
%token REGC
%token REGD
%token REGE
%token REGH
%token REGL

%token HLADDR

%token INC
%token LD
%token JP
%token OUT

%token INTEGER
%token HEXINT
%token LABEL
%token LABELDEFINITION

%token ORG

%token NEWLINE

%% /* Grammar rules and actions follow.  */

program:    /* EMPTY */
            | program statement
    ;

    /*line: */

statement:  LABELDEFINITION         { deflabel() }
            | INC reg8              { increg8($2) }     /* $x: yylval */
            | LD reg8 ',' reg8      { ldr8r8($2, $4) }
            | JP number             { jp($2) }
            | JP LABEL              { jplabel() }
            | ORG number            { org($2) }
            | OUT number            { out($2) }
            | LD reg8 ',' number    { ldregim8($2, $4) }
    ;

reg8:       REGB | REGC | REGD | REGE | REGH | REGL | REGA
            | HLADDR
    ;

number:     INTEGER | HEXINT
    ;

%%

#include "BisonMain.cs"
