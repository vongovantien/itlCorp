import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-job-fcl-export',
    templateUrl: './create-job-fcl-export.component.html'
})

export class SeaFCLExportCreateJobComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }

    onCreateJob() {

    }
}
