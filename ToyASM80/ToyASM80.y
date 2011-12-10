/* Toy Assembler for Z80(Limited Instructions). */

%{
    //#define YYSTYPE double
    #define YYSTYPE int

    #include <stdlib.h>

    #define SYMTABSIZE          (4096)
    #define CODESIZE            (4096 * 4)
    #define ISDEFINED_FALSE     (0)
    extern char *yytext;        // これ必要。重要。

    int yylex(void);
    int yyparse(void);
    void yyerror (char const *);
	
    // 単なるWarning対策。インタフェース変わる可能性あり
    void yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep);

    void increg8(int reg8);
    void ldr8r8(int dest, int src);
    void jp(int absoluteAddress);
    void jplabel(void);
    void out(int port);
    void ldregim8(int dest, int immediate);
    void deflabel(void);
    void org(int address);


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
            | ORG number            { org($2) }
            | INC reg8              { increg8($2) }     /* $x: yylval */
            | LD reg8 ',' reg8      { ldr8r8($2, $4) }
            | JP number             { jp($2) }
            | JP LABEL              { jplabel() }
            | OUT number            { out($2) }
            | LD reg8 ',' number    { ldregim8($2, $4) }
    ;

reg8:       REGB | REGC | REGD | REGE | REGH | REGL
            | HLADDR
            | REGA
    ;

number:     INTEGER | HEXINT
    ;

%%

#include "BisonMain.cs"
