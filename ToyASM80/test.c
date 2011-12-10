#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

int mystrncasecmp(const char *s1, const char *s2, size_t n);

char filename[256];

struct obj {
	int x;
	int y;
};

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

int main(int argc, char *argv[])
{
	printf("%d\n", argc);

	if (argc == 2) {
		printf("argv[1] = %s\n", argv[1]);
		if (mystrncasecmp(argv[1] + strlen(argv[1]) - 4, ".Z80", (size_t)4) != 0) {
			printf("Usage: %s assembly_source.z80\n", argv[0]);
			exit(EXIT_FAILURE);
		} else {
			strncpy(filename, argv[1], strlen(argv[1]) - (size_t)4);
			printf("Z80 Source: %s.Z80\n", filename);
		}
	}
	exit(EXIT_SUCCESS);
}
