import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-sea-lcl-export-housebill-list',
    templateUrl: './sea-lcl-export-housebill-list.component.html',
    styleUrls: ['./sea-lcl-export-housebill-list.component.scss']
})
export class SeaLclExportHousebillListComponent implements OnInit {

    openCD: boolean = false;
    open_CD() {
        this.openCD = true;
    }
    constructor() { }

    ngOnInit() {
    }

}
