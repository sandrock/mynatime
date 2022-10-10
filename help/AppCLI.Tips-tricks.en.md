
[Back: Index](AppCLI.0Index.en.md) - Back: [Add an activity](AppCLI.Add-activity.en.md) - Next: nothing here yet

---

Tips and tricks - Mynatime CLI app documentation
====================================

Daily use
---------------------

### Undoing an operation before commiting

Open the JSON file of the profile. Search for the element `Transaction`. It contains the pending actions. Each action type has a dedicated data representation. You might be able to tinker with the data to have the desired effect. You can also remove the transaction item to get rid of the problematic action. 


### Looking at past actions

Open the JSON file of the profile. Search for the element `Commits`. It contains the past actions.


Developers
---------------------

### Name profiles for easier selection

The profiles are stored in `~/.config/mynatime/`. The file name is generated using the date of authentication. You can rename the files to facilitate selection. Example:

- rename `profile.20220918T100535632Z.json` as `kevin.json`
  - and use argument `-pkevin`
- rename `profile.20220924T041201230Z.json` as `test.json`
  - and use argument `-ptest`


### Setting a default profile

When you use multiple profiles, typically during development, you may wish to set a default profile. 

Open the JSON file of the profile to use by default and add a JSON entry `"IsDefault": true,` in the root object:

```json
{
  "__manifest": "MynatimeProfile",
  "IsDefault": true,
  "DateCreated": "2022-09-18T10:05:35.6283817Z",
  ...
}
```

---

[Back: Index](AppCLI.0Index.en.md) - Back: [Add an activity](AppCLI.Add-activity.en.md) - Next: nothing here yet
