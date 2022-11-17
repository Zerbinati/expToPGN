# expToPGN
Tool to make a PGN file from an "eman-like" EXP file<p>

Prerequisites :<br>
rename BUREAU.ini to YOUR-COMPUTER-NAME.ini<br>
set moteurEXP to path_to_your_eman_engine.exe<br>
set fichierEXP to path_to_your_experience_file.exp<br>

There are 2 ways to use this tool :<br>
- either run this command : expToPGN.exe path_to_your_opening.pgn<br>
- either run expToPGN.exe then enter your limits and your opening<p>

# Most common scenario
1°) enter your settings :<br>
![1 enter your settings](https://github.com/chris13300/expToPGN/blob/main/expToPGN/bin/Debug/1.%20set%20your%20limits.jpg)<p>

2°) export in progress :<br>
![2 exporting](https://github.com/chris13300/expToPGN/blob/main/expToPGN/bin/Debug/2.%20export.jpg)<p>

How it works ?<p>
The program will search for all lines containing a move whose depth is less than the max depth. The game's length must not exceed the max plies. It caps at [100k positions](https://github.com/chris13300/expToPGN/blob/main/expToPGN/modMain.vb#L159).<br>
![under_D40](https://github.com/chris13300/expToPGN/blob/main/expToPGN/bin/Debug/under_D40.jpg)<br>
