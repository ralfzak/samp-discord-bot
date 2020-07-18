
# SA:MP Discord Bot
This is a .NET Core discord bot built using [Discord.Net framework](https://github.com/discord-net/Discord.Net). This bot is also known as Woozie which is part of [SA:MP's discord server](https://forum.sa-mp.com/showthread.php?t=665803).

![Wu Zi Mu](WuZiMu.jpg)

[![Build Status](https://travis-ci.com/ralfzak/samp-discord-bot.svg?branch=master)](https://travis-ci.com/ralfzak/samp-discord-bot) [![License](https://img.shields.io/badge/license-GNU%203.0-orange)](LICENSE) [![Codecov branch](https://img.shields.io/codecov/c/github/ralfzak/samp-discord-bot/master)](https://github.com/ralfzak/samp-discord-bot)

## Features
### Verification
Linking a discord account to a forum account. Linking a forum account grants a verified role. This role is saved and attached to the discord user Id which allows role persistence in case the user left the server and came back.
This module introduces the following commands:
|Command|Parameters|Information|Availability|
|-|-|-|-|
|verify|[profile_id/done/cancel]|Links your discord account with a SAMP forum account using a simple verification process.|As a direct message|
|whois|[@]user|This command looks up SAMP forum profiles of given linked discord accounts.|Any channel|
|rvwhois|forum_id/forum_name|This command fetches discord user(s) linked to a given forum account.|Admin channel|
|fverify|[@]user [forumid]|Force links a discord account with a SAMP forum account|Admin channel|
|funverify|[@]user|Drops a forum verification and removes the user role|Admin channel|

### Banning
Issuing and monitoring discord bans. This module also supports timed discord bans. Discord bans are synced and persisted. A ban can be issued using discord's interface or using the bot's exposed commands.
This module introduces the following commands:
|Command|Parameters|Information|Availability|
|-|-|-|-|
|ban|[@]user [days] [hours] [send ban message] [reason]|This command issues a discord ban.|Admin channel|
|banlookup|userid/username|This command lists a list of banned account based on a search criteria.|Admin channel|
|unban|userid/username|This command lifts a list of bans given a certain criteria.|Admin channel|

### SA:MP Server Querying
Querying SA:MP servers to fetch live data. Data is queried using two packet types: **r** and **i**. Find more information [here](https://wiki.sa-mp.com/wiki/Query_Mechanism).
This module introduces the following commands:
|Command|Parameters|Information|Availability|
|-|-|-|-|
|server/srv|ip/hostname[:port]|This command queries a given SA:MP server with two packets to fetch general and rules data. More information [here](https://wiki.sa-mp.com/wiki/Query)|Bot channel|

### SA:MP Wiki
Fetching SA:MP wiki page data and parsing it. This module also implements [Levenshtein distance](https://en.wikipedia.org/wiki/Levenshtein_distance) to match an article name to closest pre-defined articles.
This module introduces the following commands:
|Command|Parameters|Information|Availability|
|-|-|-|-|
|wiki|article|This command fetches articles from the official SAMP Wiki|Bot and scripting channel|

### SA:MP global stats parsing
Responsible for fetching and parsing global SA:MP stats from [sa-mp.com](https://sa-mp.com/). The stats include parsing global servers count and global players count. This module does not introduce any commands.

## Configuration
Most of the bot can be configured using environment variables.
|Key|Description|
|-|-|
|GUILD_COMMAND_PREFIX|The prefix for command execution. Default: backslash */*|
|BOT_TOKEN|Discord bot client secret. More information [here](https://discord.com/developers/docs/intro).|
|BOT_ID|Bot client Id. More information [here](https://discord.com/developers/docs/intro).|
|GUILD_ID|Discord server Id. More information [here](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-).|
|GUILD_SCRIPTING_CHANNEL_ID|Scripting channel Id. Used to limit some commands usage. More information [here](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-).|
|GUILD_BOTCOMMANDS_CHANNEL_ID|Bot channel Id. Used to limit some commands usage. More information [here](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-).|
|GUILD_ADMIN_CHANNEL_ID|Admin channel Id. Used to limit some commands usage and logging. More information [here](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-).|
|GUILD_VERIFIED_ROLE_ID|Verified role Id. Role to be granted when a user successfully verifies. More information [here](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-) and [here](https://github.com/ralfzak/samp-discord-bot#verification).|
|DATABASE_HOST|Database host.|
|DATABASE_SCHEMA|Database schema, aka database name.|
|DATABASE_USER|Database user having access to the schema.|
|DATABASE_PASSWORD|Database user password.|
|URL_SAMP_WEBSITE|SA:MP's official website. Used for global stats fetching and in different references. Default: https://sa-mp.com|
|URL_SAMP_HOSTEDTABPROVIDER|SA:MP's hosted tab listing. Used to identify if a server is on hosted tab. Default: https://game-mp.com|
|URL_FORUM_PROFILE|Forum profiles link. Used to query forum profiles followed by a profile Id and in different references. Default: https://forum.sa-mp.com/member.php?u=|
|URL_FORUM_SETTINGS|Forum profile privacy settings link. Used in different references. Default: http://forum.sa-mp.com/profile.php?do=privacy|
|URL_WIKI_DOCS|Wiki articles link. Used to query wiki pages followed by the article name and in different references. Default: https://wiki.sa-mp.com/wiki/|
|URL_WIKI_SEARCH|Wiki search link. Used to reference searching wiki pages followed by the article name. Default: https://wiki.sa-mp.com/wiki/Special:Search?search=|

## Dependencies
1. Docker & Docker Compose. Download [here](https://docs.docker.com/get-docker/).
3. Discord application. More information [here](https://discord.com/developers/docs/intro).

## Installation
1. Change your secrets in `run.sh`.
2. Configure the bot as needed by changing the container's environment variables in `docker-compose.yml`.
3. Start the containers `docker-compose up`.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Upcoming work
* Setup a proper CD pipeline.

## License
[GNU 3.0](https://github.com/ralfzak/samp-discord-bot/blob/master/LICENSE)
