#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <stdarg.h>

int mystrncasecmp(const char *s1, const char *s2, size_t n);

char filename[256];

struct Symbol {
    char *SymbolName;           // �V���{��������ւ̃|�C���^
    unsigned int SymbolValue;   // �V���{���Ɋ��蓖�Ă�ꂽ��̓I�ȃA�h���X
};
int symbols;                    // �o�^�ς݃V���{����

struct CodeObj {
    int Type;                   // ���ԃI�u�W�F�N�g�̃I�u�W�F�N�g�E�^�C�v
    int Size;                   // ���ԃI�u�W�F�N�g�̃}�V���R�[�h�E�T�C�Y
    int UsedSymbol;             // ���ԃI�u�W�F�N�g�Ŏg�p�����V���{���̔ԍ�
    unsigned char code[4];      // �}�V���R�[�h
    char *ListingString;        // ���X�e�B���O�o�͗p�̕�����
};
int objCount;                   // �o�͍ς݂̒��ԃI�u�W�F�N�g�̐�

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

int codeBytes;                  // �o�͍ς݂̒��ԃI�u�W�F�N�g�̃o�C�g��

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
