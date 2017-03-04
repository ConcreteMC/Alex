# Makefile for TheoraPlay#
# Written by Ethan "flibitijibibo" Lee

build: clean
	mkdir bin
	cp TheoraPlay-CS.dll.config bin
	dmcs /unsafe -debug -out:bin/TheoraPlay-CS.dll -target:library TheoraPlay.cs

clean:
	rm -rf bin
