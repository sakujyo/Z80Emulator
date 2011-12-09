/* 
 * flex用字句解析
 *
 */

/*** Definition section ***/

%{

/*
 * C コードにはCの標準I/O ライブラリを使うものがある.
 * %{と %} で囲まれた部分はそのまま生成ファイルに
 * 取り込まれる.
 */

#include "ToyASM80.tab.h"
extern YYSTYPE yylval;

void debug(const char *format, ...);

%}

/* マクロ;  正規表現 */

delim               [ \t]
whitesp             {delim}+
comment             ;.*$

DIGIT               [0-9]
INTEGER             {DIGIT}+
HEXINT              0x{DIGIT}+

LABEL               [a-zA-Z][0-9a-zA-Z]+
                    /* 今のところ長さ制限なし */

LABELDEFINITION     {LABEL}:


/* これはflexに入力ファイルが一つであることを示す. */
%option noyywrap

%%
    /*
     * 規則領域
     *
     * コメントはインデントしなければならない.
     * そうしないと正規表現と誤認識してしまう.
     */

"A"     {
            debug("Saw an register %s\n", yytext);
            yylval = 7;
            return REGA;
        }

"B"     {
            debug("Saw an register %s\n", yytext);
            yylval = 0;
            return REGB;
        }

"C"     {
            debug("Saw an register %s\n", yytext);
            yylval = 1;
            return REGC;
        }

"D"     {
            debug("Saw an register %s\n", yytext);
            yylval = 2;
            return REGD;
        }

"E"     {
            debug("Saw an register %s\n", yytext);
            yylval = 3;
            return REGE;
        }

"H"     {
            debug("Saw an register %s\n", yytext);
            yylval = 4;
            return REGH;
        }

"L"     {
            debug("Saw an register %s\n", yytext);
            yylval = 5;
            return REGL;
        }

"(HL)"  {
            debug("Saw an register %s\n", yytext);
            yylval = 6;
            return HLADDR;
        }

"LD"    {
            debug("Saw an mnemonic LD \n");
            return LD;
        }

"INC"   {
            debug("Saw an mnemonic INC \n");
            return INC;
        }

"JP"    {
            debug("Saw an mnemonic JP \n");
            return JP;
        }

"ORG"   {
            debug("Saw an pseudo-instruction ORG \n");
            return ORG;
        }

    /*"x"   {
                printf("Saw an hexadecimal prefex: \n");
            }
    */

{LABEL} {
            debug("Saw an label: %s\n", yytext);
            return LABEL;
        }

{LABELDEFINITION}   {
                        debug("Saw an label definition: %s\n", yytext);
                        return LABELDEFINITION;
                    }

{HEXINT}    {
                debug("Saw an hex integer: %s\n", yytext);
                sscanf(yytext, "%x", &yylval);
                return HEXINT; 
            }

{INTEGER}   {
                /*
                 * この規則は入力から整数を表示する.
                 * yytextには一致した文字列が含まれる.
                 */
                debug("Saw an integer: %s\n", yytext);
                sscanf(yytext, "%d", &yylval);
                return INTEGER; 
            }

{whitesp}   {
                /* No action and no return */
            }

{comment}   {
                /* No action and no return */
            }

\n          {
                /* No action and no return */
            }

.           {
                return yytext[0]; /*それ以外をそのまま返してみる */
                /* それ以外の文字は無視. */
            }

%%
/*** Cコード領域 ***/
