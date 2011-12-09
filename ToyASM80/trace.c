#include <stdio.h>
#include <stdarg.h>

void debug(const char *format, ...)
{
    #ifdef DEBUG 
    va_list argp;

    va_start(argp, format);
    vfprintf(stderr, format, argp);
    #endif
}
