// Copyright (c) 2026 kurnakovv
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for full license information.

namespace Biak.ConsoleApp.Constants;

/// <summary>
/// All docs constants here.
/// </summary>
public static class DocsConstant
{
    /// <summary>
    /// `dotnet biak` greeting text.
    /// </summary>
    public const string GREETING = @"
       __________________________________
      | Hi, I'm biak!                    |
      | I was made by kurnakovv          |
      | To work with .editorconfig       |
      |__________________________________|
                                          \
                                           \
                           WWNNWWWWWWWWWWNNNXXXXXXXXXNNWWW                                     
                        WNKOkdlodOXXK0Okxdollccc:cccclodxOKXNNW                                
                       W0ooxo;'.,colc:;;::;,,,,''''.....',;cldOKXNNK000XNW                     
                      WKo,'''.':l:,;cokO00Oo:;;;;,,''.......;:;:oxxl:ddclkXW                   
                     WW0l'..',,:c:ckKNNNX0xc;;,,,,,'''......,;,..';:;;,'.,oKW                  
                  WX0kxxdc,''.'cddok00Oxdc:;;,,,,,'''...............',,'..:ON                  
                WXkxo;',:,...'cxko:::::;;;;,,,,''''''.................,;;;l0W                  
                Xd;;;',,.',;;;;::;;;;;;;,,,,,'''''''...................':okXW                  
               WKl'.','..;l:,,,,'''',,,,,,'''''''''......................;o0XW                 
                Nkc:;'...''''... .,;..'''''''''''.........''.   ..........;o0XW                
                WXOo'........   .kNNo...''''''...........dXKc     .........;o0NW               
                NKx,........    .:do,  ................ .;ol'      .........;dKNW              
               WXk:.........           ................            ..........:xKN              
              WN0l..........           ................            ..........'cOXW             
              NKx;........''..        ..................      .,...'..........,dKNW            
             WN0l........',co:...  .......................    ..'co:'..........cOXW            
             NKx;.........',:;,'...........'...................',;;,...........,o0NW           
            WXOl..............................'.................................:kKN           
           WN0d,.......................................................  .......'lOXNW         
         WNXOo;....   ................................................  .....  ..,oOKXXNW      
       WNKkdc:,....    .............................................    ....    ..;ldxxdOKNW   
     WKkolodxl;'..      .........................................        .        .',,,',lkKW  
    NOl;;lxOkl,..        .....................................             ....    .......;d0W 
   NOc..,;;;,....... ..,:::;...........................      .     ....   ...... .'..   ...;xX 
   Xx;..........;,....cdddoc,.............               ........,:lll:,.......  ....   ...;dX 
   NOl'.........''.....',''...........                 .........'cxkkkd:'''.... ..........;lON 
    NOl;'.........................................................',,,'.',............'';cd0N  
     WXOdl:;,,,,,,'..........................................';,................',;:clodkKNW   
       WWNXK0Okxddlcc::;;,,,'''.............................',;;,,,,,,,,,,,,,;:cox0KXXNWW      
              WWNXK0kkxddollcc::;;,,,''....................',;:cclllllooooddxk0KNW             
                    WWNNXXK00OOkkkkxxxxdddddddddddddddddddxxkkkOOO0000KKXXNNWW                 
                              WWWWWWWWWWNNNNNNNNNNNNNNNNWWWWWWWW         
                                   

                                                                  ___________________________________
                                                                 | GitHub                            |
                                                                 | https://github.com/kurnakovv/biak |
                                                                 |                                   |
                                                                 | Need help?                        |
                                                                 | dotnet biak --help                |
                                                                 |___________________________________|
";

    /// <summary>
    /// `dotnet biak --help` text.
    /// </summary>
    public const string HELP = @"--------------------
Enable / Disable .editorconfig rules | Change severity level with one command without losing the original values.

---

* dotnet biak setup | The setup command initializes the Biak environment in your current project directory by creating a dedicated configuration folder and copying your existing .editorconfig into it https://github.com/kurnakovv/biak/wiki/Setup

* dotnet biak enable | The enable command activates Biak configuration in your project by copying the managed configuration file from .biak/.editorconfig-main back to the root .editorconfig (enable all rules in .editorconfig file) https://github.com/kurnakovv/biak/wiki/Enable

* dotnet biak disable | The disable command takes the contents of editorconfig-main, disables all rules (error|warning|suggestion -> none) and inserts them into .editorconfig https://github.com/kurnakovv/biak/wiki/Disable
--------------------";
}
