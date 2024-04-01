
Checklist before merging things
------------------------------------------

- [ ] is the build OK? (check CI)
- [ ] are existing unit tests all green (check CI)
- [ ] are there new unit tests covering (at least a bit) the new feature?
- [ ] are there enough code comments? at least a class description
- [ ] [CLI app] does the `--help` argument of the CLI app contains information about the new feature?
- [ ] [CLI app] is the new feature explained in the `help` pages?
- [ ] is the new feature tested with a user with no privileges?
- [ ] is the new feature tested with a user with privileges?
