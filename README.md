# Overview
This project automates downloading videos from AWS events such as reinvent.
There is nothing too specific to downloading AWS videos however, so this can
easily be adapted to download vidos from other sources.

# Prerequisites

## Youtube-dl
This program relies on having access to `youtube-dl` on the command line. It has
been tested using version `2021.06.06` on Windows installed using Chocolatey

## AWS Events Auth
In order to view AWS Events on demand videos you must be authenticated. To
provide authentication for this program you must log in so that you can view the
videos in a browser, and then export the browsers cookies to a file in the
netscape cookie format. There are browser extensions which allow you to do this
such as https://addons.mozilla.org/en-US/firefox/addon/cookies-txt/ for Firefox.

# How to run this program
To build and execute this program, use the `dotnet run` command, place program
arguments after `--` to separate arguments for `dotnet` and for this program.

## Examples

### Get the help text for this program
`dotnet run -- --help`

### Start Downloading videos with concurrency = 10
`dotnet run -- -c 10 C:\Users\gian\Documents\workspace\aws_downloader\data\URLs.csv C:\Users\gian\Documents\workspace\aws_downloader\data\auth_cookies.txt C:\Users\gian\Documents\workspace\aws_downloader\output`
