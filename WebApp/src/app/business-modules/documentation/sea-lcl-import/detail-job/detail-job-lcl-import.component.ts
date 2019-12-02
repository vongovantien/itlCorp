import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { SeaLCLImportCreateJobComponent } from '../create-job/create-job-lcl-import.component';

@Component({
    selector: 'app-sea-lcl-import-detail-job',
    templateUrl: './detail-job-lcl-import.component.html'
})

export class SeaLCLImportDetailJobComponent extends SeaLCLImportCreateJobComponent implements OnInit {
    constructor(
        protected _router: Router
    ) {
        super(_router);
    }

    ngOnInit() { }
}
