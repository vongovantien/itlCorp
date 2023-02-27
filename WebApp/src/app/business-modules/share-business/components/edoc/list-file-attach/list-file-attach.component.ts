import { Component, OnInit } from "@angular/core";
import { AppList } from "src/app/app.list";

@Component({
    selector: 'list-files-attach',
    templateUrl: './list-file-attach.component.html',
})

export class ShareBussinessListAttachFileComponent extends AppList implements OnInit {
    constructor(
    ) {
        super();
    }

    ngOnInit() { }
}
