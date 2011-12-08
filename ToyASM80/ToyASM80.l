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

%}

/* マクロ;  正規表現 */

delim         [ \t]
whitesp       {delim}+


/*
BINPREFIX   b
*/

DIGIT       [0-9]
INTEGER     {DIGIT}+
	//HP			0x
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
			printf("Saw an register %s\n", yytext);
			yylval = 7;
			return REGA;
		}

"B"		{
			printf("Saw an register %s\n", yytext);
			yylval = 0;
			return REGB;
		}

"C"		{
			printf("Saw an register %s\n", yytext);
			yylval = 1;
			return REGC;
		}

"D"		{
			printf("Saw an register %s\n", yytext);
			yylval = 2;
			return REGD;
		}

"E"		{
			printf("Saw an register %s\n", yytext);
			yylval = 3;
			return REGE;
		}

"H"		{
			printf("Saw an register %s\n", yytext);
			yylval = 4;
			return REGH;
		}

"L"		{
			printf("Saw an register %s\n", yytext);
			yylval = 5;
			return REGL;
		}

"LD"	{
			printf("Saw an mnemonic LD \n");
			return LD;
	    }

"INC"	{
			printf("Saw an mnemonic INC \n");
			return INC;
	    }

"JP"	{
			printf("Saw an mnemonic JP \n");
			return JP;
	    }

"(HL)"	{
			printf("Saw an mnemonic REG8 \n");
			yylval = 6;
			return HLADDR;
	    }

	/*"x"		{
				printf("Saw an hexadecimal prefex: \n");
			}
	*/

{LABEL}		{
				printf("Saw an label: %s\n", yytext);
				return LABEL;
			}

{LABELDEFINITION} {
				printf("Saw an label definition: %s\n", yytext);
				return LABELDEFINITION;
			}

{HEXINT}	{
                printf("Saw an hex integer: %s\n", yytext);
				sscanf(yytext, "%x", &yylval);
				return HEXINT; 
			}

{INTEGER}   {
                /*
                 * この規則は入力から整数を表示する.
                 * yytextには一致した文字列が含まれる.
                 */
                printf("Saw an integer: %s\n", yytext);
				sscanf(yytext, "%d", &yylval);
				return INTEGER; 
            }

{whitesp} { /* No action and no return */}

.           { return yytext[0]; /*それ以外をそのまま返してみる */ /* それ以外の文字は無視. */ }

%%
/*** Cコード領域 ***/
