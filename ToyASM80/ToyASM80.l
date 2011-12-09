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

#include <stdarg.h>
void pmsg(const char *format, ...);

%}

/* マクロ;  正規表現 */

delim         [ \t]
whitesp       {delim}+

DIGIT       [0-9]
INTEGER     {DIGIT}+
HEXINT		0x{DIGIT}+

LABEL		[a-zA-Z][0-9a-zA-Z]+
	/* 今のところ長さ制限なし */

LABELDEFINITION	{LABEL}:

/* これはflexに入力ファイルが一つであることを示す. */
%option noyywrap

%%
    /*
     * 規則領域
     *
     * コメントはインデントしなければならない.
     * そうしないと正規表現と誤認識してしまう.
     */

"A"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 7;
			return REGA;
		}

"B"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 0;
			return REGB;
		}

"C"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 1;
			return REGC;
		}

"D"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 2;
			return REGD;
		}

"E"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 3;
			return REGE;
		}

"H"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 4;
			return REGH;
		}

"L"		{
			pmsg("Saw an register %s\n", yytext);
			yylval = 5;
			return REGL;
		}

"(HL)"	{
			pmsg("Saw an register %s\n", yytext);
			yylval = 6;
			return HLADDR;
	    }

"LD"	{
			pmsg("Saw an mnemonic LD \n");
			return LD;
	    }

"INC"	{
			pmsg("Saw an mnemonic INC \n");
			return INC;
	    }

"JP"	{
			pmsg("Saw an mnemonic JP \n");
			return JP;
	    }

	/*"x"		{
				printf("Saw an hexadecimal prefex: \n");
			}
	*/

{LABEL}		{
				pmsg("Saw an label: %s\n", yytext);
				return LABEL;
			}

{LABELDEFINITION} {
				pmsg("Saw an label definition: %s\n", yytext);
				return LABELDEFINITION;
			}

{HEXINT}	{
                pmsg("Saw an hex integer: %s\n", yytext);
				sscanf(yytext, "%x", &yylval);
				return HEXINT; 
			}

{INTEGER}   {
                /*
                 * この規則は入力から整数を表示する.
                 * yytextには一致した文字列が含まれる.
                 */
                pmsg("Saw an integer: %s\n", yytext);
				sscanf(yytext, "%d", &yylval);
				return INTEGER; 
            }

{whitesp}	{
				/* No action and no return */
			}

.           {
				return yytext[0]; /*それ以外をそのまま返してみる */
				/* それ以外の文字は無視. */
			}

%%
/*** Cコード領域 ***/
void pmsg(const char *format, ...)
{
	/* #ifdef DEBUG */
	va_list argp;

	va_start(argp, format);
	//vfprintf(stderr, format, argp);

	/* #endif */
	//printf(format, msg);
}
