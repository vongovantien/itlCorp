import { Component, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppList } from '@app';
import { SystemConstants } from '@constants';
import { ContextMenuDirective } from '@directives';
import { ExportRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-user-attach-file',
    templateUrl: './user-attach-file-management.component.html',
})
export class UserAttachFileManagementComponent extends AppList implements OnInit {
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;

    @Input() type: string = 'Accountant';
    listFile: any[] = [];
    documentTypes: any[] = [];
    edocs: any[] = [];

    paramsAvailable = ['Advance', 'Settlement', 'SOA'];

    module: string;
    folder: string;
    objectId: string;
    billingNo: string = '';

    isSubmitted: boolean;

    selectedFile: any;

    constructor(
        private readonly _toastService: ToastrService,
        private readonly _systemFileRepo: SystemFileManageRepo,
        private readonly _activedRoute: ActivatedRoute,
        private readonly _exportRepo: ExportRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Action', field: 'No' },
            { title: 'Alias name', field: 'No', width: 300 },
            { title: 'Real file name', field: 'No', width: 300 },
            { title: 'Type', field: 'No' },
            { title: 'Note', field: 'No' },
        ]

        this._activedRoute.queryParams
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (param: Params) => {
                    console.log(param);
                    if (this.paramsAvailable.includes(param.folder) && !!param.module && !!param.folder && !!param.objectId) {
                        this.module = param.module;
                        this.folder = param.folder;
                        this.objectId = param.objectId;
                        this.billingNo = param.billingNo;

                        this.getDocumentType(param.folder);
                        this.getEdoc(this.objectId, this.folder);
                    } else {
                        this._toastService.warning("Params invalid");
                    }
                }
            )
    }

    getDocumentType(transactionType: string) {
        this._systemFileRepo.getDocumentType(transactionType)
            .subscribe(
                (res: any[]) => {
                    this.documentTypes = res;
                },
            );
    }

    getEdoc(billingId: string, transactionType: string) {
        this._systemFileRepo.getEDocByAccountant(billingId, transactionType)
            .subscribe(
                (res: any) => {
                    this.edocs = res.eDocs || [];
                },
            );
    }

    chooseFile(e) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        let docType: any;
        if (this.documentTypes.length === 1) {
            docType = this.documentTypes[0];
        }
        for (let i = 0; i < files.length; i++) {
            if (!!docType) {
                files[i].AccountingType = docType.accountingType;
                files[i].Code = docType.code;
                files[i].DocumentId = docType.id;
                files[i].AccountingType = docType.accountingType;
                files[i].docType = docType.id;
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
        e.target.value = ''
    }

    removeFile(index: number) {
        this.listFile?.splice(index, 1);
    }

    onSelectDataFormInfo(event, index: number, type: string) {
        switch (type) {
            case 'docType':
                const selectedDocType = this.documentTypes.find(x => x.id == event);
                if (!selectedDocType) { return; }
                this.listFile[index].Code = selectedDocType.code;
                this.listFile[index].DocumentId = selectedDocType.id;
                this.listFile[index].AccountingType = selectedDocType.accountingType;
                this.listFile[index].docType = selectedDocType.id;
                this.listFile[index].AccountingType = selectedDocType.accountingType;
                this.listFile[index].aliasName = selectedDocType.code + '_' + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                break;
        }
    }

    submitUpload() {
        this.isSubmitted = true;
        if (!this.listFile.length || !this.module || !this.folder || !this.objectId) {
            return;
        }
        this.uploadEDoc();
    }

    uploadEDoc() {
        let edocFileList: any[] = [];
        let files: any[] = [];

        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: SystemConstants.EMPTY_GUID,
                Code: x.Code,
                TransactionType: '',
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: this.folder,
                HBL: SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: x.note !== undefined ? x.note : '',
                BillingId: this.objectId,
                Id: SystemConstants.EMPTY_GUID,
                DocumentId: x.DocumentId,
                AccountingType: x.AccountingType
            }));
        });

        if (!edocFileList.every(x => !!x.Code && !!x.DocumentId && !!x.AliasName)) {
            return;
        }
        const edocUploadFile = {
            ModuleName: this.module,
            FolderName: this.folder,
            Id: this.objectId,
            EDocFiles: edocFileList,
        };

        this._systemFileRepo.uploadEDoc(edocUploadFile, files, this.folder)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(`Uploaded ${this.listFile.length} files successfully`);
                        this.listFile = [];
                        this.getEdoc(this.objectId, this.folder);
                        this.isSubmitted = false;
                        return;
                    }
                    this._toastService.error(res.message);
                },
                (error) => { },
                () => {
                    this.isSubmitted = false;
                }
            );
    }

    selectFileItem(file: any) {
        this.selectedFile = file;
    }

    onSelectFileMenuContext(file: any) {
        this.selectedFile = file;
        this.clearMenuContext(this.queryListMenuContext);

    }

    viewEdocFromName(imageUrl: string) {
        this.selectedFile = Object.assign({}, this.selectedFile);
        this.selectedFile.imageUrl = imageUrl;
        this.viewFile();
    }

    viewFile() {
        if (!this.selectedFile.imageUrl) {
            return;
        }
        const extension = this.selectedFile.imageUrl.split('.').pop();
        if (['xlsx', 'docx', 'doc', 'xls'].includes(extension)) {
            this._exportRepo.previewExport(this.selectedFile.imageUrl);
        }
        else if (['html', 'htm'].includes(extension)) {
            this._systemFileRepo.getFileEdocHtml(this.selectedFile.imageUrl).subscribe(
                (res: any) => {
                    window.open('', '_blank').document.write(res.body);
                }
            )
        }
        else {
            this._exportRepo.downloadExport(this.selectedFile.imageUrl);
        }
    }

    download() {
        const selectedEdoc = Object.assign({}, this.selectedFile);
        this._systemFileRepo.getFileEdoc(selectedEdoc.sysImageId)
            .subscribe(
                (data) => {
                    const extention = selectedEdoc.imageUrl.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedEdoc.systemFileName + '.' + extention);
                }
            )
    }
}
