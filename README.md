# Overview
This project automates downloading videos from AWS events such as reinvent.
There is nothing too specific to downloading AWS videos however, so this can
easily be adapted to download vidos from other sources.

# Prerequisites

## Youtube-dl
This program relies on having access to `youtube-dl` on the command line. It has
been tested using version `2021.06.06` on Windows installed using Chocolatey. The files in the 
`/data` directory serve as examples.

## AWS Events Auth
In order to view AWS Events on demand videos you must be authenticated. To
provide authentication for this program you must log in so that you can view the
videos in a browser, and then export the browsers cookies to a file in the
netscape cookie format. There are browser extensions which allow you to do this
such as https://addons.mozilla.org/en-US/firefox/addon/cookies-txt/ for Firefox.

## AWS Events Notes
For AWS Reinvent 2021, the browser URLs worked just fine with youtube-dl downloader, just copy from 
the URL bar and paste into the CSV.

For the AWS Innovate AI/ML event in 2022, youtube-dl could no longer download directly from the URLs
in the browser. Instead you must get the URLs that are returned from the kaltura (a video service) 
play manifest endpoint. here is an example URL and it's response:
```
GET https://www.kaltura.com/p/4374333/sp/437433300/playManifest/entryId/1_bp996ar0/protocol/https/format/applehttp/flavorIds/1_zfrisnxw,1_5jx7oiuf,1_esvgi0fm,1_8wdc3xh5,1_re6rleqs,1_7vmgv1ex,1_jipqqije/ks/...etc

RESPONSE:
#EXTM3U
#EXT-X-STREAM-INF:PROGRAM-ID=1,BANDWIDTH=158998,RESOLUTION=426x240
https://play.virtual.awsevents.com/scf/hls/p/4374333/sp/437433300/serveFlavor/entryId/1_bp996ar0/v/31/ev/32/flavorId/1_zfrisnxw/name/a.mp4/index.m3u8...etc
#EXT-X-STREAM-INF:PROGRAM-ID=1,BANDWIDTH=263625,RESOLUTION=640x360
https://play.virtual.awsevents.com/scf/hls/p/4374333/sp/437433300/serveFlavor/entryId/1_bp996ar0/v/31/ev/32/flavorId/1_5jx7oiuf/name/a.mp4/index.m3u8...etc
...etc
```
Notice that this gives a URL for each quality option. The easiest way to do this is as follows:
1. Open Firefox > developer tools > network tab
2. enter "playManifest" into the search
3. Navigate to the video you wish to download
4. You'll see a new request for "playManifest" data appear in the network tab, click to see the 
   response value
5. Copy the URL for the resolution you wish to download.

This whole process could be automated, but I haven't bothered to do this yet.

# How to run this program
To build and execute this program, use the `dotnet run` command, place program
arguments after `--` to separate arguments for `dotnet` and for this program.

## Examples

### Get the help text for this program
`dotnet run -- --help`

### Start Downloading videos with concurrency = 10
`dotnet run -- -c 10 C:\Users\gian\Documents\workspace\aws_downloader\data\URLs.csv C:\Users\gian\Documents\workspace\aws_downloader\data\auth_cookies.txt C:\Users\gian\Documents\workspace\aws_downloader\output`
