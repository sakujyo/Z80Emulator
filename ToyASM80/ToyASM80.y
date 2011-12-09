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
    void labeldef(void);
    int symbolNum(char *symbol);

    int pass2(void);

    void debug(const char *format, ...);
    void trace(const char *format, ...);

    enum codetype {
        nolabel,
        labeled,
        labelActualValue
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

%token INTEGER
%token HEXINT
%token LABEL
%token LABELDEFINITION

%token NEWLINE

%% /* Grammar rules and actions follow.  */

program:    /* EMPTY */
            | program statement
    ;

    /*line: */

statement:  LABELDEFINITION     { labeldef() }
            | INC reg8          { increg8($2) }     /* $x: yylval */
            | LD reg8 ',' reg8  { ldr8r8($2, $4) }
            | JP number         { jp($2) }
            | JP LABEL          { jplabel() }
    ;

reg8:       REGB | REGC | REGD | REGE | REGH | REGL | REGA
            | HLADDR
    ;

number:     INTEGER | HEXINT
    ;

%%


/* The lexical analyzer returns a double floating point
        number on the stack and the token NUM, or the numeric code
        of the character read if not a number.  It skips all blanks
        and tabs, and returns 0 for end-of-input.  */

/*
 * メインプログラム.
 *
 * 字句解析を呼び出し、処理が済むと終了する. 
 */

//char **symbols;
char *symbolTable[SYMTABSIZE];          // シンボル文字列へのポインタ
unsigned int symbolValue[SYMTABSIZE];   // シンボルに割り当てられた具体的なアドレス
int symbols;                            // 登録済みシンボル数

unsigned char codeArray[CODESIZE];      // 中間オブジェクトのバイト表現の配列
int codeCount;                          // 出力済みの中間オブジェクトの数
int codeBytes;                          // 出力済みの中間オブジェクトのバイト数
int sizeArray[CODESIZE];                // 中間オブジェクトのマシンコード・サイズ
int typeArray[CODESIZE];                // 中間オブジェクトのオブジェクト・タイプ
unsigned int usedSymbol[CODESIZE];      // 中間オブジェクトで使用したシンボル

int main(void)
{
    int result;
    symbols = 0;
    /* symbols = malloc(SYMTABSIZE); */
    codeCount = 0;
    codeBytes = 0;
    result = yyparse();

    /* pass 2 */
    if (result) return result;
    result = pass2();

    return result;
}

/* Called by yyparse on error.  */
     void
     yyerror (char const *s)
     {
       fprintf (stderr, "%s\n", s);
     }

void putobj(int code)
{
    //debug("object code: {%02x}\n", code);
    codeArray[codeBytes++] = (unsigned char)code;
}

void puttype(int codeSize, int type)
{
    debug("object type: {%02x}\n", type);
    sizeArray[codeCount] = codeSize;		// codeCount 番目の中間オブジェクトのサイズ
    typeArray[codeCount] = type;			// 出力する中間オブジェクトのタイプ
    codeCount++;							// 出力済みの中間オブジェクトの数のカウントアップ
}

void increg8(int reg8)
{
    debug("reg8: %d\n", reg8);
    puttype(1, nolabel);
    putobj(0x04 | (reg8 << 3));
}

void ldr8r8(int dest, int src)
{
    debug("ld: (reg %d) <- (reg %d)\n", dest, src);
    puttype(1, nolabel);
    putobj(0x40 | (dest << 3) | src);
}

void jp(int absoluteAddress)
{
    puttype(3, nolabel);
    putobj(0xc3);
    putobj((absoluteAddress) & 0x00ff);
    putobj(((absoluteAddress) >> 8) & 0x00ff);
    trace("jp: nn = (%04x)\n", absoluteAddress);
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

    trace("Bison LABEL definition: (%p)%s\n", yytext, yytext);
    symNum = symbolNum(yytext);
    trace("symnum = %d, codeBytes = %d\n", symNum, codeBytes);
    symbolValue[symNum] = codeBytes;    // TODO: ORG 実装とこの行への対応
    //symbolTable[symbols++] = yytext;  // これはやっちゃダメなパターンのやつ
}

int symbolNum(char *symbol)
{
    int i;
    size_t sl;
    char *p;

    sl = strlen(symbol);
    if (symbol[sl - 1] == ':') sl--;    // ラベル定義の場合は、":"の1バイトを引く

    trace("finding symbol (%s)...\n", symbol);
    for (i = 0; i < symbols; i++) {
        // 登録済みのシンボルがないか検索する
        trace("comparing with (%s)\n", symbolTable[i]);
        if (strncmp(symbol, symbolTable[i], sl) == 0) {
            // 一致した
            if (sl == strlen(symbolTable[i])) return i;	    // 見つかったらそのシンボル番号を返す	
        }
    }

    // 見つからなかった場合は新たにシンボル表に追加する
    // シンボル出現時の番号確認用と、シンボルの再定義(エラー)検出用
    p = malloc(sl + 1);		// NULL文字分の1バイトを足す
    strncpy(p, symbol, sl);	// 領域の末尾にはstrncpy()によりNULL文字が補われる
    *(p + sl) = '\0';

    symbolValue[symbols] = UNDEFINED_SYMBOL;
    symbolTable[symbols++] = p;
    trace("symbol (%s) added.\n", p);
	
    return i;
}

int pass2(void)
{
    // 最終コード生成部
    int i, j;
    unsigned int address;
    unsigned char *p;
    int *t;                 // 中間オブジェクトのタイプをトラバースするポインタ

    p = codeArray;
    t = typeArray;
    for (i = 0; i < codeCount; i++) {
        if (*(t++) == labeled) {	// 0x??, 0xLL, 0xHHタイプ
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

void trace(const char *format, ...)
{
    va_list argp;

    va_start(argp, format);
    //vfprintf(stderr, format, argp);
}
