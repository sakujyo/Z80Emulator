/*
 * ���C���v���O����.
 *
 * �����͂��Ăяo���A�������ςނƏI������. 
 */

//char **symbols;
char *symbolTable[SYMTABSIZE];          // �V���{��������ւ̃|�C���^
unsigned int symbolValue[SYMTABSIZE];   // �V���{���Ɋ��蓖�Ă�ꂽ��̓I�ȃA�h���X
int symbols;                            // �o�^�ς݃V���{����

unsigned char codeArray[CODESIZE];      // ���ԃI�u�W�F�N�g�̃o�C�g�\���̔z��
int codeCount;                          // �o�͍ς݂̒��ԃI�u�W�F�N�g�̐�
int codeBytes;                          // �o�͍ς݂̒��ԃI�u�W�F�N�g�̃o�C�g��
int sizeArray[CODESIZE];                // ���ԃI�u�W�F�N�g�̃}�V���R�[�h�E�T�C�Y
int typeArray[CODESIZE];                // ���ԃI�u�W�F�N�g�̃I�u�W�F�N�g�E�^�C�v
unsigned int usedSymbol[CODESIZE];      // ���ԃI�u�W�F�N�g�Ŏg�p�����V���{��
char *listArray[CODESIZE];              // ���X�e�B���O�o�͗p�̔z��
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

void putobj(int code)
{
    //debug("object code: {%02x}\n", code);
    codeArray[codeBytes++] = (unsigned char)code;
}

void puttype(int codeSize, int type)
{
    debug("object type: {%02x}\n", type);   // codeBytes�X�V�܂łɂ��ׂĂ̎d�����I���Ă���������
    sizeArray[codeCount] = codeSize;		// codeCount �Ԗڂ̒��ԃI�u�W�F�N�g�̃T�C�Y
    typeArray[codeCount] = type;			// �o�͂��钆�ԃI�u�W�F�N�g�̃^�C�v
    codeCount++;							// �o�͍ς݂̒��ԃI�u�W�F�N�g�̐��̃J�E���g�A�b�v
}

void increg8(int reg8)
{
    char *p;
    debug("reg8: %d\n", reg8);
    p = malloc((size_t)25);
    snprintf(p, 24, "INC     %s               ", reg8name[reg8]);
    listArray[codeCount] = p;
    
    puttype(1, NOLABEL);
    putobj(0x04 | (reg8 << 3));
}

void out(int port)
{
    char *p;
    debug("out port: %d\n", port);
    p = malloc((size_t)25);
    snprintf(p, 24, "OUT     0x%02X            ", port);
    listArray[codeCount] = p;
    
    puttype(2, NOLABEL);
    putobj(0xd3);
    putobj(port & 0x00ff);
}

void ldr8r8(int dest, int src)
{
    char *p;
    debug("ld: (reg %d) <- (reg %d)\n", dest, src);
    p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, %s            ", reg8name[dest], reg8name[src]);
    listArray[codeCount] = p;
    puttype(1, NOLABEL);
    putobj(0x40 | (dest << 3) | src);
}

void ldregim8(int dest, int immediate)
{
    char *p;
    debug("ld: (reg %d) <- (immediate %d)\n", dest, immediate);
    p = malloc((size_t)25);
    snprintf(p, 24, "LD      %s, 0x%02X         ", reg8name[dest], immediate);
    listArray[codeCount] = p;
    puttype(2, NOLABEL);
    putobj(0x06 | (dest << 3));
    putobj(immediate);
}

void jp(int absoluteAddress)
{
    char *p;

    //��΃A�h���X�W�����v�Ȃ񂾂��炱�̉���1�s�͂���Ȃ�
    //absoluteAddress += relocAddress;        // ORG �[�����ߑΉ�

    p = malloc((size_t)25);
    snprintf(p, 24, "JP      0x%04X            ", absoluteAddress);
    listArray[codeCount] = p;
    puttype(3, NOLABEL);
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
    puttype(3, LABELED);
    putobj(0xc3);
    putobj(0x00);
    putobj(0x00);
}

void org(int address)
{
    char *p;

    p = malloc((size_t)25);
    snprintf(p, 24, "ORG     0x%04X           ", address);
    listArray[codeCount] = p;
    relocAddress = address;
    puttype(0, ORIGIN);    
}

void deflabel(void)
{
    int symNum;
    char *p;

    trace("Bison LABEL definition: (%p)%s\n", yytext, yytext);
    symNum = symbolNum(yytext);
    trace("symnum = %d, address = %d\n", symNum, relocAddress + codeBytes);
    symbolValue[symNum] = relocAddress + codeBytes;    // ORG �Ή�

    p = malloc((size_t)25);
    snprintf(p, 24, "%s                       ", yytext);
    listArray[codeCount] = p;
    puttype(0, LABELDEF);
}

int symbolNum(char *symbol)
{
    int i;
    size_t sl;
    char *p;

    sl = strlen(symbol);
    if (symbol[sl - 1] == ':') sl--;    // ���x����`�̏ꍇ�́A":"��1�o�C�g������

    trace("finding symbol (%s)...\n", symbol);
    for (i = 0; i < symbols; i++) {
        // �o�^�ς݂̃V���{�����Ȃ�����������
        trace("comparing with (%s)\n", symbolTable[i]);
        if (strncmp(symbol, symbolTable[i], sl) == 0) {
            // ��v����
            if (sl == strlen(symbolTable[i])) return i;	    // ���������炻�̃V���{���ԍ���Ԃ�	
        }
    }

    // ������Ȃ������ꍇ�͐V���ɃV���{���\�ɒǉ�����
    // �V���{���o�����̔ԍ��m�F�p�ƁA�V���{���̍Ē�`(�G���[)���o�p
    p = malloc(sl + 1);		// NULL��������1�o�C�g�𑫂�
    strncpy(p, symbol, sl);	// �̈�̖����ɂ�strncpy()�ɂ��NULL�����������
    *(p + sl) = '\0';

    symbolValue[symbols] = UNDEFINED_SYMBOL;
    symbolTable[symbols++] = p;
    //symbolTable[symbols++] = yytext;  // ����͂������_���ȃp�^�[���̂��
    trace("symbol (%s) added.\n", p);
	
    return i;
}

int pass2(void)
{
    // �ŏI�R�[�h������
    int i, j, k;
    unsigned int address;
    unsigned char *p;
    int *t;                 // ���ԃI�u�W�F�N�g�̃^�C�v���g���o�[�X����|�C���^

    char lcode[25];         // ���X�e�B���O����
    char lmnemo[25];        // ���X�e�B���O������

    p = codeArray;
    t = typeArray;
    for (i = 0; i < codeCount; i++) {
        //fprintf(stdout, "%04X:", (p - codeArray));
        fprintf(stdout, "%04X:", (relocAddress + (p - codeArray)));
        switch (*(t++)) {
        case LABELED:           // 0x??, 0xLL, 0xHH�^�C�v
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
        case LABELDEF:
            fprintf(stdout, "%s\n", listArray[i]);
            break;
        default:
            for (j = 0; j < sizeArray[i]; j++) {
                fprintf(stdout, "%02X ", (int)*p);
                p++;
            }
            for (k = 0; k < 24 - 5 - (j * 3); k++) fputc(' ', stdout);
            // �j�[���j�b�N�o��
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
