import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';

@Component({
    selector: 'app-sea-lcl-import',
    templateUrl: './sea-lcl-import.component.html'
})
export class SeaLCLImportComponent extends AppList implements OnInit {

    constructor(
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
    }

    gotoCreateJob() {
        this._router.navigate(["home/documentation/sea-lcl-import/new"]);
    }

}
