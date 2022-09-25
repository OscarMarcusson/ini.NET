# ini.NET
A simple .NET ini-parser for parsing .ini files from disk, streams or strings into strongly typed objects or to dictionaries. It is written in a single C# file for the sake of easy portability; there is no need for linking DLLs, just copy and paste the source file and call it a day.


## Using the INI dictionary
```C#
// Load a dictionary
var dictionary = System.IO.IniDictionary.FromFile("C:/some/directory/config.ini");

// Read from the dictionary
var someValue = dictionary.GetField("some-value");
var someInteger = dictionary.GetField<int>("some-integer", defaultValue: -1);
var someBool = dictionary.GetField<bool>("some-bool", defaultValue: false);

// Get a section like [info]
var section = dictionary.GetSection("info");
var someSectionValue = section.GetField("some-value");
```




## Example file
This is the example file used in the examples above:
```ini
; A comment that will be ignored
some-value = Hello World
some-integer = 123
some-bool = true
# some-other-value-to-ignore = ABC123

[info]
some-value = Hello Word from the info section!
```
