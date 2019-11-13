import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { ActivatedRoute } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-sea-fcl-import-asignment',
    templateUrl: './sea-fcl-import-asignment.component.html'
})

export class SeaFCLImportAsignmentComponent extends AppList {
    constructor(
        private _operation: OperationRepo,
        private _activedRouter: ActivatedRoute,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
    ) {
        super();
    }

    ngOnInit() { }
}