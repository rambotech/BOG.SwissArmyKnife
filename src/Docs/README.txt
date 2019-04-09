BOG.SwissArmyKnife

This is a collection of utility classes I have made over the years to handle 
various tasks which weren't native to the Microsoft assemblies.

1.5.0 -- 04/09/2019
- Reduce JSON serialization fingerprint
  - Refactor the Iteration/ItemIteration classes to not enumerate numeric values in string lists; instead, calculate the value from the index.
  - Refactor the classes for proper serialization/deserialization (deserialization had issues)
  - Fix namespace in SettingsDictionaryTest.cs
  - Upgrade nuget packages.

1.4.2 -- 12/28/.2018
- Iteration: correct bug/cleanup in AddNumberRange/AddNumberSequence methods.

1.4.1 -- 12/26/2018
- Iteration: change double to decimal type for better accuracy.

1.4.0 -- 12/20/2018
- Adds new StringEx method ContainsWildcardPattern().  Supports * (multiple) and ? (single) wildcard in any position.  Set StringExTests.cs for examples.

