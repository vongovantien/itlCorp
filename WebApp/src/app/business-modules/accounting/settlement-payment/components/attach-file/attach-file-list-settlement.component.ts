import { Component, OnInit } from '@angular/core';
import { AppForm } from '@app';

@Component({
    selector: 'settlement-attach-file-list',
    templateUrl: './attach-file-list-settlement.component.html',
    styleUrls: ['./attach-file-settlement.component.scss']
})

export class SettlementAttachFileListComponent extends AppForm implements OnInit {

    files: any[] = [];
    selectedFile: any;

    constructor() {
        super();
    }

    ngOnInit() { }
}