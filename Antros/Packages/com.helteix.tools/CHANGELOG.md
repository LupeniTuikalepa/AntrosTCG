
# Change Log
## [1.6.2] - 2026-05-06

### Added
### Changed
 - Changed PhaseCompletionResult's "SetResult" method to return a bool if the result was successfully assigned.
### Fixed

## [1.6.0] - 2026-01-06

### Added
 - Added Phase System.
 - Added UILists, UIMultiColumnLists and various UI's tools for common boilerplate code for UI.
### Changed
### Fixed


## [1.3.6] - 2025-10-15
### Added
### Changed
 
### Fixed
 - Fixed error when no `TypeRefCollection` asset was found at the start of a new build.
 
## [1.3.7] - 2025-10-15

### Added
- Option to trigger a manual building of the `TypeRefCollection` asset with the **Tools/Helteix/Setup TypeMapping Collection**  menu.
- Added *Entries* property to the `TypeRefCollection` class. Use it to access all mapped types in the project.

### Changed
- Changed accessibility of the *entries* property inside the `TypeRefCollection` class from **public** to **private**.

### Fixed
 - Fixed an error (again) when no TypeRefCollection asset was found at the start of a new build.