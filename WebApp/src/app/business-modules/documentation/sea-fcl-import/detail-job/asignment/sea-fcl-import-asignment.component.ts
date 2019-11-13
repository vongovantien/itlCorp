import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { ActivatedRoute } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-sea-fcl-import-asignment',
    templateUrl: './sea-fcl-import-asignment.component.html'
})

export class SeaFCLImportAsignmentComponent extends AppList {
    data: any = null;
    jobId: string = '';

    constructor(
        private _operation: OperationRepo,
        private _activedRouter: ActivatedRoute,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            if (param.id) {
                this.jobId = param.id;
                this.getCSTransactionDetails(this.jobId);

            }
        });
    }


    getCSTransactionDetails(id: any) {
        this._progressRef.start();
        this._documentRepo.getDetailTransaction(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            ).subscribe(
                (response: any) => {

                    this.data = response;
                    console.log(this.data);
                },
            );
    }

}