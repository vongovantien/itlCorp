import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { SystemConstants } from '@constants';
import { Store } from '@ngrx/store';
import { SystemFileManageRepo } from '@repositories';
import { IAppState } from '@store';
import _uniqBy from 'lodash/uniqBy';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { getAdvanceDetailRequestState } from 'src/app/business-modules/accounting/advance-payment/store';
import { getGrpChargeSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getSOADetailState } from 'src/app/business-modules/accounting/statement-of-account/store/reducers';
import { PopupBase } from 'src/app/popup.base';
import { getTransactionLocked, getTransactionPermission } from '../../store';
@Component({
    selector: 'document-type-attach',
    templateUrl: './document-type-attach.component.html',
    styleUrls: ['./document-type-attach.component.scss']
})
export class ShareDocumentTypeAttachComponent extends PopupBase implements OnInit {
    @Input() jobNo: string = '';
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    @Input() housebills: any[] = [];
    //@Input() jobs: any[] = [];
    @Input() billingId: string = '';
    @Input() billingNo: string = '';
    @Input() jobId: string = '';
    @Input() transactionType: string = '';
    @Input() selectedtDocType: any = null;
    @Input() selectedtTrantype: any = null;
    @Input() typeFrom: string = 'Shipment';

    headers: CommonInterface.IHeaderTable[] = [];
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];
    isUpdate: boolean = false;
    detailDocId: number;
    formData: IEDocUploadFile;
    documentTypes: any[] = [];
    isSubmitted: boolean = false;
    configJob: CommonInterface.IComboGirdConfig | any = {};
    jobDatSource: any[] = [];
    configDocType: CommonInterface.IComboGirdConfig | any = {};

    constructor(
        private _toastService: ToastrService,
        private _store: Store<IAppState>,
        private _systemFileManagerRepo: SystemFileManageRepo,
    ) {
        super();
        this.isLocked = this._store.select(getTransactionLocked);
        this.permissionShipments = this._store.select(getTransactionPermission);
    }

    ngOnInit(): void {
        this.getJobList();
        this.getDocumentType();
        this.transactionType = this.typeFrom;
        this.configJob = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'jobId', label: 'JobID' },
                { field: 'customNo', label: 'Custom No' },
                { field: 'hbl', label: 'House Bill' },
                { field: 'mbl', label: 'Master Bill' },
            ]
        }, { selectedDisplayFields: ['jobId'], });
        this.configDocType = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Code' },
                { field: 'nameEn', label: 'Name EN' },
            ]
        }, { selectedDisplayFields: ['nameEn'], });
    }

    chooseFile(event: any) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        let docType: any;
        if (this.documentTypes.length === 1) {
            docType = this.documentTypes[0];
        }
        for (let i = 0; i < files.length; i++) {
            if (!!docType) {
                files[i].Code = docType.code;
                files[i].DocumentId = docType.id;
                files[i].docType = docType;
                files[i].aliasName = docType.code + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
            }
            this.listFile.push(files[i]);
            //this.listFile[i].aliasName = files[i].name.substring(0, files[i].name.lastIndexOf('.'));
        }
        if (fileList?.length > 0) {
            let validSize: boolean = true;
            for (let i = 0; i <= fileList?.length - 1; i++) {
                const fileSize: number = fileList[i].size / Math.pow(1024, 2); //TODO Verify BE
                if (fileSize >= 100) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toastService.warning("maximum file size < 100Mb");
                return;
            }
        }
        event.target.value = ''

    }

    getDocumentType() {
        this._systemFileManagerRepo.getDocumentType(this.transactionType, this.billingId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    this.documentTypes = res;
                    this.configDocType.dataSource = res;
                },
            );
    }


    getJobList() {
        //let jobs: any[] = [];
        if (this.typeFrom === 'Settlement') {
            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            _uniqBy(data, 'hbl').forEach(element => {
                                let item = ({
                                    jobId: element.jobId,
                                    id: element.shipmentId,
                                    customNo: element.clearanceNo,
                                    hbl: element.hbl,
                                    mbl: element.mbl

                                })
                                this.jobDatSource.push(item);
                            }
                            );
                        }
                    }
                );
        } else if (this.typeFrom === 'Advance') {
            this._store.select(getAdvanceDetailRequestState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            _uniqBy(data, 'hbl').forEach(element => {
                                let item = ({
                                    jobId: element.jobId,
                                    id: element.shipmentId,
                                    customNo: element.customNo,
                                    hbl: element.hbl,
                                    mbl: element.mbl

                                })
                                this.jobDatSource.push(item);
                            }
                            );
                        }
                    }
                );
        } else if (this.typeFrom === 'SOA') {
            this._store.select(getSOADetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            _uniqBy(data.chargeShipments, 'hbl').forEach(element => {
                                let item = ({
                                    jobId: element.jobId,
                                    id: element.shipmentId,
                                    customNo: element.clearanceNo,
                                    hbl: element.hbl,
                                    mbl: element.mbl

                                })
                                this.jobDatSource.push(item);
                            }
                            );
                        }
                    }
                );
        }

    }

    onSelectDataFormInfo(event: any, index: number, type: string) {
        switch (type) {
            case 'docType':
                this.listFile[index].Code = event.code;
                this.listFile[index].DocumentId = event.id;
                this.listFile[index].aliasName = this.isUpdate ? event.code + '_' + this.listFile[index].name : event.code + '_' + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                this.selectedtDocType = event.id;
                break;
            case 'aliasName':
                this.listFile[index].aliasName = event;
                break;
            case 'houseBill':
                this.listFile[index].hblid = event.id;
                break;
            case 'job':
                this.listFile[index].jobNo = event.jobNo;
                this.listFile[index].jobId = event.id;
                break;
            case 'note':
                this.listFile[index].note = event;
                break;
        }
    }
    resetForm() {
        this.listFile?.splice(0, this.listFile.length);
    }
    removeFile(index: number) {
        this.listFile?.splice(index, 1);
    }
    uploadEDoc() {
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        this.isSubmitted = true;
        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: this.typeFrom === 'Shipment' ? this.jobId : x.jobId !== undefined ? x.jobId : SystemConstants.EMPTY_GUID,
                Code: x.Code,
                TransactionType: this.transactionType,
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: this.typeFrom,
                HBL: x.hblid !== undefined ? x.hblid : SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: x.note !== undefined ? x.note : '',
                BillingId: this.billingId !== '' ? this.billingId : SystemConstants.EMPTY_GUID,
                Id: x.id !== undefined ? x.id : SystemConstants.EMPTY_GUID,
                DocumentId: x.DocumentId
            }));
        });
        this.EdocUploadFile = ({
            ModuleName: this.typeFrom === 'Shipment' ? 'Document' : 'Accounting',
            FolderName: this.typeFrom,
            Id: this.typeFrom === 'Shipment' ? this.jobId !== undefined ? this.jobId : SystemConstants.EMPTY_GUID : this.billingId,
            EDocFiles: edocFileList,
        })


        if (this.isUpdate) {
            let edocUploadModel: any = {
                Hblid: edocFileList[0].HBL,
                SystemFileName: edocFileList[0].AliasName,
                Note: edocFileList[0].Note,
                Id: edocFileList[0].Id,
                JobId: edocFileList[0].JobId,
                DocumentTypeId: this.selectedtDocType,
                TransactionType: this.selectedtTrantype,
            }

            if (edocUploadModel.DocumentTypeId === undefined || edocUploadModel.SystemFileName === '') {
                this._toastService.error("Please fill all field!");
                return;
            }
            this._systemFileManagerRepo.updateEdoc(edocUploadModel)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.resetForm();
                            this.hide();
                            this.onSearch.emit(this.transactionType);
                            this.isSubmitted = false;
                        }
                    }
                );
        } else {
            if (edocFileList.find(x => x.DocumentId === undefined || x.AliasName === '')) {

                this._toastService.error("Please fill all field!");
                return;
            }
            this._systemFileManagerRepo.uploadEDoc(this.EdocUploadFile, files, this.typeFrom)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.resetForm();
                            this.hide();
                            this.onSearch.emit(this.transactionType);
                            this.isSubmitted = false;
                        }
                    }
                );
        }
    }

    removeJob(index: number) {
        this.listFile[index].jobNo = null;
        this.listFile[index].jobId = null;
    }

    removeDocType(index: number) {
        this.listFile[index].Code = null;
        this.listFile[index].DocumentId = null;
        this.listFile[index].aliasName = null;
        this.selectedtDocType = null;
    }
}
export interface IEDocUploadFile {
    FolderName: string,
    ModuleName: string,
    EDocFiles: IEDocFile[],
    Id: string,
}
export interface IEDocFile {
    JobId: string,
    Code: string,
    TransactionType: string,
    AliasName: string,
    BillingNo: string,
    BillingType: string,
    HBL: any
    FileName: string,
    Note: string,
    BillingId: string,
    Id: string,
    DocumentId: string
}
