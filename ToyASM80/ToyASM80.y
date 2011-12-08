/* Toy Assembler for Z80(Limited Instructions).  */
     
%{
	//#define YYSTYPE double
	#define YYSTYPE int
    #include <stdio.h>
    #include <ctype.h>
    #include <math.h>

	#include <stdlib.h>
	#include <string.h>
	#define SYMTABSIZE 4096
	#define CODESIZE (4096 * 4)
	#define UNDEFINED_SYMBOL (0xffffffff)
	extern char *yytext;

	int yylex(void);
	int yyparse(void);
	void yyerror (char const *);

	void putobj(int code);
	void puttype(int codeSize, int type);
	void increg8(int reg8);
	void ldr8r8(int dest, int src);
	void jp(int absoluteAddress);
	void jplabel(void);
	void labeldef(void);
	int symbolNum(char *symbol);

	int pass2(void);

	enum codetype {
		nolabel,
		labeled,
		labelActualValue
	};

%}
     

//%token NUM
	//%token REG8
%token REGA

%token REGB
%token REGC
%token REGD
%token REGE
%token REGH
%token REGL

%token HLADDR

	//%token MNEMOLD
%token INC
%token LD
%token JP

%token INTEGER
%token HEXINT
%token LABEL
%token LABELDEFINITION
     
%% /* Grammar rules and actions follow.  */

program:       /* EMPTY */ /* statement */
             | statement program
	;

statement:   /*MNEMOLD REG8 DELIM REG8
             |*/ 
			 LABELDEFINITION		{ labeldef() }
			 | INC reg8	{ increg8(yylval) } /* $x: yylval */
			 | LD reg8 ',' reg8 { ldr8r8($2, $4) }
			 /*| JP INTEGER { jp($2) } */
			 | JP number { jp($2) }
			 | JP LABEL { jplabel() }

reg8:		REGB | REGC | REGD | REGE | REGH | REGL | REGA
			| HLADDR
	;

number:		INTEGER | HEXINT
	;
%%


/* The lexical analyzer returns a double floating point
        number on the stack and the token NUM, or the numeric code
        of the character read if not a number.  It skips all blanks
        and tabs, and returns 0 for end-of-input.  */
     

/*
 * ���C���v���O����.
 *
 * �����͂��Ăяo���A�������ςނƏI������. 
 */

//char **symbols;
char *symbolTable[SYMTABSIZE];			// �V���{��������ւ̃|�C���^
unsigned int symbolValue[SYMTABSIZE];	// �V���{���Ɋ��蓖�Ă�ꂽ��̓I�ȃA�h���X
int symbols;							// �o�^�ς݃V���{����

unsigned char codeArray[CODESIZE];		// ���ԃI�u�W�F�N�g�̃o�C�g�\���̔z��
int codeCount;							// �o�͍ς݂̒��ԃI�u�W�F�N�g�̐�
int codeBytes;							// �o�͍ς݂̒��ԃI�u�W�F�N�g�̃o�C�g��
int sizeArray[CODESIZE];				// ���ԃI�u�W�F�N�g�̃}�V���R�[�h�E�T�C�Y
int typeArray[CODESIZE];				// ���ԃI�u�W�F�N�g�̃I�u�W�F�N�g�E�^�C�v
unsigned int usedSymbol[CODESIZE];		// ���ԃI�u�W�F�N�g�Ŏg�p�����V���{��

int main(void)
{
	int result;
	symbols = 0;
	codeCount = 0;
	codeBytes = 0;
	/* symbols = malloc(SYMTABSIZE); */
	result = yyparse();
	/* pass 2 */
	/* not implemented */
	if (result) return result;
	result = pass2();

	return result;
}

/*int
     main (void)
     {
		symbols = malloc(SYMTABSIZE);
		return yyparse();
     }*/


/* Called by yyparse on error.  */
     void
     yyerror (char const *s)
     {
       fprintf (stderr, "%s\n", s);
     }

void putobj(int code)
{
	//printf("object code: {%02x}\n", code);
	codeArray[codeBytes++] = (unsigned char)code;
}

void puttype(int codeSize, int type)
{
	printf("object type: {%02x}\n", type);
	sizeArray[codeCount] = codeSize;		// codeCount �Ԗڂ̒��ԃI�u�W�F�N�g�̃T�C�Y
	typeArray[codeCount] = type;			// �o�͂��钆�ԃI�u�W�F�N�g�̃^�C�v
	codeCount++;							// �o�͍ς݂̒��ԃI�u�W�F�N�g�̐��̃J�E���g�A�b�v
}

void increg8(int reg8)
{
	puttype(1, nolabel);
	putobj(0x04 | (reg8 << 3));

	printf("reg8: %d\n", reg8);
}

void ldr8r8(int dest, int src)
{
	puttype(1, nolabel);
	putobj(0x40 | (dest << 3) | src);
	printf("ld: (reg %d) <- (reg %d)\n", dest, src);
}

void jp(int absoluteAddress)
{
	puttype(3, nolabel);
	putobj(0xc3);
	putobj((absoluteAddress) & 0x00ff);
	putobj(((absoluteAddress) >> 8) & 0x00ff);
	printf("jp: nn = (%04x)\n", absoluteAddress);
}

void jplabel(void)
{
	usedSymbol[codeCount] = symbolNum(yytext);

	puttype(3, labeled);
	putobj(0xc3);
	putobj(0x00);
	putobj(0x00);

}

void labeldef(void)
{
	int symNum;
/*
	size_t sl = strlen(yytext);

	char *p = malloc(sl);
	printf("bison label definition: (%p)%s\n", yytext, yytext);
	strncpy(p, yytext, sl);
	symbolTable[symbols++] = p;
*/
	printf("bison label definition: (%p)%s\n", yytext, yytext);
	//symbolNum(yytext);
	symNum = symbolNum(yytext);
	printf("symnum = %d, codeBytes = %d\n", symNum, codeBytes);
	symbolValue[symNum] = codeBytes;	// TODO: ORG �����Ƃ��̍s�ւ̑Ή�
	//symbolTable[symbols++] = yytext;			// �V���{���o�����̔ԍ��m�F�p�ƁA�V���{���̍Ē�`���o�p
}

int symbolNum(/* const */char *symbol)
{
	int i;
	size_t sl, s1l;
	char *p;

	printf("finding symbol (%s)...", symbol);

	sl = strlen(symbol);
	if (symbol[sl - 1] == ':') sl--;

	for (i = 0; i < symbols; i++) {
		// �o�^�ς݂̃V���{�����Ȃ�����������
		printf("comparing with (%s)\n", symbolTable[i]);
		//s1l = strlen(symbol);
		//if (strncmp(symbol, symbolTable[i], s1l) == 0) {
		if (strncmp(symbol, symbolTable[i], sl) == 0) {
			// ��v����
			//if (s1l == strlen(symbolTable[i])) return i;	// ���������炻�̃V���{���ԍ���Ԃ�	// break;	
			if (sl == strlen(symbolTable[i])) return i;	// ���������炻�̃V���{���ԍ���Ԃ�	// break;	
		}
	}
	//if (i != symbols) return i;		// ���������炻�̃V���{���ԍ���Ԃ�

	// ������Ȃ������ꍇ�͐V���ɃV���{���\�ɒǉ�����
	
	p = malloc(sl + 1);		// NULL��������1�o�C�g�𑫂����A":"��1�o�C�g������
	strncpy(p, symbol, sl);	// �̈�̖����ɂ�strncpy()�ɂ��NULL�����������
	*(p + sl) = '\0';
/*
		if (symbol[sl - 1] == ':') {
			p = malloc(sl + 1 - 1);		// NULL��������1�o�C�g�𑫂����A":"��1�o�C�g������
			strncpy(p, symbol, sl - 1);	// �̈�̖����ɂ�strncpy()�ɂ��NULL�����������
			*(p + sl - 1) = '\0';
		} else {
			p = malloc(sl + 1);		// NULL��������1�o�C�g�𑫂����A":"��1�o�C�g������
			strncpy(p, symbol, sl);	// �̈�̖����ɂ�strncpy()�ɂ��NULL�����������
			*(p + sl) = '\0';
		}
*/
	symbolValue[symbols] = UNDEFINED_SYMBOL;
	symbolTable[symbols++] = p;
	printf("symbol (%s) added.\n", p);
	
	return i;
}

int pass2(void)
{
	// �ŏI�R�[�h������
	int i, j;
	unsigned int address;
	unsigned char *p;
	int *t;					// ���ԃI�u�W�F�N�g�̃^�C�v���g���o�[�X����|�C���^

	p = codeArray;
	for (i = 0; i < codeBytes; i++) {
		 // �Ƃ�ܒ��g�����̂܂�܏o��
		 printf("%02X, ", (int)*p);
		 p++;
	}
	printf("\n");

	printf("Defined Label (%s)\n", symbolTable[0]);
	p = codeArray;
	t = typeArray;
	for (i = 0; i < codeCount; i++) {
		if (*(t++) == labeled) {	// 0x??, 0xLL, 0xHH�^�C�v
			address = symbolValue[usedSymbol[i]];
			//printf("Reference: (%d)%p: %04X, ", usedSymbol[i], symbolTable[usedSymbol[i]], address);
			if (address == UNDEFINED_SYMBOL) {
				//yyerror()?
				printf("Undefined Symbol: \"%s\".\n", symbolTable[usedSymbol[i]]);
			}
			printf("%02X, %02X, %02X, ", p[0], address & 0x00ff, (address >> 8) & 0x00ff);			
			p += sizeArray[i];
		} else {
			for (j = 0; j < sizeArray[i]; j++) {
				printf("%02X, ", (int)*p);
				p++;
			}
		}
	}

	return 0;
}
