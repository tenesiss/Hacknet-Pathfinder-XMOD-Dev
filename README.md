# Docs

## Ports and Cracks
`Port 188: Police Port` -> `#POLICE_CRACK#`

`Port 591: Shading Controller` -> `#SHADING_CRACK#`

`Port 3500: Socket Connect` -> `#SOCKET_CRACK#`

`Port 500: TX Port` -> `#TX_CRACK#`

`Port 211: Transfer` -> `#TRANSFER_CRACK#`

`Port 9418: Version Control` -> `#VERSION_CONTROL_CRACK#`

`Port 3659: eOS Device Port` -> `#EOS_CRACK#`

`Port 25: SMTP (Fast Crack)` -> `#SMTP_FAST_CRACK#`

## Executables
### En Break
- Breakes EnSec giving the password
- Ram: 151 mb
- Time: 10 seconds
- Requires admin: No
### Encypher
- Encrypts files under a password or not (DEC protocol)
- Ram: 250 mb
- Time: 10 seconds
- Requires admin: Yes (To save the encrypted file)
### Firewall Defacer
- Solves the firewall without needing to find solution
- Ram: 300 mb
- Time: 10.3 seconds
- Requires admin: No

## Comamnds
### Make File
- Creates a file
- Format: `mkfile [filename] [content]`
- Requires admin: Yes
### Make Folder/Directory
- Creates a new folder
- Format: `mkdir [dirname]`
- Requires admin: Yes

## Conditional Actions
### File Copied (FileCopied)
- Is triggered when a file is copied from the source node to the target node
- Attributes available:
  - target: The target comp (where file must be uploaded to)
  - targetFile: Name of the file
  - path: The path of the target comp where the file must be uploaded to
  - source: Source comp (where the file is)
  - sourcePath: The path where the file is in source comp
  - checkOnce: If to check every frame or just 1 time
### File Deleted (FileDeleted)
- Is triggered when a file is deleted from the specified node
- Attributes available
  - target: The target comp (where the file is)
  - targetFile: The name of the file to delete
  - path: The path where the file is in the target comp
  - checkOnce: If to check every frame or just 1 time
### File Created (FileCreated)
- Is triggered when a file is created in the specified node
- Attributes available
  - target: The target comp (where the file has to be created)
  - targetFile: The name that the created file must has
  - path: The path where the file must be created on
  - checkOnce: If to check every frame or just 1 time
### Has Points (HasPoints)
- Is triggered if the player has that amount of points
- Attributes available
   - placeholder: The placeholder of points
   - has: The condition (using operator, for example: `>=12` checks if the player has in the placeholder more than 12 or 12 points)
   - checkOnce: If to check every frame or just 1 time

## Actions
### Add Points (AddPoints)
- When executed the specified amount of points are added to the placeholder
- Attributes available
   - amount: The amount of points to add
   - placeholder: The placeholder where the points are added (Automatically created)
### Remove Points (RemovePoints
- When executed the specified amount of points are removed from the placeholder (Can't be negative)
- Attributes available
   - amount: The amount of points to remove
   - placeholder: The placeholder where the points are removed from
### Run Comamnd (CmdRun)
- When executed it runs the given command in terminal
- Attributes available
   - Comamnd: The command that will be executed (it can contain arguments)
### Send Mail (SendMail)
- When executed it sends an email to the player
- Attributes available
   - subject: The subject of the email
   - author: The author of the email
- SubElements (That go into the SendEmail tag
   - body: It's content (what is inside of it) will be the content of the email
   - attachments: It can contain inside attachments tags, these are them:
      - `<link comp="id"/>`
      - `<note title="Cool title">Cool content</note>`
      - `<account comp="id" user="Naix" pass="el"`

## Goals
### File Creation (filecreation)
- It's completed when the player creates a file in the target with the name and content given
- Attributes available
   - target: The target comp (where file has to be created)
   - path: The path where the file must be created on
   - filename: The name that the file must have
   - fileContent(?): The content that the file must have

## Extra
### checkOnce Attribute
- The checkOnce attribute is optional
- HasFlags and DoesNotHaveFlags (from the base game) also have checkOnce attribute
- It accepts true/false values (in string obviously)
- By default is `false`
### ? symbol
- (?) in attributes refers that it's optional
