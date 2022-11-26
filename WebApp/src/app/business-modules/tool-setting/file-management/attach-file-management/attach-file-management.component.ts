import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppList } from '@app';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-attach',
    templateUrl: './attach-file-management.component.html',
})
export class AttachFileManagementComponent extends AppList implements OnInit {
    @Input() type: string = 'Accountant';
    listFile: any[] = [];
    documentTypes: any[] = [];

    paramsAvailable = ['Advance', 'Settlement', 'SOA'];


    constructor(
        private readonly _toastService: ToastrService,
        private readonly _systemFileRepo: SystemFileManageRepo,
        private readonly _activedRoute: ActivatedRoute
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'No', field: 'No' },
            { title: 'Alias name', field: 'No' },
            { title: 'Real file name', field: 'No' },
            { title: 'Type', field: 'No' },
            { title: 'Job', field: 'No' },
            { title: 'HBL', field: 'No' },
            { title: 'Note', field: 'No' },
        ]

        this._activedRoute.queryParams
            .subscribe(
                (param: Params) => {
                    console.log(param);
                    if (this.paramsAvailable.includes(param.type)) {
                        this.getDocumentType(param.type, null);
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
            this.listFile[i].aliasName = files[i].name.substring(0, files[i].name.lastIndexOf('.'));
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
}
