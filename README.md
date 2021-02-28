![alt text](https://github.com/rambotech/BOG.SwissArmyKnife/blob/master/src/Assets/multitool.png "The most useful collection of miscellaneous tools for .NET Standard 2.1!")

# BOG.SwissArmyKnife

[![Build status](https://api.travis-ci.com/rambotech/BOG.SwissArmyKnife.svg?branch=master)](https://travis-ci.com/rambotech/BOG.SwissArmyKnife)
[![Nuget](https://img.shields.io/nuget/v/BOG.SwissArmyKnife)](https://www.nuget.org/packages/BOG.SwissArmyKnife/)

This is a .NET Standard port of BOG.Framework, which is deprecated.

The classes are:

### Accordion / AccordionItem&lt;T>
  Provides tracking of a number (ordinal) sequence by an in-progress list, from which items are retrieved and later marked as retry or complete.  Includes time-out to
  reissue work after a period of time.  Allows large sequences of numbers (even billions and higher) to be processed, with the accordion class providing the tracking.

See the NUnit project for examples.

### AssemblyInfo
  Provides the Binary file executing (in .NET core it's always the .DLL not the .EXE'), version and build date.

### BabbleOn
  Allows real-time monitoring of a service or other headless operation from any Telnet client.
  
### CipherUtility
  A class for enciphering / deciphering.  Allows client to override the SymmetricAlgorithm class used for cipher method.

### DetailedException
  Enables formatting of an exception with:
  - Optional header/footer content:
  - One of three level of details for display or logging: User, Machine or Enterprise.
  - Optional masking of user and password, or only password, in URLs and connection strings.
  
### Formatting
  Provides methods for 
  - Squashing a number into Kilo, Mega, ..., Yota (e.g. 2,325,078 becomes 2.33M).
  - Writing out a dollar amount into a phrase as it would appear on a paper check (e.g. FIVE HUNDRED AND 50/100).

### Fuse
  Similar to an electrical fuse: records frequency and/or volume, and reports when thresholds are exceeded.

### Hasher
  Various hashing methods chosen by an enumerator.

### Iteration
  A class used to manage very large and deep argument sets, where the extropolation can result in millions
  or even billions of combinations. Designed to support multi-level nested looping, where the values at each level 
  for an iteration can be derived by passing in a zero-based index--and vice-versa.  Uses the IterationItem class 
  for an iteration level.

### Logger:
  Provides a method of logging to a file, using a timestamped filename and rollover to a new file when 
  either a file size threshold is met, or when the maximum elapsed time has exceeded a threshold.

### MathEx:
  Holder for derived mathematical methods.
  
### MemoryList
  A List<> type class, which can enforce unique entries (even if an item is removed), and allows the client
  to specify the order in which items are recalled: FIFO, LIFO or random. Ideal for workflow processes,
  which may receive many requests for the same item, but restrict their action to once per item.
  
### PasswordHash
  A password hashing algorithm encapsulated in a class.  Fairly secure, and not overly complex.
  
### Scrape
  Contains methods for harvesting content with regular expressions.  See the demo form to understand its
  capability.  Also contains classes MemoryItem and MemoryList, which extend the capabilties of the core
  generic List<> class.

### SecureGram
  Creates an encrypted container which can be sent over a TCP/UDP connection between apps.  Supports automatic
  internal compression for large content.

### SerializableDictionary
  A replacement for the standard Dictionary<K,V> class, which does not allow serialization.
  
### SerializerXML
  A static class containing methods to serialize and deserialize XML between objects, strings and files.
  Also contains methods to convert to/from a secure transport container.
  
### SerializerJSON
  A static class containing methods to serialize and deserialize JSON between objects, strings and files.
  Also contains methods to convert to/from a secure transport container.
  
### SettingsDictionary
  A kind of Dictionary<string,object> with internal methods to load, save and merge.  Originally designed
  as a class to store user settings for a desktop application, outside of the app.config architecture.
  
### StringEx
  Some useful string methods:
  - Search and replace with wildcards
  - Check against flexible wildcard pattern without using regular expressions.  E.g. "The * fox * jump??"
  - Currency parsing
  - Hex to/from methods.
  - Filtering (include and exclude)
  - Placeholder replacement for allowing Environment, Common Folders and Custom Dictionary replacement in 
    a string, i.e. directory path, etc.

### Url
  A Url parser which is designed to validate listener url's for an HttpListener object, is less restrictive on
  host portions, and can rebuild a URL after parsing it to remove unnecessary URL encoding.
