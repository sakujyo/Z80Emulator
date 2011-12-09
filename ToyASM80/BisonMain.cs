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
char *listArray[CODESIZE];              // リスティング出力用の配列
unsigned int relocAddress;

const char *reg8name[8] = {
    "B", "C", "D", "E", "H", "L", "(HL)", "A"
};

enum reg8enum {
    reg8_b = 0,
    reg8_c = 1,
    reg8_d = 2,
    reg8_e = 3,
    reg8_h = 4,
    reg8_l = 5,
    reg8_hl = 6,
    reg8_a
};

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
    debug("object type: {%02x}\n", type);   // codeBytes更新までにすべての仕事を終えてくださいね
    sizeArray[codeCount] = codeSize;		// codeCount 番目の中間オブジェクトのサイズ
    typeArray[codeCount] = type;			// 出力する中間オブジェクトのタイプ
    codeCount++;							// 出力済みの中間オブジェクトの数のカウントアップ
}

void increg8(int reg8)
{
    char *p;
    debug("reg8: %d\n", reg8);
    p = malloc((size_t)25);
    snprintf(p, 24, "INC      %s              ", reg8name[reg8]);
    listArray[codeCount] = p;
    
    puttype(1, nolabel);
    putobj(0x04 | (reg8 << 3));
}

void ldr8r8(int dest, int src)
{
    char *p;
    debug("ld: (reg %d) <- (reg %d)\n", dest, src);
    p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, %s            ", reg8name[dest], reg8name[src]);
    listArray[codeCount] = p;
    puttype(1, nolabel);
    putobj(0x40 | (dest << 3) | src);
}

void jp(int absoluteAddress)
{
    char *p;
    p = malloc((size_t)25);
    snprintf(p, 24, "JP      0x%04X            ", absoluteAddress);
    listArray[codeCount] = p;
    puttype(3, nolabel);
    putobj(0xc3);
    putobj((absoluteAddress) & 0x00ff);
    putobj(((absoluteAddress) >> 8) & 0x00ff);
    trace("jp: nn = (%04x)\n", absoluteAddress);
}

void jplabel(void)
{
    char *p;
    usedSymbol[codeCount] = symbolNum(yytext);

    p = malloc((size_t)25);
    snprintf(p, 24, "JP      %s               ", yytext);
    listArray[codeCount] = p;
    puttype(3, labeled);
    putobj(0xc3);
    putobj(0x00);
    putobj(0x00);
}

void deflabel(void)
{
    int symNum;
    char *p;

    trace("Bison LABEL definition: (%p)%s\n", yytext, yytext);
    symNum = symbolNum(yytext);
    trace("symnum = %d, codeBytes = %d\n", symNum, codeBytes);
    symbolValue[symNum] = codeBytes;    // TODO: ORG 実装とこの行への対応
    //symbolTable[symbols++] = yytext;  // これはやっちゃダメなパターンのやつ

    p = malloc((size_t)25);
    snprintf(p, 24, "%s                       ", yytext);
    listArray[codeCount] = p;
    puttype(0, labeldef);
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
    int i, j, k;
    unsigned int address;
    unsigned char *p;
    int *t;                 // 中間オブジェクトのタイプをトラバースするポインタ

    char lcode[25];         // リスティング左列
    char lmnemo[25];        // リスティング中央列

    p = codeArray;
    t = typeArray;
    for (i = 0; i < codeCount; i++) {
        fprintf(stdout, "%04X:", (p - codeArray));
        switch (*(t++)) {
        case labeled:           // 0x??, 0xLL, 0xHHタイプ
            address = symbolValue[usedSymbol[i]];
            //printf("Reference: (%d)%p: %04X, ", usedSymbol[i], symbolTable[usedSymbol[i]], address);
            if (address == UNDEFINED_SYMBOL) {
                //yyerror()?
                printf("Undefined Symbol: \"%s\".\n", symbolTable[usedSymbol[i]]);
            }
            printf("%02X %02X %02X ", p[0], address & 0x00ff, (address >> 8) & 0x00ff);			
            p += sizeArray[i];
            fprintf(stdout, "          %s;comments\n", listArray[i]);
            break;
        case labeldef:
            fprintf(stdout, "%s\n", listArray[i]);
            break;
        default:
            for (j = 0; j < sizeArray[i]; j++) {
                fprintf(stdout, "%02X ", (int)*p);
                p++;
            }
            for (k = 0; k < 24 - 5 - (j * 3); k++) fputc(' ', stdout);
            // ニーモニック出力
            //fprintf(stdout, "mnemo   r, 0x____       ;comments\n");
            fprintf(stdout, "%s;comments\n", listArray[i]);
            break;
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
