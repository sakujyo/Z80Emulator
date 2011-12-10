#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <stdarg.h>

int mystrncasecmp(const char *s1, const char *s2, size_t n);

char filename[256];

struct Symbol {
    char *SymbolName;           // シンボル文字列へのポインタ
    unsigned int SymbolValue;   // シンボルに割り当てられた具体的なアドレス
};
int symbols;                    // 登録済みシンボル数

struct CodeObj {
    int Type;                   // 中間オブジェクトのオブジェクト・タイプ
    int Size;                   // 中間オブジェクトのマシンコード・サイズ
    int UsedSymbol;             // 中間オブジェクトで使用したシンボルの番号
    unsigned char code[4];      // マシンコード
    char *ListingString;        // リスティング出力用の文字列
};
int objCount;                   // 出力済みの中間オブジェクトの数

/*
struct SymbolList {
    int Count;
    struct Symbol *head;
    struct Symbol *tail;
};

struct CodeObjList {
    int Count;
    struct CodeObj *head;
    struct CodeObj *tail;
};*/

int codeBytes;                  // 出力済みの中間オブジェクトのバイト数

int mystrncasecmp(const char *s1, const char *s2, size_t n)
{
    char c1, c2;
    while (n > 0) {
        c1 = tolower(*s1);	s1++;
        c2 = tolower(*s2);	s2++;
        if (c1 < c2) return -1;
        if (c1 > c2) return 1;
        n--;
    }
    return 0;
}

void valistfunc(int count, ...)
{
    va_list argp;
    unsigned char code;
    int i;

    va_start(argp, count);
    for (i = 0; i < count; i++) {
        code = va_arg(argp, unsigned char);
        printf("(%c)", code);
    }
    va_end(argp);

    //vfprintf(stderr, format, argp);
}

int main(int argc, char *argv[])
{
    if ((argc != 2) ||(mystrncasecmp(argv[1] + strlen(argv[1]) - 4, ".Z80", (size_t)4) != 0)) {
        printf("Usage: %s assembly_source.z80\n", argv[0]);
        exit(EXIT_FAILURE);
    }

    strncpy(filename, argv[1], strlen(argv[1]) - (size_t)4);
    printf("Z80 Source: %s.Z80\n", filename);

    valistfunc(2, 0x41, 0x62);
    return 0;
}
