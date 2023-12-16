A simple program that can add palettes from HPal files (palette files used in blazblue games) to FBPal files (palette files used in French Bread games, the ones with .pal)

Command line program (so it requires knowing how to make batch files or knowing what cd is) so it's as simple as putting the file names in then the palette index.

Usage is "HPalToFBPal.exe [hpalFile] [fbPalFile] [fbPalIndex] [optionals]"
	hpalFile: Filepath to the HPal file
	fbPalFile: Filepath to the FBPal file
	fbPalIndex: What palette in the FBPal to replace. 0 indexed.
	optionals: optional commands
		-reverse: Reverses the palette colors.
		-direction D: D sets what direction. 0 for both, 1 for right, 2 for left.
		-3ds: Use if the hpal file comes from a 3ds arcsys game.
		-palSize S: S sets the size of the palette. Max of 256