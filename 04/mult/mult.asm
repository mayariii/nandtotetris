// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)

@R1
D=M     //copy value from R1 into D

@i
M=D     // set the value of i to value of D from R1

@result
M=0     // set result = 0

(LOOP)
@i
D=M     // copy the current value of i into D

@END
D;JEQ   // breakpoint - if i = 0 go to END otherwise execute next step

@R0
D=M     //copy the value from R0 into D

@result
M=D+M   // compute (value in D from R0 + current value in result) and set as new value of M

@i
M=M-1   // (i--) compute the current value in i - 1 and set that as new value of M

@LOOP
0;JMP   // unconditional loop - jump to LOOP

(END)
@result
D=M     // copy the value of result into D
@R2
M=D     // set the value of R2 to value of D from result

@END
0;JMP   // infinite loop to end to stop malicious code
