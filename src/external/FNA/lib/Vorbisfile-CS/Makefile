# Makefile for Vorbisfile#
# Written by Ethan "flibitijibibo" Lee

build: clean
	mkdir bin
	cp Vorbisfile-CS.dll.config bin
	dmcs /unsafe -debug -out:bin/Vorbisfile-CS.dll -target:library Vorbisfile.cs

clean:
	rm -rf bin
