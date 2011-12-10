/*
 * メインプログラム.
 *
 * 字句解析を呼び出し、処理が済むと終了する. 
 */


char *symbolTable[SYMTABSIZE];          // シンボル文字列へのポインタ
unsigned int symbolValue[SYMTABSIZE];   // シンボルに割り当てられた具体的なアドレス
int symbols;                            // 登録済みシンボル数

/*
struct Symbol {
    char *SymbolName;               // シンボル文字列へのポインタ
    unsigned int SymbolValue;       // シンボルに割り当てられた具体的なアドレス
};
int symbolsCount;                   // 登録済みシンボル数
struct Symbol symbols[SYMTABSIZE];
*/

struct CodeObj {
    int Type;                       // 中間オブジェクトのオブジェクト・タイプ
    int Size;                       // 中間オブジェクトのマシンコード・サイズ
    int UsedSymbol;                 // 中間オブジェクトで使用したシンボルの番号
    unsigned char code[4];          // マシンコード
    char *ListingString;            // リスティング出力用の文字列
};
int objsCount;                      // 出力済みの中間オブジェクトの数
struct CodeObj objs[CODESIZE];

int codeBytes;                      // 出力済みの中間オブジェクトのバイト数
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

int main(int argc, char *argv[])
{
    int result;
    //symbolsCount = 0;
    symbols = 0;
    objsCount = 0;
    codeBytes = 0;
    relocAddress = 0;

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

void setObj(int type, char *listingString, int size, ...)
{
    //Type, ListingString, Size, マシンコードをセットする
    va_list argp;
    unsigned char code;
    int i;

    va_start(argp, size);
    for (i = 0; i < size; i++) {
        code = va_arg(argp, unsigned char);
        debug("object code: {%02x}\n", code);
        objs[objsCount].code[i] = code;
        codeBytes++;
    }
    objs[objsCount].Size = size;
    va_end(argp);

    debug("object type: {%02x}\n", type);
    objs[objsCount].Type = type;

    objs[objsCount].ListingString = listingString;

    objsCount++;        // objsCount 更新までにすべての仕事を終えてくださいね
}

void increg8(int reg8)
{
    char *p;
    debug("reg8: %d\n", reg8);
    p = malloc((size_t)25);
    snprintf(p, 24, "INC     %s               ", reg8name[reg8]);

    setObj(NOLABEL, p, 1, 0x04 | (reg8 << 3));
}

void out(int port)
{
    char *p;
    debug("out port: %d\n", port);
    p = malloc((size_t)25);
    snprintf(p, 24, "OUT     0x%02X            ", port);

    setObj(NOLABEL, p, 2, 0xd3, port & 0x00ff);
}

void ldr8r8(int dest, int src)
{
    char *p;
    debug("ld: (reg %d) <- (reg %d)\n", dest, src);
    p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, %s            ", reg8name[dest], reg8name[src]);

    setObj(NOLABEL, p, 1, 0x40 | (dest << 3) | src);
}

void ldregim8(int dest, int immediate)
{
    char *p;
    debug("ld: (reg %d) <- (immediate %d)\n", dest, immediate);
    p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, 0x%02X         ", reg8name[dest], immediate);

    setObj(NOLABEL, p, 2, 0x06 | (dest << 3), immediate);
}

void jp(int absoluteAddress)
{
    //絶対アドレスジャンプ
    char *p;

    p = malloc((size_t)25);
    snprintf(p, 24, "JP      0x%04X            ", absoluteAddress);
    trace("jp: nn = (%04x)\n", absoluteAddress);

    setObj(NOLABEL, p, 3, 0xc3, (absoluteAddress) & 0x00ff, ((absoluteAddress) >> 8) & 0x00ff);
}

void jplabel(void)
{
    char *p;
    objs[objsCount].UsedSymbol = symbolNum(yytext);

    p = malloc((size_t)25);
    snprintf(p, 24, "JP      %s               ", yytext);

    setObj(LABELED, p, 3, 0xc3, 0x00, 0x00);
}

void org(int address)
{
    char *p;

    p = malloc((size_t)25);
    snprintf(p, 24, "ORG     0x%04X           ", address);

    relocAddress = address;
    setObj(ORIGIN, p, 0);
}

void deflabel(void)
{
    int symNum;
    char *p;

    trace("Bison LABEL definition: (%p)%s\n", yytext, yytext);
    symNum = symbolNum(yytext);
    trace("symnum = %d, address = %d\n", symNum, relocAddress + codeBytes);
    symbolValue[symNum] = relocAddress + codeBytes;    // ORG 対応

    p = malloc((size_t)25);
    snprintf(p, 24, "%s                       ", yytext);

    setObj(LABELDEF, p, 0);
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
    //symbolTable[symbols++] = yytext;  // これはやっちゃダメなパターンのやつ
    trace("symbol (%s) added.\n", p);
	
    return i;
}

int pass2(void)
{
    // 最終コード生成部
    int i, j, k, codeCount = 0;
    unsigned int address;       //現時点では ORG 擬似命令は一度しか使用してはいけない仕様

    for (i = 0; i < objsCount; i++) {
        fprintf(stdout, "%04X:", relocAddress + codeCount);
        switch (objs[i].Type) {
        case LABELED:           // 0x??, 0xLL, 0xHHタイプ
            address = symbolValue[objs[i].UsedSymbol];
            if (address == UNDEFINED_SYMBOL) {
                //yyerror()?
                printf("Undefined Symbol: \"%s\".\n", symbolTable[objs[i].UsedSymbol]);
            }
            printf("%02X %02X %02X ", objs[i].code[0], address & 0x00ff, (address >> 8) & 0x00ff);			
            fprintf(stdout, "          %s;comments\n", objs[i].ListingString);  // ニーモニック出力
            break;
        case LABELDEF:
            fprintf(stdout, "%s\n", objs[i].ListingString);             // ニーモニック出力
            break;
        default:
            for (j = 0; j < objs[i].Size; j++) {
                fprintf(stdout, "%02X ", objs[i].code[j]);
            }
            for (k = 0; k < 24 - 5 - (j * 3); k++) fputc(' ', stdout);
            fprintf(stdout, "%s;comments\n", objs[i].ListingString);    // ニーモニック出力
            break;
        }
        codeCount += objs[i].Size;
    }

    return 0;
}

void trace(const char *format, ...)
{
    va_list argp;

    va_start(argp, format);
    //vfprintf(stderr, format, argp);
}
