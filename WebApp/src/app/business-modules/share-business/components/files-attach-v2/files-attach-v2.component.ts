import _find from 'lodash/find';
import { Component, Input, ViewChild, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppList } from 'src/app/app.list';
import { catchError, finalize, map, skip, takeUntil } from 'rxjs/operators';

import { SystemFileManageRepo } from '@repositories';
import _ from 'lodash';
import { ShareDocumentTypeAttachComponent } from '../document-type-attach/document-type-attach.component';
import { ActivatedRoute, Params } from '@angular/router';
import { IAppState } from '@store';
import { getTransactionDetailCsTransactionState } from '../../store';
import { CsTransaction } from '@models';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';


@Component({
    selector: 'files-attach-v2',
    templateUrl: './files-attach-v2.component.html'
})

export class ShareBussinessAttachFileV2Component extends AppList implements OnInit {

    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;

    documentTypes: any[] = [];
    headers: CommonInterface.IHeaderTable[];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    selectedEdoc: any;

    constructor(
        private _systemFileRepo: SystemFileManageRepo,
        private _activedRoute: ActivatedRoute,
        private _store: Store<IAppState>,
    ) {
        super();
    }

    ngOnInit() {
        this._activedRoute.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((params: Params) => {
                console.log(params);
                if (params.jobId) {
                    this.jobId = params.jobId;
                    console.log(params);
                } else {
                    this.jobId = params.id;
                    this.isOps = true;
                }
            });

        if (this.isOps == false) {
            this._store.select(getTransactionDetailCsTransactionState)
                .pipe(skip(1), takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: CsTransaction) => {
                        this.getDocumentType(res.transactionType);
                        this.getEDocByJobID(res.transactionType);
                    }
                );
        } else {
            this._store.select(getOperationTransationState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: any) => {
                        this.getDocumentType(res.transactionType);
                        this.getEDocByJobID(res.transactionType);
                    }
                );
        }


        this.headers = [
            { title: 'No', field: 'no' },
            { title: 'Alias Name', field: 'aliasName' },
            { title: 'Real File Name', field: 'realFilename' },
            { title: 'House Bill No', field: 'houseBillNo' },
            { title: 'Source', field: 'source' },
            { title: 'Tag', field: 'tag' },
            { title: 'Attach Time', field: 'attachTime' },
            { title: 'Attach Person', field: 'attachPerson' },
        ];
        //this.getDocumentType();
        //this.getEDocByJobID();
    }

    onSelectEDoc(edoc: any) {
        this.selectedEdoc = edoc;
        console.log(this.selectedEdoc);

    }

    downloadEdoc() {
        window.open(this.selectedEdoc.imageUrl, "_blank");
    }

    editEdoc() {

    }
    deleteEdoc() {

    }
    getDocumentType(transactionType: string) {
        this._systemFileRepo.getDocumentType(transactionType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {

                    this.documentTypes = res;
                    this.documentAttach.documentTypes = res;
                    console.log(this.documentAttach.documentTypes);

                },
            );
    }

    getEDocByJobID(transactionType: string) {
        this._systemFileRepo.getEDocByJob(this.jobId, transactionType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    this.edocByJob = res;
                    console.log(res);

                },
            );
    }
    // downloadEdoc() {
    //     console.log(this.selectedEdoc);

    //     document.location.href = this.selectedEdoc.imageUrl;
    // }

    showDocumentAttach() {
        this.documentAttach.headers = [
            { title: 'No', field: 'no' },
            { title: 'Alias Name', field: 'aliasName' },
            { title: 'Real File Name', field: 'realFilename' },
            { title: 'Document Type', field: 'docType' },
            { title: 'Job Ref', field: 'jobRef' },
            { title: 'Source', field: 'source' }
        ];
        this.documentAttach.isUpdate = false;
        this.documentAttach.show();
    }
}
