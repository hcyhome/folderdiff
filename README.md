# folderdiff
Compare two folders and output the difference in organized format. The file comparison use MD5 hash, and empty files will be ignored.

[Usage]<br/>
folderdiff.exe sourcefolder destinationfolder [outputpath]

[Output]<br/>
add:[destinationfolder]\file1 <br/>
edit:[destinationfolder]\file2 <br/>
delete:[sourcefolder]\file3 <br/>
...
