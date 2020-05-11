BOG.SwissArmyKnife

This is a collection of utility classes I have made over the years to handle 
various tasks which weren't native to the Microsoft assemblies.

1.5.10 -- 05/10/2020
- URI parsing: Adds default ports for ftp, sftp, ftps and ftpes, and related tests.

1.5.9 -- 05/07/2020
- Adds a (generic) Payload to an accordion item, and switched to AccordionItem<T> for casting payload.

1.5.8 -- 05/06/2020
- Adds a State to an accordion item, for the client to track Success, Failed, InProgress or Pending.

1.5.7 -- 02/15/2020 -- Maintenance Update
- Add build process to Travis CI
- Update Nuget packages

1.5.6 -- 01/03/2020 -- Maintenance Update
- Update frameworks to .NET Standard 2.1 and .NET Core 3.1
- Update Nuget packages

1.5.5 -- 11/22/2019
- AccordionItem add timeoutCount property and tests.

1.5.4 -- 11/18/2019
- Accordion/AccordionItem classes finalized and tested.

1.5.3 -- 10/22/2019
- Debut of Accordion class for tracking a large sequence of ordinals.

1.5.2 -- 07/15/2019
- Nuget package updates
- Update to PackageLicenseUrl to PackageLicenseExpression

1.5.1 -- 05/09/2019
- Add class AssemblyVersion for supporting binary signatures in output/logging.

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

