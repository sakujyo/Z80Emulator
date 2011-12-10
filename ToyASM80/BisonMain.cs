/*
 * ���C���v���O����.
 *
 * �����͂��Ăяo���A�������ςނƏI������. 
 */

#include <stdio.h>
#include <string.h>
#include <ctype.h>

#include <stdarg.h>

#define MAXFNLEN    (251)           // �A�Z���u���E�\�[�X��".z80"���������ő咷��

struct CodeObj {
    int Type;                       // ���ԃI�u�W�F�N�g�̃I�u�W�F�N�g�E�^�C�v
    int Size;                       // ���ԃI�u�W�F�N�g�̃}�V���R�[�h�E�T�C�Y
    int UsedSymbol;                 // ���ԃI�u�W�F�N�g�Ŏg�p�����V���{���̔ԍ�
    unsigned char code[4];          // �}�V���R�[�h
    char *ListingString;            // ���X�e�B���O�o�͗p�̕�����
};
int objsCount;                      // �o�͍ς݂̒��ԃI�u�W�F�N�g�̐�
struct CodeObj objs[CODESIZE];

struct Symbol {
    char *Name;                     // �V���{��������ւ̃|�C���^
    unsigned int Value;             // �V���{���Ɋ��蓖�Ă�ꂽ��̓I�ȃA�h���X
    int isDefined;                  // 0 �ȊO�Ȃ犄�蓖�Ă��Ă���
};
int symbolsCount;                   // �o�^�ς݃V���{����
struct Symbol symbols[SYMTABSIZE];

int codeBytes;                      // �o�͍ς݂̒��ԃI�u�W�F�N�g�̃o�C�g��
unsigned int relocAddress;

const char *reg8name[8] = {
    "B", "C", "D", "E", "H", "L", "(HL)", "A"
};

enum codetype {
    NOLABEL,
    LABELED,            // ���x���̃A�h���X�̖��ߍ���
    LABELDEF,           // ���x���̃A�h���X�̃��X�e�B���O�o�͗p
    ORIGIN              // �A�h���X�w��
};


int mystrncasecmp(const char *s1, const char *s2, size_t n);
void setObj(int type, char *listingString, int size, ...);
int symbolNum(char *symbol);

int pass2(void);

void debug(const char *format, ...);
void trace(const char *format, ...);

extern FILE *yyin;

FILE *fpasm;
FILE *fpbin;
FILE *fplst;

int main(int argc, char *argv[])
{
    int result, fnLen;
    char fnbin[MAXFNLEN + 4 + 1], fnlst[MAXFNLEN + 4 + 1];

    symbolsCount = 0;
    objsCount = 0;
    codeBytes = 0;
    relocAddress = 0;

    if ((argc != 2) || (strlen(argv[1]) < 4) || (mystrncasecmp(argv[1] + strlen(argv[1]) - 4, ".Z80", (size_t)4) != 0)) {
        printf("Usage: %s assembly_source.z80\n", argv[0]);
        exit(EXIT_FAILURE);
    }
    fnLen = strlen(argv[1]) - (size_t)4;
    if (fnLen > MAXFNLEN) fnLen = MAXFNLEN;
    strncpy(fnlst, argv[1], fnLen);
    strncpy(fnlst + fnLen, ".lst", 5);
    fprintf(stderr, "Listing Filename: %s\n", fnlst);
    strncpy(fnbin, argv[1], fnLen);
    strncpy(fnbin + fnLen, ".bin", 5);
    fprintf(stderr, "Binary Filename: %s\n", fnbin);
    fpasm = fopen(argv[1], "r");
    if (!fpasm) {
        fprintf(stderr, "Can't open source: %s\n", argv[1]);
        exit(EXIT_FAILURE);
    }

    yyin = fpasm;
    result = yyparse();
    if (result) return result;

    /* pass 2 */
    fplst = fopen(fnlst, "w");
    if (!fplst) {
        fprintf(stderr, "Can't open output listing: %s\n", fnlst);
        exit(EXIT_FAILURE);
    }
    fpbin = fopen(fnbin, "wb");
    if (!fpbin) {
        fprintf(stderr, "Can't open output binary: %s\n", fnbin);
        exit(EXIT_FAILURE);
    }
    result = pass2();
    fclose(fplst);
    fclose(fpbin);

    return result;
}

/* Called by yyparse on error.  */
void yyerror (char const *s)
{
    fprintf (stderr, "%s\n", s);
}

void setObj(int type, char *listingString, int size, ...)
{
    //Type, ListingString, Size, �}�V���R�[�h���Z�b�g����
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

    objsCount++;        // objsCount �X�V�܂łɂ��ׂĂ̎d�����I���Ă���������
}

void increg8(int reg8)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "INC     %s               ", reg8name[reg8]);
    debug("reg8: %d\n", reg8);

    setObj(NOLABEL, p, 1, 0x04 | (reg8 << 3));
}

void out(int port)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "OUT     0x%02X            ", port);
    debug("out port: %d\n", port);

    setObj(NOLABEL, p, 2, 0xd3, port & 0x00ff);
}

void ldr8r8(int dest, int src)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, %s            ", reg8name[dest], reg8name[src]);
    debug("ld: (reg %d) <- (reg %d)\n", dest, src);

    setObj(NOLABEL, p, 1, 0x40 | (dest << 3) | src);
}

void ldregim8(int dest, int immediate)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, 0x%02X         ", reg8name[dest], immediate);
    debug("ld: (reg %d) <- (immediate %d)\n", dest, immediate);

    setObj(NOLABEL, p, 2, 0x06 | (dest << 3), immediate);
}

void jp(int absoluteAddress)
{
    //��΃A�h���X�W�����v
    char *p = malloc((size_t)25);
    snprintf(p, 24, "JP      0x%04X            ", absoluteAddress);
    trace("jp: nn = (%04x)\n", absoluteAddress);

    setObj(NOLABEL, p, 3, 0xc3, (absoluteAddress) & 0x00ff, ((absoluteAddress) >> 8) & 0x00ff);
}

void jplabel(void)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "JP      %s               ", yytext);

    objs[objsCount].UsedSymbol = symbolNum(yytext);

    setObj(LABELED, p, 3, 0xc3, 0x00, 0x00);
}

void org(int address)
{
    char *p = malloc((size_t)25);
    snprintf(p, 24, "ORG     0x%04X           ", address);

    relocAddress = address;

    setObj(ORIGIN, p, 0);
}

void deflabel(void)
{
    int symNum;
    char *p = malloc((size_t)25);
    snprintf(p, 24, "%s                       ", yytext);

    trace("Bison LABEL definition: (%p)%s\n", yytext, yytext);
    symNum = symbolNum(yytext);
    trace("symnum = %d, address = %d\n", symNum, relocAddress + codeBytes);
    symbols[symNum].Value = relocAddress + codeBytes;   // ORG �Ή��ς�
    symbols[symNum].isDefined = 1;                      // ISDEFINED_FALSE �ȊO�Ȃ� TRUE
    setObj(LABELDEF, p, 0);
}

int symbolNum(char *symbol)
{
    int i;
    size_t sl;
    char *p;

    sl = strlen(symbol);
    if (symbol[sl - 1] == ':') sl--;    // ���x����`�̏ꍇ�́A":"��1�o�C�g������

    trace("finding symbol (%s)...\n", symbol);
    for (i = 0; i < symbolsCount; i++) {
        // �o�^�ς݂̃V���{�����Ȃ�����������
        trace("comparing with (%s)\n", symbols[i].Name);
        if (strncmp(symbol, symbols[i].Name, sl) == 0) {
            // ��v����
            if (sl == strlen(symbols[i].Name)) return i;	// ���������炻�̃V���{���ԍ���Ԃ�	
        }
    }

    // ������Ȃ������ꍇ�͐V���ɃV���{���\�ɒǉ�����
    // �V���{���o�����̔ԍ��m�F�p�ƁA�V���{���̍Ē�`(�G���[)���o�p
    p = malloc(sl + 1);		// NULL��������1�o�C�g�𑫂�
    strncpy(p, symbol, sl);	// �̈�̖����ɂ�strncpy()�ɂ��NULL�����������
    *(p + sl) = '\0';

    symbols[symbolsCount].isDefined = ISDEFINED_FALSE;
    symbols[symbolsCount++].Name = p;
    //symbolTable[symbols++] = yytext;  // ����͂������_���ȃp�^�[���̂��
    trace("symbol (%s) added.\n", p);
	
    return i;
}

int pass2(void)
{
    // �ŏI�R�[�h������
    int i, j, k, codeCount = 0;
    unsigned int address;       //�����_�ł� ORG �[�����߂͈�x�����g�p���Ă͂����Ȃ��d�l

    for (i = 0; i < objsCount; i++) {
        fprintf(fplst, "%04X:", relocAddress + codeCount);
        switch (objs[i].Type) {
        case LABELED:           // 0x??, 0xLL, 0xHH�^�C�v
            if (symbols[objs[i].UsedSymbol].isDefined == ISDEFINED_FALSE) {
                //yyerror()?
                printf("Undefined Symbol: \"%s\".\n", symbols[objs[i].UsedSymbol].Name);
                fprintf(fplst, "Undefined Symbol: \"%s\".\n", symbols[objs[i].UsedSymbol].Name);
            } else {
                address = symbols[objs[i].UsedSymbol].Value;
                fprintf(fplst, "%02X %02X %02X ", objs[i].code[0], address & 0x00ff, (address >> 8) & 0x00ff);			
                fprintf(fplst, "          %s;comments\n", objs[i].ListingString);  // �j�[���j�b�N�o��
                fputc(objs[i].code[0], fpbin);
                fputc(address & 0x00ff, fpbin);
                fputc((address >> 8) & 0x00ff, fpbin);
            }
            break;
        case LABELDEF:
            fprintf(fplst, "%s\n", objs[i].ListingString);             // �j�[���j�b�N�o��
            break;
        default:
            for (j = 0; j < objs[i].Size; j++) {
                fprintf(fplst, "%02X ", objs[i].code[j]);
                fputc(objs[i].code[j], fpbin);
            }
            for (k = 0; k < 24 - 5 - (j * 3); k++) fputc(' ', fplst);
            fprintf(fplst, "%s;comments\n", objs[i].ListingString);    // �j�[���j�b�N�o��
            break;
        }
        codeCount += objs[i].Size;
    }

    return 0;
}

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

void trace(const char *format, ...)
{
    va_list argp;

    va_start(argp, format);
    //vfprintf(stderr, format, argp);
}
