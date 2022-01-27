


libShowKey.so:showkey.c
	$(CC) $^ -fPIC -shared -o $@

test.out:showkey.c
	$(CC) $^ -o $@ -DTest

all:test.out libShowKey.so

clean:
	rm -rf *.out *.so