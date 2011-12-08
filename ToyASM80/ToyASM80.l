/* 
 * flex�p������
 *
 */

/*** Definition section ***/

%{

/*
 * C �R�[�h�ɂ�C�̕W��I/O ���C�u�������g�����̂�����.
 * %{�� %} �ň͂܂ꂽ�����͂��̂܂ܐ����t�@�C����
 * ��荞�܂��.
 */

#include "ToyASM80.tab.h"
extern YYSTYPE yylval;

%}

/* �}�N��;  ���K�\�� */

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
	/* ���̂Ƃ��뒷�������Ȃ� */
LABELDEFINITION	{LABEL}:

/* �����flex�ɓ��̓t�@�C������ł��邱�Ƃ�����. */
%option noyywrap

%%
    /*
     * �K���̈�
     *
     * �R�����g�̓C���f���g���Ȃ���΂Ȃ�Ȃ�.
     * �������Ȃ��Ɛ��K�\���ƌ�F�����Ă��܂�.
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
                 * ���̋K���͓��͂��琮����\������.
                 * yytext�ɂ͈�v���������񂪊܂܂��.
                 */
                printf("Saw an integer: %s\n", yytext);
				sscanf(yytext, "%d", &yylval);
				return INTEGER; 
            }

{whitesp} { /* No action and no return */}

.           { return yytext[0]; /*����ȊO�����̂܂ܕԂ��Ă݂� */ /* ����ȊO�̕����͖���. */ }

%%
/*** C�R�[�h�̈� ***/