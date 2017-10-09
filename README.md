# artcopy
Artifacts copy tool

Allow copy files by rules. Kind of replacement for xcopy

# Rules

General syntax

`[(+|-):]pattern [=> destination]`

`+:` - copy files, matched by pattern to destination (default)

`-:` - exclude files, matched by pattern to destination

If file matched several times by different rules, and destination is different, then file will be copied to all destinations:

`
file1.txt => File1
file*.txt => FileAll
`

If file matched by exclude rule, then all previous matches ignored. But file can be again include after exclude rule:

`
+:file1.txt => will-not-be-copied-here
+:file*.txt => will-not-be-copied-here-also
-:*.1.txt
+:*.txt => will-be-copied
`

Pattern can contains wildcards: `?`, `*`, `**`

where `?` match against any one symbol in file/directory name

`*` match any number of symbols in file/directory name (but will not match across path separator)

`**` match any number of symbols in file/directory path. It match against path separator

Examples:

`file?.txt` - match `file1.txt`, `file2.txt`

`dir/file*.txt` - match `dir/file.txt`, `dir/file1.txt`, `dir/file12.txt` but not `dir/file/1.txt`

`dir/**.txt` - match `dir/file1.txt`, `dir/subdir/file2.txt` and so on


Matched files copied to destination with keeping directory structure starting with first wildcard

`dir/*.txt => txt` will copy all txt files to `txt` directory immediately

`dir*/*.txt => txt` will copy all txt files to `txt/dir` directory

`dir/** => dst` will copy all files and directories under `dir` to `dst` and keep structure