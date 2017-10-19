.include "include1.asm"
.include "include2.asm"

start:
	mov SP, 0x420
	mov SS, 0x100

	mov AR, [load_stack]
	push AR
	mov DR, 17
	mov BR, [string_loaded]
	sub BR, 1
	jmp [display]
	
load_stack:
	mov AR, [done_msg]
	push AR
	mov DR, 20
	mov BR, [string_stack]
	sub BR, 1
	jmp [display]
	
blankout:
	mov AR, [cmd]
	push AR
	mov DR, 2
	mov BR, [string_nl]
	sub BR, 1
	jmp [display]
	
cmd:
	mov AR, [halt]
	push AR
	mov DR, 8
	mov BR, [string_06]
	sub BR, 1
	jmp [display]
	
done_msg:
	mov AR, [blankout]
	push AR
	mov DR, 6
	mov BR, [string_done]
	sub BR, 1
	jmp [display]
	
halt:
	hlt