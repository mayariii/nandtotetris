// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed.
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// infinite loop listening to keyboard input

//if key = pressed (if KBD !=0), turn all pixels in memory map blackens
//if no key = pressed (if KBD = 0), turn all pixels in memory map to white

// SCREEN SET UP / INITIALISATION
// set a pointer for screen base address value (starting position)
@SCREEN
D=A

// initialise current position as screen start position
@currentPosition
M=D

// create a variable for the screen size so incrementor knows when to stop
@8192 // (value for maxScreenSize)
D=A
@SCREEN
D=D+A
@maxScreenSize
M=D

// MAIN LOOP
(LOOP)
// get value stored in keyboard
@KBD
D=M

// if KBD = 0 (not pressed) goto CLEAR
@CLEAR
D;JEQ

// else (if KBD != 0 (pressed)) goto FILL
@FILL
0;JMP


(FILL) // sets screen to black
@currentPosition
A=M
M=-1 // turns the 16 bits at the register at currentPosition all to -1 (black)
@INC // jump to incrementor for currentPosition
0;JMP


(CLEAR) // sets screen to white
@currentPosition
A=M
M=0 // turns the 16 bits at the register at currentPosition all to 0 (white)
@INC // jump to incrementor for currentPosition
0;JMP

(INC) // increments currentPosition by 1
@currentPosition
D=M+1
M=D

@maxScreenSize
D=D-M // computes currentPosition - maxScreenSize
@LOOP
D;JNE // if not equal

// if currentPosition is at end point (= maxScreenSize), reset the screen + currentPosition to base value
@SCREEN
D=A
@currentPosition
M=D
@LOOP // unconditional loop to start
0;JMP
