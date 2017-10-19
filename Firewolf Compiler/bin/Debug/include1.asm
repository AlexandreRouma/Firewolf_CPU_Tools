display:
	mov AR, $[BR]
	out
	add BR, 0x01
	sub DR, 0x01
	mov AR, 0
    cmp DR, AR
    jne [display]
	pop AR
	jmp [AR]