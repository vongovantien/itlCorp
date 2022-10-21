import { Component, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';

import { catchError, skip, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

import { ActivatedRoute, Params } from '@angular/router';
import { CsTransaction } from '@models';
import { SystemFileManageRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { getTransactionDetailCsTransactionState } from '../../store';
import { ShareDocumentTypeAttachComponent } from '../document-type-attach/document-type-attach.component';


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
    edocByAcc: any[] = [];
    selectedEdoc: any;
    transationType: string;
    housebills: any[];
    headerAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType' },
    { title: 'Job Ref', field: 'jobRef' },
    { title: 'House Bill No', field: 'hbl' },
    { title: 'Note', field: 'note' },
    { title: 'Source', field: 'source' },]

    jobNo: string = '';

    constructor(
        private _systemFileRepo: SystemFileManageRepo,
        private _activedRoute: ActivatedRoute,
        private _store: Store<IAppState>,
        private _toast: ToastrService,

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
                        this.transationType = res.transactionType;
                        this.getDocumentType(res.transactionType);
                        this.getEDocByJobID(res.transactionType);
                        this.jobNo = res.jobNo;
                    }
                );
        } else {
            this._store.select(getOperationTransationState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: any) => {
                        this.transationType = res.transactionType;
                        this.getDocumentType(res.transactionType);
                        this.getEDocByJobID(res.transactionType);
                        this.jobNo = res.jobNo;
                    }
                );
        }


        this.headers = [
            { title: 'Alias Name', field: 'aliasName' },
            { title: 'Real File Name', field: 'realFilename' },
            { title: 'Job Ref', field: 'jobRef' },
            { title: 'House Bill No', field: 'houseBillNo' },
            { title: 'Billing No', field: 'billingNo' },
            { title: 'Source', field: 'source' },
            { title: 'Tag', field: 'tag' },
            { title: 'Attach Time', field: 'attachTime' },
            { title: 'Attach Person', field: 'attachPerson' },
        ];
        //this.getDocumentType();
        //this.getEDocByJobID();
        //this.documentAttach.headers = this.headerAttach;
    }


    onSelectEDoc(edoc: any) {
        this.selectedEdoc = edoc;
        console.log(this.selectedEdoc);
        this.selectedEdoc.name = edoc.userFileName;
    }

    downloadEdoc() {
        window.open(this.selectedEdoc.imageUrl, "_blank");
        this._toast.success("Download Sucess")
    }

    editEdoc() {
        console.log(this.documentAttach.listFile);
        console.log(this.selectedEdoc);
        // this.documentAttach.headers = [
        //     { title: 'No', field: 'no' },
        //     { title: 'Alias Name', field: 'aliasName' },
        //     { title: 'Real File Name', field: 'realFilename' },
        //     { title: 'Document Type', field: 'docType' },
        //     { title: 'Job Ref', field: 'jobRef' },
        //     { title: 'Source', field: 'source' }
        // ];
        this.documentAttach.headers = this.headerAttach;
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        this.documentAttach.listFile.push(this.selectedEdoc);
        console.log(this.documentAttach.listFile);
        this.documentAttach.show();
    }

    deleteEdoc() {
        this._systemFileRepo.deleteEdoc(this.selectedEdoc.id)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toast.success("Delete Sucess")
                        this.getEDocByJobID(this.transationType);
                    }
                },
            );
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
                    this.edocByJob = res.filter(x => x.documentType.type !== 'Accountant');
                    this.edocByAcc = res.filter(x => x.documentType.type === 'Accountant');
                    console.log(res);
                },
            );
    }
    // downloadEdoc() {
    //     console.log(this.selectedEdoc);

    //     document.location.href = this.selectedEdoc.imageUrl;
    // }

    showDocumentAttach() {
        // this.documentAttach.headers = [
        //     { title: 'No', field: 'no' },
        //     { title: 'Alias Name', field: 'aliasName' },
        //     { title: 'Real File Name', field: 'realFilename' },
        //     { title: 'Document Type', field: 'docType' },
        //     { title: 'Job Ref', field: 'jobRef' },
        //     { title: 'House Bill No', field: 'hbl' },
        //     { title: 'Note', field: 'note' },
        //     { title: 'Source', field: 'source' },
        // ];
        this.documentAttach.headers = this.headerAttach;
        this.documentAttach.isUpdate = false;
        this.documentAttach.show();
    }
}
