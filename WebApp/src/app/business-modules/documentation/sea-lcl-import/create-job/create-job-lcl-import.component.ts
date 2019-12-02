import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Router } from '@angular/router';

@Component({
    selector: 'app-create-job-lcl-import',
    templateUrl: './create-job-lcl-import.component.html'
})

export class SeaLCLImportCreateJobComponent extends AppForm implements OnInit {
    constructor(
        protected _router: Router
    ) {
        super();
    }

    ngOnInit() { }

    gotoList() {
        this._router.navigate(["home/documentation/sea-lcl-import"]);
    }

    showImportPopup() {

    }

    onCreateJob() {

    }

}
