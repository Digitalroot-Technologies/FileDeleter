FileDeleter
===========

Handles cleaning up of temp files.  
Reads path data from a SQLite database and age limit on files.  
All files that match are deleted.  
Run without any parameters to have it delete files. (I run it with a task scheduler.)   

===========
    Usage: FileDeleter [options ...] [parameters]
  
    Options:
        -a or -add - Add a new path
                FileDeleter -a path age

        -d or -del - Delete a path
                FileDeleter -d pathid

        -l or -list - List all paths from database.

        -p or -path - Delete files from the path older then age. Do not add to database.
                FileDeleter -p path age

        -h or -help - This help list.
