BOG.SwissArmyKnife

This is a collection of utility classes I have made over the years to handle 
various tasks which weren't native to the Microsoft assemblies.

FUTURE -- 2.0.0 -- No roadmap
- Refactoring crypto and others to replace obsolete items.

1.10.0 -- 04/21/2024
- Add an optional magic string which a TCP client must send, before BabbleOn begins feeding traffic to the client.
- Add new class CommandArgParser for turning --key VALUE or -k VALUE style arguments into Dictionary<string, List<string>>

1.9.1a -- 12/01/2024
- Updates wip interim

1.9.0 -- 04/03/2024
- .NET 8.0
- Add name and ToJson string output to AssemblyVersion

1.8.0 -- 08/23/2023
- .NET 7.0

1.7.15 -- 04/02/2023
- Github Actions: add nuget update to actions

1.7.14 -- 03/18/2023
- Drop Travis-CI for Github Actions
- Update Nuget packages

1.7.13 -- 12/28/2022
- revert to dotnet core 3
- Update Nuget packages

1.7.12 -- 12/12/2022
- .NET Core to .NET 7 for all projects

1.7.11 -- 12/11/2022
- Maintenance release: package updates due to security flaw

1.7.10 -- 12/28/2021
- Maintenance release: package updates

1.7.9 -- 12/28/2021
- Backslide to .NET Standard 2.0 for compatibility with .NET 6.0 and .NET Framework 4.8

1.7.8 -- 04/25/2021
- .NET Standard 2.0 -> 2.1 (https://github.com/advisories/GHSA-7jgj-8wvc-jh57)

1.7.7 -- 02/27/2021
- Misc updates

1.7.6 -- 02/27/2021
- Nuget package updates; adjust tests for Windows/Linux runtime environment.

1.7.5 -- 12/30/2020
- Nuget package updates; travis config update.

1.7.4 -- 09/18/2020
- Drop MegaAccordion + assoc.  Too many issues with the design.

1.7.3 -- 08/18/2020
- Fix JSON attribute names.

1.7.2 -- 08/14/2020
- Minor fixes to ResetMegaAccordion() methods for levels.

1.7.1 -- 07/15/2020
- Minor fixes to MegaAccordion / MegaAccordionItem classes to expand Accordion / AccordionItem
  for combinations in excess of 2^64 items. Accordion and AccordionItem still maintained.

1.7.0 -- 07/14/2020
- Introduction of MegaAccordion / MegaAccordionItem classes to expand Accordion / AccordionItem
  for combinations in excess of 2^64 items. Accordion and AccordionItem still maintained.

1.6.3 -- 05/27/2020
- Fix serialization issue in IterationItems

1.6.2 -- 05/26/2020
- TotalIterationItems removed as a property.
- TotalIterationItems calculated at first get action.

1.6.1 -- 05/25/2020
- Add GetItemPayload method.
- Correct IssuePayload property name in JSON.

1.6.0 -- 05/14/2020
- Simplify the AccordionItem: consumer tracks fails and attempts.

1.5.11 -- 05/11/2020
- Reduce target framework to .NET Standard 2.0 for better compatibility.

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

