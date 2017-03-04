# Makefile for MojoShader#
# Written by Ethan "flibitijibibo" Lee

build: clean
	mkdir bin
	cp MojoShader-CS.dll.config bin
	dmcs /unsafe -debug -out:bin/MojoShader-CS.dll -target:library MojoShader.cs

clean:
	rm -rf bin
