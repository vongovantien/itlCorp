import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppList } from '@app';
import { SystemConstants } from '@constants';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-user-attach-file',
    templateUrl: './user-attach-file-management.component.html',
})
export class UserAttachFileManagementComponent extends AppList implements OnInit {
    @Input() type: string = 'Accountant';
    listFile: any[] = [];
    documentTypes: any[] = [];

    paramsAvailable = ['Advance', 'Settlement', 'SOA'];

    module: string;
    folder: string;
    objectId: string;
    isSubmitted: boolean;

    constructor(
        private readonly _toastService: ToastrService,
        private readonly _systemFileRepo: SystemFileManageRepo,
        private readonly _activedRoute: ActivatedRoute
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

                        this.getDocumentType(param.folder, null);
                    } else {
                        this._toastService.warning("Params invalid");
                    }
                }
            )
    }

    getDocumentType(transactionType: string, billingId: string) {
        this._systemFileRepo.getDocumentType(transactionType, billingId)
            .subscribe(
                (res: any[]) => {
                    this.documentTypes = res;
                },
            );
    }

    chooseFile(e) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        let docType: any;
        if (this.documentTypes.length === 1) {
            docType = this.documentTypes[0].id;
        }
        for (let i = 0; i < files.length; i++) {
            if (!!docType) {
                files[i].Code = docType.code;
                files[i].DocumentId = docType.id;
                files[i].docType = docType;
                files[i].aliasName = docType.code + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
            } else {
                files[i].docType = null;
            }
            this.listFile.push(files[i]);
            this.listFile[i].aliasName = files[i].name.substring(0, files[i].name.lastIndexOf('.'));
        }
        console.log(this.listFile);
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
        console.log(event);
        switch (type) {
            case 'docType':
                const selectedDocType = this.documentTypes.find(x => x.id == event);
                if (!selectedDocType) { return; }
                this.listFile[index].Code = selectedDocType.code;
                this.listFile[index].DocumentId = selectedDocType.id;
                this.listFile[index].aliasName = selectedDocType.code + '_' + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                break;
        }
    }

    submitUpload() {
        this.isSubmitted = true;
        if (!this.listFile.length || !this.module || !this.folder || !this.objectId) {
            return;
        }
        console.log(this.listFile);
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
                DocumentId: x.DocumentId
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
}
