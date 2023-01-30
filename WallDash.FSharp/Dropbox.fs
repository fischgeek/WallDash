﻿namespace WallDash.FSharp

open DropboxConnect

module Dropbox = 
    let GetSpace() = 
        printf "\tGetting Dropbox space..."
        //let used,allocated,free,percentUsed = DropboxConnect.GetSpaceDetailsFull()
        let used = "0"
        printfn "Done."
        $"""
            <div id='dropbox-drive' data-percent='2' data-text="<img src='https://img.icons8.com/ultraviolet/40/000000/dropbox.png'/>" data-animate='false' class='green medium circle float-end'>
                <span class='drive-space' style='margin-top: 2px'>{used}</span>
            </div>
        """




         //<span class='drive-icon'><img src='https://img.icons8.com/ultraviolet/40/000000/dropbox.png'/></span>