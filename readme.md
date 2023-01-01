# Sprite Splitter Console 
A command-line tool for splitting a sheet of sprites into a set of transparent bitmaps. Windows only because it uses `GetPixel` and `SetPixel` in `System.Drawing.Common`.

## Usage
*Simple*
`dotnet run .\sheets\test.png`

*Advanced*
`dotnet run file threshold min-width min-height`
`dotnet run .\sheets\test.png 17 20 20`

# Sprite Splitter Api
an api host running asp.net.

## Api
Serves some stand-alone javascript tooling.
Requesting `GET` from `/` returns an experimental js based sprite splitter.
Requesting `GET` from `/viewer` returns a static html page for generating sprite sheets.
