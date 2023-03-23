import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { SystemConstants } from '@constants';
import { Store } from '@ngrx/store';
import { SystemFileManageRepo } from '@repositories';
import { IAppState } from '@store';
import _uniqBy from 'lodash/uniqBy';
import moment from 'moment';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { getAdvanceDetailRequestState } from 'src/app/business-modules/accounting/advance-payment/store';
import { LoadListEDocSettle, getGrpChargeSettlementPaymentDetailState, getSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getSOADetailState } from 'src/app/business-modules/accounting/statement-of-account/store/reducers';
import { PopupBase } from 'src/app/popup.base';
import { getTransactionLocked, getTransactionPermission } from '../../../store';
import { ShareListFilesAttachComponent } from '../list-file-attach/list-file-attach.component';
@Component({
    selector: 'document-type-attach',
    templateUrl: './document-type-attach.component.html',
    styleUrls: ['./document-type-attach.component.scss']
})
export class ShareDocumentTypeAttachComponent extends PopupBase implements OnInit {

    @ViewChild(ShareListFilesAttachComponent) listFileAttach: ShareListFilesAttachComponent;

    @Output() onSearchEdoc: EventEmitter<any> = new EventEmitter<any>();

    @Input() jobNo: string = '';
    @Input() housebills: any[] = [];
    @Input() billingId: string = '';
    @Input() billingNo: string = '';
    @Input() jobId: string = '';
    @Input() transactionType: string = '';
    @Input() selectedDocType: any = null;
    @Input() selectedTrantype: any = null;
    @Input() typeFrom: string = 'Shipment';
    @Input() docTypeId: number = 0;
    @Input() documentTypes: any[] = [];
    @Input() readonly: boolean = false;

    lstEdocExist: any[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];
    isUpdate: boolean = false;
    detailDocId: number;
    formData: IEDocUploadFile;
    isSubmitted: boolean = false;
    configJob: CommonInterface.IComboGirdConfig | any = {};
    configPayee: CommonInterface.IComboGirdConfig | any = {};
    configINV: CommonInterface.IComboGirdConfig | any = {};
    jobDataSource: any[] = [];
    payeeDataSource: any[] = [];
    invDataSource: any[] = [];
    invDataSourceUpdate: [any[]] = [[]];
    configDocType: CommonInterface.IComboGirdConfig | any = {};
    enablePayeeINV: boolean[] = [];
    payFirst: boolean[] = [];
    payFilled: boolean = true;
    jobOnSettle: boolean = false;
    isEdocByAcc: boolean = false;

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
                { field: 'code', label: 'Code' },
                { field: 'nameEn', label: 'Name EN' },
            ]
        }, { selectedDisplayFields: ['nameEn'], });

        if (this.typeFrom === 'Settlement') {
            this.configPayee = Object.assign({}, this.configComoBoGrid, {
                displayFields: [
                    { field: 'payer', label: 'Name EN' },
                ]
            }, { selectedDisplayFields: ['payer'], });
            this.configINV = Object.assign({}, this.configComoBoGrid, {
                displayFields: [
                    { field: 'invoiceNo', label: 'Invoice No' },
                    { field: 'invoiceDate', label: 'Invoice Date' },
                ]
            }, { selectedDisplayFields: ['invoiceNo'], });

            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data: any[]) => {
                        if (!!data && data.length > 0) {
                            this.getDocType(data.some(x => x.advanceNo !== null))
                        }
                    }
                );
            this._store.select(getSettlementPaymentDetailState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((res) => {
                    if (res) {
                        this.billingId = res.settlement.id;
                        this.billingNo = res.settlement.settlementNo
                    }
                })
        } else {
            this.configDocType.dataSource = this.documentTypes
        }
    }

    updateListFileItem() {
        this.listFileAttach.filterJob(this.jobNo);
    }

    getDocType(isADV: boolean) {
        this._systemFileManagerRepo.getDocumentType(this.transactionType)
            .subscribe(
                (res: any[]) => {
                    if (this.transactionType === 'Settlement') {
                        if (isADV) {
                            this.documentTypes = res.filter(x => x.accountingType === 'ADV-Settlement');
                            this.configDocType.dataSource = res.filter(x => x.accountingType === 'ADV-Settlement');
                        } else {
                            this.documentTypes = res.filter(x => x.accountingType === 'Settlement');
                            this.configDocType.dataSource = res.filter(x => x.accountingType === 'Settlement');
                        }
                    }
                },
            );
    }

    getJobList() {
        this.jobDataSource = [];
        this.payeeDataSource = [];
        this.invDataSource = [];
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
                                this.jobDataSource.push(item);
                            }
                            );

                            _uniqBy(data, 'payer').forEach(element => {
                                let item = ({
                                    payer: element.payer,
                                })
                                this.payeeDataSource.push(item);
                            }
                            );
                            this.configPayee.dataSource = this.payeeDataSource;
                            _uniqBy(data, 'invoiceNo').forEach(element => {
                                if (element.invoiceNo !== '') {
                                    let item = ({
                                        invoiceNo: element.invoiceNo,
                                        payer: element.payer,
                                        series: element.seriesNo,
                                        invoiceDate: moment.utc(element.invoiceDate).format('DD/MM/YYYY')
                                    })
                                    this.invDataSource.push(item);
                                }
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
                                this.jobDataSource.push(item);
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
                                this.jobDataSource.push(item);
                            }
                            );
                        }
                    }
                );
        }

    }

    chooseFile(event: any) {
        this.getJobList();
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        let docType: any;
        if (this.documentTypes.length === 1) {
            docType = this.documentTypes[0];
        }
        for (let i = 0; i < files.length; i++) {
            if (this.typeFrom == 'SOA') {
                files[i].DocumentId = 0;
                files[i].docType = 'SOA';
                files[i].aliasName = 'SOA' + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
                files[i].Code = 'SOA';
            } else if (this.typeFrom === 'Advance') {
                files[i].DocumentId = 0;
                files[i].docType = 'AD';
                files[i].aliasName = 'AD' + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
                files[i].Code = 'AD';
            }
            else if (!!docType) {
                files[i].DocumentId = docType.id;
                files[i].docType = docType;
                files[i].aliasName = docType.code + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
            }
            this.listFile.push(files[i]);
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
        this._systemFileManagerRepo.getDocumentType(this.transactionType)
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

    onSelectDataFormInfo(event: any, index: number, type: string) {
        switch (type) {
            case 'docType':
                this.enablePayeeINV[index] = false;
                this.listFile[index].Code = event.code;
                this.listFile[index].DocumentId = event.id;
                this.listFile[index].aliasName = this.isUpdate ? event.code + '_' + this.listFile[index].name : event.code + '_' + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                this.listFile[index].docType = event.code;
                if (event.code === 'INV' || event.code === 'OBH_INV') {
                    this.enablePayeeINV[index] = true;
                }
                this.listFile[index].AccountingType = event.accountingType;
                this.listFile[index].series = null;
                this.listFile[index].payee = null;
                this.listFile[index].payeeName = null;
                this.listFile[index].inv = null
                if (this.isUpdate) {
                    this.selectedDocType = event.id;
                }
                this.listFile[index].docTitle = event.nameEn;
                break;
            case 'aliasName':
                this.listFile[index].aliasName = event;
                break;
            case 'houseBill':
                this.listFile[index].hblid = event.id;
                break;
            case 'job':
                this.listFile[index].jobTitle = event.jobId;
                this.listFile[index].jobId = event.id;
                break;
            case 'note':
                this.listFile[index].note = event;
                break;
            case 'payee':
                this.listFile[index].payee = event.payer;
                this.listFile[index].aliasGenPay = true;
                this.listFile[index].inv = null;
                this.listFile[index].series = null;
                if (!this.listFile[index]?.inv || this.listFile[index]?.inv === null) {
                    this.invDataSourceUpdate[index] = this.invDataSource.filter(x => x.payer == event.payer);
                    this.payFirst[index] = true;
                }
                this.listFile[index].aliasName = this.listFile[index].Code + '_' + this.listFile[index].payee + ('_' + this.listFile[index].inv === null ? '' : this.listFile[index]?.inv);
                break;
            case 'inv':
                this.listFile[index].aliasGenPay = true;
                this.listFile[index].inv = event.invoiceNo;
                this.listFile[index].series = event.series;
                if (!this.payFirst[index]) {
                    this.listFile[index].payee = event.payer;
                    this.listFile[index].payeeName = event.payer;
                    this.listFile[index].payer = event.payer;
                }
                this.listFile[index].payee = event.payer;
                this.listFile[index].inv = event.invoiceNo;
                this.listFile[index].payeeName = event.payer;
                this.listFile[index].aliasName = this.listFile[index].Code + '_' + this.listFile[index].payee + '_' + this.listFile[index].inv;
                break;
        }
    }

    resetForm() {
        this.listFile?.splice(0, this.listFile.length);
        this.enablePayeeINV = [];
        this.payFirst = [];
        this.payFilled = true;
    }

    removeFile(index: number) {
        this.listFile?.splice(index, 1);
    }

    uploadEDoc() {
        this.payFilled = true;
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        this.isSubmitted = true;
        if (this.typeFrom === 'Settlement') {
            this.listFile.forEach(x => {
                if (x.DocumentId === null) {
                    this._toastService.warning("Please fill all field!");
                    this.payFilled = false;
                    return;
                } else if (x.Code === 'INV' || x.Code === 'OBH_INV') {
                    if (((x.payee === null || !x.payee) || ((!x.inv || x.inv === null) && x.noINV === false) || x.series === null)) {
                        this._toastService.warning("Please fill all field!");
                        this.payFilled = false;
                        return;
                    }
                }
            })
        }
        if (this.payFilled) {
            this.listFile.forEach(x => {
                files.push(x);
                edocFileList.push(({
                    JobId: this.typeFrom === 'Shipment' || this.jobOnSettle ? this.jobId : x.jobId !== undefined ? x.jobId : SystemConstants.EMPTY_GUID,
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
                    DocumentId: x.DocumentId,
                    AccountingType: x.AccountingType,
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
                    DocumentTypeId: this.selectedDocType,
                    TransactionType: this.selectedTrantype,
                }

                if (edocUploadModel.DocumentTypeId === undefined || edocUploadModel.SystemFileName === '') {
                    this._toastService.warning("Please fill all field!");
                    return;
                }
                this._systemFileManagerRepo.updateEdoc(edocUploadModel)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success("Upload file successfully!");
                                this.resetForm();
                                this.isSubmitted = false;
                            }
                        }
                    );
            } else {
                if (edocFileList.find(x => x.DocumentId === undefined || x.AliasName === '')) {

                    this._toastService.warning("Please fill all field!");
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
                                this.isSubmitted = false;
                                if (this.transactionType === "Settlement") {
                                    this._store.dispatch(LoadListEDocSettle({ transactionType: this.transactionType, billingId: this.billingId }));
                                } else {
                                    this.onSearchEdoc.emit(this.transactionType);
                                }
                            }
                        }
                    );
            }
        }
    }

    removeJob(index: number) {
        this.listFile[index].jobNo = null;
        this.listFile[index].jobId = SystemConstants.EMPTY_GUID;
        this.listFile[index].jobTitle = '';
    }

    removePayee(index: number) {
        this.removeINV(index);
    }

    removeINV(index: number) {
        this.listFile[index].series = null;
        this.listFile[index].inv = null;
        this.listFile[index].invoiceNo = null; this.listFile[index].payee = null;
        this.listFile[index].payer = null;
        this.listFile[index].payeeName = null;
        this.listFile[index].aliasName = null;
        this.payFirst[index] = false;
        this.listFile[index].payee = null;
    }

    removeDocType(index: number) {
        this.listFile[index].Code = null;
        this.listFile[index].DocumentId = null;
        this.selectedDocType = null;
        this.enablePayeeINV[index] = false;
        this.listFile[index].docTitle = '';
        this.removePayee(index);
    }

    cleanListFile() {
        this.listFile = [];
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
    DocumentId: string,
    AccountingType: string,
}
