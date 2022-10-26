import { Component, Input, OnInit, ViewChild } from '@angular/core';
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
    @Input() typeFrom: string = 'Job';
    @Input() billingId: string = '';
    documentTypes: any[] = [];
    headers: CommonInterface.IHeaderTable[];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    edocByAcc: any[] = [];
    selectedEdoc: any;
    transactionType: string = '';
    housebills: any[];

    headerAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType' },
    { title: 'Job Ref', field: 'jobRef' },
    { title: 'House Bill No', field: 'hbl' },
    { title: 'Note', field: 'note' },
    { title: 'Source', field: 'source' },]
    accountantAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType' },
    { title: 'Note', field: 'note' },]

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
        if (this.typeFrom === 'Job') {
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
                            this.transactionType = res.transactionType;
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
                            this.transactionType = 'CL';
                            this.getDocumentType('CL');
                            this.getEDocByJobID('CL');
                            this.jobNo = res.jobNo;
                        }
                    );


            }
        } else {
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom);
        }

        this.headers = [
            { title: 'Alias Name', field: 'aliasName' },
            { title: 'Real File Name', field: 'realFilename' },
            { title: 'House Bill No', field: 'houseBillNo' },
            { title: 'Billing No', field: 'billingNo' },
            { title: 'Source', field: 'source' },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'attachTime' },
            { title: 'Attach Person', field: 'attachPerson' },
        ];
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
        if (this.typeFrom === 'Job') {
            this.documentAttach.headers = this.headerAttach;
        } else {
            this.documentAttach.headers = this.accountantAttach;
        }
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        let detailSeletedEdoc = ({
            aliasName: this.selectedEdoc.systemFileName,
            name: this.selectedEdoc.userFileName,
        })
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        console.log(this.documentAttach.detailDocId);

        this.documentAttach.listFile.push(detailSeletedEdoc);
        console.log(this.documentAttach.listFile);
        this.documentAttach.show();
        this.getEDocByJobID(this.transactionType);
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
                        this.getEDocByJobID(this.transactionType);
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
                    console.log(res);

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

    showDocumentAttach() {
        if (this.typeFrom === 'Job') {
            this.documentAttach.headers = this.headerAttach;
        } else {
            this.documentAttach.headers = this.headerAttach;
        }
        this.documentAttach.isUpdate = false;
        this.documentAttach.show();
    }
}
