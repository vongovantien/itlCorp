import { Component, OnInit, ViewChild } from '@angular/core';
import { ShareBusinessAsignmentComponent } from 'src/app/business-modules/share-business/components/asignment/asignment.component';

@Component({
    selector: 'sea-fcl-export-assignment',
    templateUrl: './sea-fcl-export-assignment.component.html'
})

export class SeaFCLExportAssignmentComponent implements OnInit {
    @ViewChild(ShareBusinessAsignmentComponent, { static: false }) assignment: ShareBusinessAsignmentComponent;

    constructor() { }

    ngOnInit() { }
}