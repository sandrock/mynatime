---
title: Tips and tricks
parent: Documentation
nav_order: 6
---

# Tips and tricks

Daily use
---------------------

### Undoing an operation before commiting

Sometimes you can make a mistake while typing commands. The app does not have undo capabilities (for now) but you can change things the hard way before commiting. 

Open the JSON file of the profile. Search for the element `Transaction`. It contains the pending actions. Each action type has a dedicated data representation. You might be able to tinker with the data to have the desired effect. You can also remove the transaction item to get rid of the problematic action. Use the `m status` command to verify your file is correct. 


### Looking at past actions

Open the JSON file of the profile. Search for the element `Commits`. It contains the past actions, grouped by commits.


### Activity category alias #15

When tracking activity, some categories are very long to type (and don't single match).

```bash
m act start interne
Too many possibilities for category "interne": "Company-interne", "Customer2-interne", "Meeting-interne"
```

You can create an alias:

```bash
m act alias Company-interne interne
```

And use the alias for activity adding/tracking:

```bash
m act start interne
```


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


### Profile options

Profile files are JSON and support additional options you can set manually:

| Option | Default | Description |
|--------|---------|-------------|
| `ConfirmLocalSave` | false | Prompt before saving the profile file locally. Recommended during development to avoid accidental writes. |
| `ConfirmServiceSave` | false | Prompt before sending each transaction item to the service. Useful to review items one by one before they are committed. |

These options are also offered interactively when creating a profile with `pro add`.
