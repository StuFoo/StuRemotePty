
#include <stdio.h>
#include <unistd.h>
#include <getopt.h>
#include <signal.h>
#include <fcntl.h>
#include <termios.h>
#include <linux/kd.h>
#include <linux/keyboard.h>
#include <sys/ioctl.h>
#include <stdlib.h>
// #include "getfd.h"
// #include "nls.h"

int tmp; /* for debugging */
int fd;
int oldkbmode;
struct termios old;

int InitRead()
{
	struct termios new;
	if (tcgetattr(0, &old) == -1)
		return -1;
	new = old;
	new.c_lflag &= ~(ICANON | ISIG);
	new.c_lflag |= (ECHO);
	new.c_iflag = 0;
	new.c_cc[VMIN] = 1;
	new.c_cc[VTIME] = 0;
	return tcsetattr(fd, TCSAFLUSH, &new);
}

int EndRead()
{
	return tcsetattr(fd, 0, &old);
}

char ReadChar()
{
	unsigned char buf[18];
	while (1)
	{
		int n = read(fd, buf, 1);
		if (n == 1)
			return buf[0];
	}

	return -1;
}

#ifdef Test
int main(int argc, char *argv[])
{
	InitRead();
	while (1)
	{
		char ch = Read();
		if (ch == -1)
			break;
		printf(" \t%3d 0%03o 0x%02x\n", ch, ch, ch);

		if (ch == 04)
			break;
	}

	exit(0);
}
#endif