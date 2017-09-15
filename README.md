# folderdiff
Compare two folders and output the difference in organized format. The file comparison use MD5 hash, and empty files will be ignored.

Usage:

folderdiff.exe sourcefolder destinationfolder [outputpath]

[Output]
add:[destinationfolder]\file1
edit:[destinationfolder]\file2
delete:[sourcefolder]\file3
...
